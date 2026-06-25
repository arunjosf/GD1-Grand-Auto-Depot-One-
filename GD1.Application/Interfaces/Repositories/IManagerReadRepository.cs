using GD1.Application.Features.LotManager.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IManagerReadRepository
    {
        Task<ManagerDashboardMetricsDto> GetDashboardMetricsAsync(long managerId);
        Task<IEnumerable<ManagerPickupDto>> GetPickupsAsync(long managerId, bool isCompleted);
        Task<IEnumerable<GD1.Application.Features.LotManager.Queries.SelfDropDto>> GetSelfDropsAsync(long managerId, bool isCompleted);
        Task<IEnumerable<ManagerVehicleDto>> GetVehiclesAsync(long managerId);
        Task<ManagerVehicleDetailDto> GetVehicleDetailAsync(long managerId, long vehicleId, long? bookingId = null);
    }
}
