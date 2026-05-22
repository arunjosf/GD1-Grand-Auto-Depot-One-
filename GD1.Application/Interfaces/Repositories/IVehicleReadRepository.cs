using GD1.Application.Features.Vehicle.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IVehicleReadRepository
    {
        Task<IEnumerable<VehicleDto>> GetByOwnerIdAsync(long ownerId, long? vehicleId = null);
    }
}
