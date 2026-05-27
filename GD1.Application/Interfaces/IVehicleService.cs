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
        Task<List<VehicleLookupDto>> SearchAsync(string term, string? brand = null, string? category = null);
        Task<(double Length, double Width, double Height)> GetDimensionsAsync(string brand, string model, string type);
        Task<GD1.Application.Features.Vehicle.DTOs.VehicleLookupDto?> DecodeVinAsync(string vin);
        Task<(bool IsValid, string Category)> ValidateVehicleYearAsync(string brand, string model, int year);
    }

}
