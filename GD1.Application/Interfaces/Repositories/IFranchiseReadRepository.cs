using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.GD1Admin.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IFranchiseReadRepository
    {
        Task<ApplicationDto?> GetByIdAsync(long id, long applicantId);
        Task<IEnumerable<ApplicationDto>> GetByApplicantIdAsync(long applicantId);
        Task<IEnumerable<ApplicationDto>> GetAllApplicationsAsync(string? status);
        Task<IEnumerable<PendingAgentDto>> GetPendingAgentsAsync();
        Task<IEnumerable<UserListDto>> GetNearbyAgentsAsync(double lat, double lon);
        Task<IEnumerable<UserListDto>> GetAllAgentsAsync(bool verifiedOnly, string? city, string? state);
        Task<IEnumerable<ApplicationDto>> GetAgentAssignedApplicationsAsync(long agentId);
    }
}
