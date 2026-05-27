using GD1.Application.Common;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class SearchVehicleQuery : IRequest<BaseResponse<List<VehicleLookupDto>>>
    {
        public string? SearchTerm { get; set; }
        public string? SelectedBrand { get; set; }
        public string? Category { get; set; }
    }
    
    public class SearchVehicleHandler : IRequestHandler<SearchVehicleQuery, BaseResponse<List<VehicleLookupDto>>>
    {
        private readonly IVehicleService _vehicleService;
        
        public SearchVehicleHandler(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<BaseResponse<List<VehicleLookupDto>>> Handle(SearchVehicleQuery request, CancellationToken ct)
        {
            var term = request.SearchTerm == "null" ? null : request.SearchTerm;
            var brand = request.SelectedBrand == "null" ? null : request.SelectedBrand;
            var category = request.Category == "null" ? null : request.Category;

            var data = await _vehicleService.SearchAsync(term ?? "", brand, category);

            return BaseResponse<List<VehicleLookupDto>>.Ok(data);
        }
    }
}
