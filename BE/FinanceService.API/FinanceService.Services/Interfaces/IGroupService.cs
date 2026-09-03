using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<GroupResponse>> GetAllAsync();
    Task<GroupResponse> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateGroupRequest request);
    Task UpdateAsync(long id, UpdateGroupRequest request);
    Task ArchiveAsync(long id);
    Task<IEnumerable<GroupMemberResponse>> GetMembersAsync(long id);
    Task<InviteResponse> InviteAsync(long groupId, InviteMemberRequest request);
    Task AcceptAsync(long groupId);
    Task LeaveAsync(long groupId);
}
