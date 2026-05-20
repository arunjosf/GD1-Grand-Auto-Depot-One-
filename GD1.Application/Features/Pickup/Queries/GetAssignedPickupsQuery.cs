using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Queries
{
    public class GetAssignedPickupsQuery : IRequest<BaseResponse<IEnumerable<AssignedPickupDto>>>
    {
        public long ManagerId { get; set; }
    }

    public class AssignedPickupDto
    {
        public long PickupRequestId { get; set; }
        public long BookingId { get; set; }
        public DateTime RequestedPickupTime { get; set; }
        public string Status { get; set; } = string.Empty;

        // Vehicle Info
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        // Location Info
        public string? PickupAddress { get; set; }
        public string? PickupPincode { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
    }

    public class GetAssignedPickupsQueryHandler : IRequestHandler<GetAssignedPickupsQuery, BaseResponse<IEnumerable<AssignedPickupDto>>>
    {
        private readonly IPickupReadRepository _repo;

        public GetAssignedPickupsQueryHandler(IPickupReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<AssignedPickupDto>>> Handle(
            GetAssignedPickupsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetAssignedPickupsAsync(request.ManagerId);
            return BaseResponse<IEnumerable<AssignedPickupDto>>.Ok(result);
        }
    }
}
