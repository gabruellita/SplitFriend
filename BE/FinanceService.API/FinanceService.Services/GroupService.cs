using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;

namespace FinanceService.Services;

public class GroupService(
    IGroupRepository    groupRepo,
    ICurrencyRepository currencyRepo,
    ICurrentUser        currentUser,
    FinanceService.Infrastructure.Notifications.INotificationClient notifications
) : IGroupService
{
    private static readonly TimeSpan InvitationLifetime = TimeSpan.FromDays(7);

    public async Task<IEnumerable<GroupResponse>> GetAllAsync()
    {
        var list = await groupRepo.GetAllAsync(currentUser.UserId);
        return list.Select(MapToResponse);
    }

    public async Task<GroupResponse> GetByIdAsync(long id)
    {
        var g = await groupRepo.GetByIdAsync(id, currentUser.UserId)
            ?? throw new NotFoundException("Grupul nu exista sau nu esti membru.");
        return MapToResponse(g);
    }

    public async Task<long> CreateAsync(CreateGroupRequest request)
    {
        if (!await currencyRepo.ExistsActiveAsync(request.CurrencyId))
            throw new ValidationException("Moneda selectata nu exista sau este inactiva.");

        return await groupRepo.CreateAsync(
            request.Name.Trim(), request.Description?.Trim(), request.CurrencyId, currentUser.UserId);
    }

    public async Task UpdateAsync(long id, UpdateGroupRequest request)
    {
        await EnsureOwnerAsync(id);
        var rows = await groupRepo.UpdateAsync(id, currentUser.UserId, request.Name.Trim(), request.Description?.Trim());
        if (rows == 0)
            throw new NotFoundException("Grupul nu exista sau nu esti owner.");
    }

    public async Task ArchiveAsync(long id)
    {
        await EnsureOwnerAsync(id);
        var rows = await groupRepo.ArchiveAsync(id, currentUser.UserId);
        if (rows == 0)
            throw new NotFoundException("Grupul nu exista sau nu esti owner.");
    }

    public async Task<IEnumerable<GroupMemberResponse>> GetMembersAsync(long id)
    {
        await EnsureMemberAsync(id);
        var members = await groupRepo.GetMembersAsync(id);
        return members.Select(m => new GroupMemberResponse(
            m.UserId, m.Email, m.Username, m.FirstName, m.LastName, m.Role, m.Status, m.JoinedAt));
    }

    public async Task<InviteResponse> InviteAsync(long groupId, InviteMemberRequest request)
    {
        await EnsureMemberAsync(groupId);   // orice membru activ poate invita

        // Numele grupului (pentru email) il citim o singura data, inainte de a scrie invitatia,
        // ca un esec aici sa nu lase invitatia scrisa in DB dar apelantul sa primeasca 500.
        var group     = await groupRepo.GetByIdAsync(groupId, currentUser.UserId);
        var groupName = group?.Name ?? "grup";

        var email   = request.Email.Trim().ToLowerInvariant();
        var existing = await groupRepo.FindUserIdByEmailAsync(email);

        if (existing is long userId)
        {
            var status = await groupRepo.GetMemberStatusAsync(groupId, userId);
            if (status is "ACTIVE")  throw new ConflictException("Utilizatorul este deja membru al grupului.");
            if (status is "INVITED") throw new ConflictException("Utilizatorul are deja o invitatie in asteptare.");

            await groupRepo.InviteExistingUserAsync(groupId, userId);
            // Deep-link direct la grup cu ?invite=1: frontend-ul cheama /accept la incarcare,
            // flip-uind membership-ul INVITED → ACTIVE intr-un singur click din mail.
            await notifications.SendGroupInviteAsync(email, groupName, $"http://localhost:5173/app/groups/{groupId}?invite=1");
            return new InviteResponse("INVITED_EXISTING");
        }

        var token     = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTime.UtcNow.Add(InvitationLifetime);
        await groupRepo.CreatePendingInvitationAsync(groupId, email, token, expiresAt);
        var registerLink = $"http://localhost:5173/register?invite={Uri.EscapeDataString(token)}";
        await notifications.SendGroupInviteAsync(email, groupName, registerLink);
        return new InviteResponse("PENDING_EMAIL");
    }

    public async Task AcceptAsync(long groupId)
    {
        var rows = await groupRepo.AcceptInvitationAsync(groupId, currentUser.UserId);
        if (rows == 0)
        {
            // Idempotent: daca esti deja membru ACTIVE (dublu-apel din StrictMode in dev,
            // refresh sau re-click pe link-ul din mail), tratam accept-ul ca succes — nu mai
            // aruncam 404. Doar daca n-ai nicio invitatie/membership semnalam eroarea.
            var status = await groupRepo.GetMemberStatusAsync(groupId, currentUser.UserId);
            if (status is "ACTIVE") return;
            throw new NotFoundException("Nu ai o invitatie in asteptare pentru acest grup.");
        }
    }

    public async Task LeaveAsync(long groupId)
    {
        var unsettled = await groupRepo.GetUserUnsettledAsync(groupId, currentUser.UserId);
        if (unsettled > 0)
            throw new ConflictException($"Nu poti parasi grupul: ai un sold neachitat de {unsettled:0.00}.");

        var rows = await groupRepo.LeaveGroupAsync(groupId, currentUser.UserId);
        if (rows == 0)
            throw new ConflictException("Nu poti parasi grupul (nu esti membru activ sau esti owner).");
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    /// <summary>Verifica ca userul curent e membru ACTIVE al grupului; altfel 403.</summary>
    private async Task EnsureMemberAsync(long groupId)
    {
        if (!await groupRepo.IsMemberAsync(groupId, currentUser.UserId))
            throw new ForbiddenException("Nu esti membru al acestui grup.");
    }

    /// <summary>Verifica ca userul curent e OWNER al grupului; altfel 403.</summary>
    private async Task EnsureOwnerAsync(long groupId)
    {
        var role = await groupRepo.GetRoleAsync(groupId, currentUser.UserId);
        if (role != "OWNER")
            throw new ForbiddenException("Doar owner-ul grupului poate face aceasta operatie.");
    }

    private static GroupResponse MapToResponse(Group g)
        => new(g.Id, g.Name, g.Description, g.CurrencyId, g.CurrencyCode,
               g.OwnerUserId, g.Status, g.MemberCount, g.MyRole, g.CreatedAt);
}
