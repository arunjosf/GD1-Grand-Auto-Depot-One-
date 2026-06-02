using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Queries
{
    public class GetMyServiceRequestsQuery : IRequest<BaseResponse<IEnumerable<MyServiceRequestDto>>>
    {
        /// <summary>The vehicle owner's user ID.</summary>
        public long OwnerId { get; set; }

        /// <summary>Optional: filter by vehicle ID.</summary>
        public long? VehicleId { get; set; }
    }

    public class MyServiceRequestDto
    {
        public long Id { get; set; }
        public long BookingId { get; set; }

        // Vehicle
        public long VehicleId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;

        // Storage lot
        public long? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyCity { get; set; }

        // Service center
        public long ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; } = string.Empty;
        public string ServiceCenterPhone { get; set; } = string.Empty;
        public string ServiceCenterCity { get; set; } = string.Empty;

        // Request details
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        public decimal ServiceCost { get; set; }
        public string? BillUrl { get; set; }
        public string? CompletionNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetMyServiceRequestsQueryHandler
        : IRequestHandler<GetMyServiceRequestsQuery, BaseResponse<IEnumerable<MyServiceRequestDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;

        public GetMyServiceRequestsQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo)
        {
            _requestRepo = requestRepo;
            _centerRepo = centerRepo;
        }

        public async Task<BaseResponse<IEnumerable<MyServiceRequestDto>>> Handle(
            GetMyServiceRequestsQuery request, CancellationToken ct)
        {
            // Load service requests where the booking belongs to this owner
            var allRequests = await _requestRepo.FindAsync(
                r => r.RequestedBy == request.OwnerId,
                "Booking.Vehicle", "Booking.Property", "ServiceCenter");

            var query = allRequests.AsEnumerable();

            if (request.VehicleId.HasValue)
                query = query.Where(r => r.Booking?.VehicleId == request.VehicleId.Value);

            var dtos = query.Select(r => new MyServiceRequestDto
            {
                Id                    = r.Id,
                BookingId             = r.BookingId,
                VehicleId             = r.Booking?.VehicleId ?? 0,
                VehicleBrand          = r.Booking?.Vehicle?.Brand ?? string.Empty,
                VehicleModel          = r.Booking?.Vehicle?.Model ?? string.Empty,
                VehicleRegistrationNo = r.Booking?.Vehicle?.RegistrationNo ?? string.Empty,
                PropertyId            = r.Booking?.PropertyId,
                PropertyName          = r.Booking?.Property?.Name,
                PropertyCity          = r.Booking?.Property?.City,
                ServiceCenterId       = r.ServiceCenterId,
                ServiceCenterName     = r.ServiceCenter?.Name ?? string.Empty,
                ServiceCenterPhone    = r.ServiceCenter?.PhoneNumber ?? string.Empty,
                ServiceCenterCity     = r.ServiceCenter?.City ?? string.Empty,
                ServiceType           = r.ServiceType,
                Notes                 = r.Notes,
                ScheduledDate         = r.ScheduledDate,
                Status                = r.Status,
                CancellationReason    = r.CancellationReason,
                ServiceCost           = r.ServiceCost,
                BillUrl               = r.BillUrl,
                CompletionNotes       = r.CompletionNotes,
                CreatedAt             = r.CreatedAt
            })
            .OrderByDescending(x => x.Id)
            .ToList();

            return BaseResponse<IEnumerable<MyServiceRequestDto>>.Ok(dtos);
        }
    }
}
