using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetServiceCenterPaymentsQuery : IRequest<BaseResponse<List<SCPaymentDto>>>
    {
        public long AdminId { get; set; }
    }

    public class SCPaymentDto
    {
        public long ServiceRequestId { get; set; }
        public long BookingId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal CenterEarning { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }

    public class GetServiceCenterPaymentsQueryHandler : IRequestHandler<GetServiceCenterPaymentsQuery, BaseResponse<List<SCPaymentDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;

        public GetServiceCenterPaymentsQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo)
        {
            _scRepo = scRepo;
            _requestRepo = requestRepo;
        }

        public async Task<BaseResponse<List<SCPaymentDto>>> Handle(GetServiceCenterPaymentsQuery request, CancellationToken cancellationToken)
        {
            var centers = await _scRepo.FindAsync(x => x.AdminId == request.AdminId);
            var sc = centers.FirstOrDefault();
            if (sc == null) return BaseResponse<List<SCPaymentDto>>.Fail("Service center not found");

            // Fetch requests that are either paid or completed
            var allRequests = await _requestRepo.FindAsync(x => x.ServiceCenterId == sc.Id && (x.IsPaid || x.IsCompleted == true), "Booking.Vehicle");

            var dtos = allRequests.Select(pr => new SCPaymentDto
            {
                ServiceRequestId = pr.Id,
                BookingId = pr.BookingId,
                VehicleBrand = pr.Booking?.Vehicle?.Brand ?? "",
                VehicleModel = pr.Booking?.Vehicle?.Model ?? "",
                VehicleRegistrationNo = pr.Booking?.Vehicle?.RegistrationNo ?? "",
                Date = pr.UpdatedAt,
                Amount = pr.Amount,
                CenterEarning = pr.CenterEarning,
                ServiceType = pr.ServiceType,
                Status = pr.Status,
                IsPaid = pr.IsPaid
            }).OrderByDescending(x => x.Date).ToList();

            return BaseResponse<List<SCPaymentDto>>.Ok(dtos, "Success");
        }
    }
}
