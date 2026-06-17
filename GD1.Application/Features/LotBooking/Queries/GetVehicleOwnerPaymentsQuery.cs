using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class VehicleOwnerPaymentDto
    {
        public long BookingId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class VehicleOwnerPaymentsResultDto
    {
        public List<VehicleOwnerPaymentDto> Pending { get; set; } = new();
        public List<VehicleOwnerPaymentDto> Upcoming { get; set; } = new();
        public List<VehicleOwnerPaymentDto> Paid { get; set; } = new();
    }

    public class GetVehicleOwnerPaymentsQuery : IRequest<BaseResponse<VehicleOwnerPaymentsResultDto>>
    {
        public long OwnerId { get; set; }
    }

    public class GetVehicleOwnerPaymentsQueryHandler : IRequestHandler<GetVehicleOwnerPaymentsQuery, BaseResponse<VehicleOwnerPaymentsResultDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Payment> _paymentRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;

        public GetVehicleOwnerPaymentsQueryHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.Payment> paymentRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _serviceRequestRepo = serviceRequestRepo;
        }

        public async Task<BaseResponse<VehicleOwnerPaymentsResultDto>> Handle(GetVehicleOwnerPaymentsQuery request, CancellationToken cancellationToken)
        {
            var dtoList = new List<VehicleOwnerPaymentDto>();
            var now = DateTime.UtcNow;

            // Get bookings with vehicle and property info
            var bookings = await _bookingRepo.FindAsync(
                b => b.OwnerId == request.OwnerId,
                "Vehicle",
                "Property",
                "JourneyEvents"
            );

            foreach (var booking in bookings)
            {
                var vehicleName = $"{booking.Vehicle?.Brand} {booking.Vehicle?.Model}";
                var registration = booking.Vehicle?.RegistrationNo ?? "";
                var propertyName = booking.Property?.Name ?? "";

                // Get payment record for this booking
                var payments = await _paymentRepo.FindAsync(p => p.BookingId == booking.Id);
                var paidPayments = payments.Where(p => p.Status == "paid").ToList();
                var unpaidPayments = payments.Where(p => p.Status != "paid").ToList();

                // ---- PAID PAYMENTS (Lot Booking + Pickup) ----
                foreach (var p in paidPayments)
                {
                    // Lot Booking payment
                    var lotAmount = p.TotalAmount - p.PickupChargeAmount;
                    if (lotAmount > 0)
                    {
                        dtoList.Add(new VehicleOwnerPaymentDto
                        {
                            BookingId = booking.Id,
                            VehicleName = vehicleName,
                            VehicleRegistration = registration,
                            PropertyName = propertyName,
                            Amount = lotAmount,
                            Date = p.CreatedAt,
                            Status = "Paid",
                            Type = "Lot Booking"
                        });
                    }

                    // Pickup charge (if any)
                    if (p.PickupChargeAmount > 0)
                    {
                        dtoList.Add(new VehicleOwnerPaymentDto
                        {
                            BookingId = booking.Id,
                            VehicleName = vehicleName,
                            VehicleRegistration = registration,
                            PropertyName = propertyName,
                            Amount = p.PickupChargeAmount,
                            Date = p.CreatedAt,
                            Status = "Paid",
                            Type = "Pickup Charge"
                        });
                    }
                }

                // ---- PENDING UNPAID PAYMENTS ----
                foreach (var p in unpaidPayments)
                {
                    var lotAmount = p.TotalAmount - p.PickupChargeAmount;
                    if (lotAmount > 0)
                    {
                        dtoList.Add(new VehicleOwnerPaymentDto
                        {
                            BookingId = booking.Id,
                            VehicleName = vehicleName,
                            VehicleRegistration = registration,
                            PropertyName = propertyName,
                            Amount = lotAmount,
                            Date = p.CreatedAt,
                            Status = "Pending",
                            Type = "Lot Booking"
                        });
                    }

                    if (p.PickupChargeAmount > 0)
                    {
                        dtoList.Add(new VehicleOwnerPaymentDto
                        {
                            BookingId = booking.Id,
                            VehicleName = vehicleName,
                            VehicleRegistration = registration,
                            PropertyName = propertyName,
                            Amount = p.PickupChargeAmount,
                            Date = p.CreatedAt,
                            Status = "Pending",
                            Type = "Pickup Charge"
                        });
                    }
                }

                // ---- STORAGE CYCLES (upcoming/due recurring payments) ----
                var storedEvent = booking.JourneyEvents?.FirstOrDefault(e => e.EventType == "VehicleStored");
                if (storedEvent != null && paidPayments.Any())
                {
                    var storedDate = storedEvent.CreatedAt;
                    var daysStored = (int)(now - storedDate).TotalDays;
                    if (daysStored < 0) daysStored = 0;

                    var advancePaid = paidPayments.Sum(p => p.TotalAmount - p.AdminCutAmount - p.PickupChargeAmount);
                    int totalCycles = (daysStored / 30) + 1;

                    for (int i = 1; i <= totalCycles; i++)
                    {
                        var cycleStart = storedDate.AddDays((i - 1) * 30);
                        var dueDate = storedDate.AddDays(i * 30);
                        if (booking.EndDate > storedDate && dueDate > booking.EndDate)
                            dueDate = booking.EndDate;

                        var cycleDays = (int)Math.Ceiling((dueDate - cycleStart).TotalDays);
                        if (cycleDays <= 0) cycleDays = 1;

                        decimal cycleCost = cycleDays * booking.PricePerDay;
                        decimal deductible = Math.Min(cycleCost, advancePaid);
                        advancePaid -= deductible;
                        decimal amountDue = cycleCost - deductible;

                        if (amountDue > 0)
                        {
                            dtoList.Add(new VehicleOwnerPaymentDto
                            {
                                BookingId = booking.Id,
                                VehicleName = vehicleName,
                                VehicleRegistration = registration,
                                PropertyName = propertyName,
                                Amount = amountDue,
                                Date = dueDate,
                                Status = now >= dueDate ? "Pending" : "Upcoming",
                                Type = $"Storage Cycle {i}"
                            });
                        }
                    }
                }
            }

            // ---- SERVICE PAYMENTS ----
            var bookingIds = bookings.Select(b => b.Id).ToList();
            var serviceRequests = await _serviceRequestRepo.FindAsync(
                sr => bookingIds.Contains(sr.BookingId),
                "ServiceCenter",
                "Booking",
                "Booking.Vehicle"
            );

            foreach (var sr in serviceRequests)
            {
                var serviceAmount = sr.Amount > 0 ? sr.Amount : (sr.ServiceCost + sr.PlatformFee);
                if (serviceAmount <= 0) continue;

                var vName = $"{sr.Booking?.Vehicle?.Brand} {sr.Booking?.Vehicle?.Model}";
                var vReg = sr.Booking?.Vehicle?.RegistrationNo ?? "";

                string srStatus;
                if (sr.IsPaid)
                    srStatus = "Paid";
                else if (sr.Status == "Payment" || sr.Status == "Service Completed")
                    srStatus = "Pending";
                else
                    srStatus = "Upcoming";

                dtoList.Add(new VehicleOwnerPaymentDto
                {
                    BookingId = sr.BookingId,
                    VehicleName = vName,
                    VehicleRegistration = vReg,
                    PropertyName = sr.ServiceCenter?.Name ?? "Service Center",
                    Amount = serviceAmount,
                    Date = sr.UpdatedAt,
                    Status = srStatus,
                    Type = "Service Payment"
                });
            }

            var result = new VehicleOwnerPaymentsResultDto
            {
                Pending = dtoList.Where(x => x.Status == "Pending").OrderByDescending(x => x.Date).ToList(),
                Upcoming = dtoList.Where(x => x.Status == "Upcoming").OrderBy(x => x.Date).ToList(),
                Paid = dtoList.Where(x => x.Status == "Paid").OrderByDescending(x => x.Date).ToList(),
            };

            return BaseResponse<VehicleOwnerPaymentsResultDto>.Ok(result, "Payments fetched successfully.");
        }
    }
}
