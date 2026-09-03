using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface IGroupRepository
{
    Task<long> CreateAsync(string name, string? description, long currencyId, long ownerId);
    Task<IEnumerable<Group>> GetAllAsync(long userId);
    Task<Group?> GetByIdAsync(long id, long userId);
    Task<string?> GetRoleAsync(long groupId, long userId);
    Task<bool> IsMemberAsync(long groupId, long userId);
    Task<int> UpdateAsync(long id, long ownerId, string name, string? description);
    Task<int> ArchiveAsync(long id, long ownerId);
    Task<IEnumerable<GroupMember>> GetMembersAsync(long groupId);
    Task<long?> FindUserIdByEmailAsync(string email);
    Task<string?> GetMemberStatusAsync(long groupId, long userId);
    Task<int> InviteExistingUserAsync(long groupId, long userId);
    Task<long> CreatePendingInvitationAsync(long groupId, string email, string token, DateTime expiresAt);
    Task<int> AcceptInvitationAsync(long groupId, long userId);
    Task<int> LeaveGroupAsync(long groupId, long userId);
    Task<decimal> GetUserUnsettledAsync(long groupId, long userId);
}
