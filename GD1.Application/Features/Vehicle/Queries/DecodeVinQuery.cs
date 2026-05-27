using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class DecodeVinQuery : IRequest<VehicleLookupDto?>
    {
        public string Vin { get; set; } = string.Empty;
    }

    public class DecodeVinQueryHandler : IRequestHandler<DecodeVinQuery, VehicleLookupDto?>
    {
        private readonly IVehicleService _vehicleService;

        public DecodeVinQueryHandler(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<VehicleLookupDto?> Handle(DecodeVinQuery request, CancellationToken cancellationToken)
        {
            return await _vehicleService.DecodeVinAsync(request.Vin);
        }
    }
}
