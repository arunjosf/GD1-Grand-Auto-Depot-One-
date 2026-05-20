using GD1.Application.Features.Pickup.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IPickupReadRepository
    {
        Task<IEnumerable<AssignedPickupDto>> GetAssignedPickupsAsync(long managerId);
    }
}
