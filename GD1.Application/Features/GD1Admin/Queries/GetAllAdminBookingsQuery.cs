using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllAdminBookingsQuery : IRequest<BaseResponse<AdminBookingsDto>>
    {
    }

    public class AdminBookingsDto
    {
        public List<AdminGarageBookingDto> GarageBookings { get; set; } = new();
        public List<AdminServiceBookingDto> ServiceBookings { get; set; } = new();
    }

    public class AdminGarageBookingDto
    {
        public long BookingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyCity { get; set; } = string.Empty;
        
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;

        // Payment Info
        public string PaymentStatus { get; set; } = "Unpaid";
        public decimal AmountPaid { get; set; }
    }

    public class AdminServiceBookingDto
    {
        public long ServiceRequestId { get; set; }
        public long BookingId { get; set; }
        
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;

        public string ServiceCenterName { get; set; } = string.Empty;
        public string ServiceCenterCity { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }

        // Payment Info
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
    }

    public class GetAllAdminBookingsQueryHandler : IRequestHandler<GetAllAdminBookingsQuery, BaseResponse<AdminBookingsDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Payment> _paymentRepo;

        public GetAllAdminBookingsQueryHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo,
            IGenericRepository<GD1.Domain.Entities.Payment> paymentRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRequestRepo = serviceRequestRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<BaseResponse<AdminBookingsDto>> Handle(GetAllAdminBookingsQuery request, CancellationToken cancellationToken)
        {
            var garageBookings = await _bookingRepo.FindAsync(x => !x.IsDeleted, "Vehicle", "Property");
            var serviceRequests = await _serviceRequestRepo.FindAsync(x => !x.IsDeleted, "Booking.Vehicle", "ServiceCenter");
            var payments = await _paymentRepo.GetAllAsync();

            var dto = new AdminBookingsDto();

            dto.GarageBookings = garageBookings.Select(b => {
                var payment = payments.FirstOrDefault(p => p.BookingId == b.Id && p.Status == "paid");
                return new AdminGarageBookingDto
                {
                    BookingId = b.Id,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    PricePerDay = b.PricePerDay,
                    TotalCost = b.TotalCost,
                    Status = b.Status.ToString(),
                    PropertyName = b.Property?.Name ?? "",
                    PropertyCity = b.Property?.City ?? "",
                    VehicleBrand = b.Vehicle?.Brand ?? "",
                    VehicleModel = b.Vehicle?.Model ?? "",
                    VehicleRegistrationNo = b.Vehicle?.RegistrationNo ?? "",
                    PaymentStatus = payment != null ? "Paid" : "Unpaid",
                    AmountPaid = payment?.TotalAmount ?? 0
                };
            }).OrderByDescending(x => x.StartDate).ToList();

            dto.ServiceBookings = serviceRequests.Select(sr => new AdminServiceBookingDto
            {
                ServiceRequestId = sr.Id,
                BookingId = sr.BookingId,
                VehicleBrand = sr.Booking?.Vehicle?.Brand ?? "",
                VehicleModel = sr.Booking?.Vehicle?.Model ?? "",
                VehicleRegistrationNo = sr.Booking?.Vehicle?.RegistrationNo ?? "",
                ServiceCenterName = sr.ServiceCenter?.Name ?? "",
                ServiceCenterCity = sr.ServiceCenter?.City ?? "",
                ServiceType = sr.ServiceType,
                Status = sr.Status,
                ScheduledDate = sr.ScheduledDate,
                IsPaid = sr.IsPaid,
                Amount = sr.Amount
            }).OrderByDescending(x => x.ScheduledDate).ToList();

            return BaseResponse<AdminBookingsDto>.Ok(dto, "Success");
        }
    }
}
