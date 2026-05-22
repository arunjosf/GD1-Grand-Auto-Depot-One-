using GD1.Application.Features.Pickup.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IPickupReadRepository
    {
        Task<IEnumerable<PickupRequestDto>> GetPropertyPickupsAsync(long propertyId, long? managerId);
        Task<IEnumerable<PickupRequestDto>> GetMyAssignmentsAsync(long managerUserId);
    }
}
