using GD1.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.FranchiseApplication.DTOs;


namespace GD1.Application.Interfaces.Repositories
{
    public interface IFranchiseReadRepository
    {
        Task<ApplicationDto?> GetByIdAsync(long applicationId, long applicantId);
        Task<IEnumerable<ApplicationDto>> GetByApplicantIdAsync(long applicantId);
        Task<IEnumerable<ApplicationDto>> GetAllPendingAsync();
        Task<IEnumerable<LotUnit>> GetLotUnitsByApplicationIdAsync(long applicationId);
        Task<InspectionReport?> GetReportByIdAsync(long reportId);
        Task<IEnumerable<InspectionReport>> GetReportsByApplicationIdAsync(long applicationId);
        Task<IEnumerable<ApplicationDto>> GetAllApplicationsAsync(GD1.Domain.Entities.Enums.FranchiseStatus? status, string? searchTerm, string? sortBy, bool descending);
        Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetAllAgentsAsync(bool onlyActive, string? city, string? state);
        Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetNearbyAgentsAsync(double lat, double lon);
        Task<IEnumerable<ApplicationDto>> GetAgentAssignedApplicationsAsync(long agentId);
    }
}
