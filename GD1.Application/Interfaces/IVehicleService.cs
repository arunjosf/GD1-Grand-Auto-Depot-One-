using GD1.Application.Features.Vehicle.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<List<VehicleLookupDto>> SearchAsync(string term, string? brand = null);
    }

}
