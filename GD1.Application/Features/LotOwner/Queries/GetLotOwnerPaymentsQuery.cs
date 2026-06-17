using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotOwner.Queries
{
    public class LotOwnerPaymentDto
    {
        public long? PaymentId { get; set; }
        public long BookingId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Upcoming", "Paid"
        public string Type { get; set; } = string.Empty;
    }

    public class LotOwnerPaymentsResultDto
    {
        public List<LotOwnerPaymentDto> Pending { get; set; } = new();
        public List<LotOwnerPaymentDto> Upcoming { get; set; } = new();
        public List<LotOwnerPaymentDto> Paid { get; set; } = new();
    }

    public class GetLotOwnerPaymentsQuery : IRequest<BaseResponse<LotOwnerPaymentsResultDto>>
    {
        public long LotOwnerId { get; set; }
    }

    public class GetLotOwnerPaymentsQueryHandler : IRequestHandler<GetLotOwnerPaymentsQuery, BaseResponse<LotOwnerPaymentsResultDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Payment> _paymentRepo;

        public GetLotOwnerPaymentsQueryHandler(IGenericRepository<Booking> bookingRepo, IGenericRepository<GD1.Domain.Entities.Payment> paymentRepo)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<BaseResponse<LotOwnerPaymentsResultDto>> Handle(GetLotOwnerPaymentsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepo.FindAsync(
                b => b.Property.LotOwnerId == request.LotOwnerId, 
                "Property", 
                "Vehicle", 
                "Vehicle.Owner", 
                "JourneyEvents"
            );

            var dtoList = new List<LotOwnerPaymentDto>();

            foreach (var booking in bookings)
            {
                var vehicleName = $"{booking.Vehicle?.Brand} {booking.Vehicle?.Model}";
                var ownerName = booking.Vehicle?.Owner?.FullName ?? "Unknown";
                var ownerPhone = booking.Vehicle?.Owner?.PhoneNumber ?? "Unknown";

                var payments = await _paymentRepo.FindAsync(p => p.BookingId == booking.Id);

                var paidPayments = payments.Where(p => p.Status == "paid").ToList();
                var unpaidPayments = payments.Where(p => p.Status != "paid").ToList();

                // 1. Paid Payments
                foreach (var p in paidPayments)
                {
                    dtoList.Add(new LotOwnerPaymentDto
                    {
                        PaymentId = p.Id,
                        BookingId = booking.Id,
                        VehicleName = vehicleName,
                        VehicleRegistration = booking.Vehicle?.RegistrationNo ?? "",
                        OwnerName = ownerName,
                        OwnerPhone = ownerPhone,
                        Amount = p.PropertyOwnerAmount + p.PickupChargeAmount,
                        Date = p.CreatedAt,
                        Status = "Paid",
                        Type = "Advance & Pickup"
                    });
                }

                // 2. Pending Unpaid Advance Payments
                foreach (var p in unpaidPayments)
                {
                    dtoList.Add(new LotOwnerPaymentDto
                    {
                        PaymentId = p.Id,
                        BookingId = booking.Id,
                        VehicleName = vehicleName,
                        VehicleRegistration = booking.Vehicle?.RegistrationNo ?? "",
                        OwnerName = ownerName,
                        OwnerPhone = ownerPhone,
                        Amount = p.PropertyOwnerAmount + p.PickupChargeAmount,
                        Date = p.CreatedAt,
                        Status = "Pending",
                        Type = "Advance & Pickup"
                    });
                }

                // 3. Storage Cycles
                var storedEvent = booking.JourneyEvents.FirstOrDefault(e => e.EventType == "VehicleStored");
                if (storedEvent != null)
                {
                    var storedDate = storedEvent.CreatedAt;
                    var daysStored = (DateTime.UtcNow - storedDate).Days;
                    if (daysStored < 0) daysStored = 0;
                    
                    var storageAdvancePaid = paidPayments.Sum(p => p.TotalAmount - p.AdminCutAmount - p.PickupChargeAmount);

                    int totalCycles = (daysStored / 30) + 1;

                    for (int i = 1; i <= totalCycles; i++)
                    {
                        var cycleStart = storedDate.AddDays((i - 1) * 30);
                        var expectedDueDate = storedDate.AddDays(i * 30);
                        var actualDueDate = expectedDueDate;
                        if (booking.EndDate > storedDate && expectedDueDate > booking.EndDate)
                        {
                            actualDueDate = booking.EndDate;
                        }
                        
                        var cycleDays = (int)Math.Ceiling((actualDueDate - cycleStart).TotalDays);
                        if (cycleDays <= 0) cycleDays = 1; // Minimum 1 day cost
                        
                        decimal cycleCost = cycleDays * booking.PricePerDay;

                        decimal deductible = Math.Min(cycleCost, storageAdvancePaid);
                        storageAdvancePaid -= deductible;
                        decimal amountDue = cycleCost - deductible;

                        if (amountDue >= 0)
                        {
                            if (DateTime.UtcNow >= actualDueDate)
                            {
                                dtoList.Add(new LotOwnerPaymentDto
                                {
                                    BookingId = booking.Id,
                                    VehicleName = vehicleName,
                                    VehicleRegistration = booking.Vehicle?.RegistrationNo ?? "",
                                    OwnerName = ownerName,
                                    OwnerPhone = ownerPhone,
                                    Amount = amountDue,
                                    Date = actualDueDate,
                                    Status = "Pending",
                                    Type = $"Storage Cycle {i}"
                                });
                            }
                            else
                            {
                                dtoList.Add(new LotOwnerPaymentDto
                                {
                                    BookingId = booking.Id,
                                    VehicleName = vehicleName,
                                    VehicleRegistration = booking.Vehicle?.RegistrationNo ?? "",
                                    OwnerName = ownerName,
                                    OwnerPhone = ownerPhone,
                                    Amount = amountDue,
                                    Date = actualDueDate,
                                    Status = "Upcoming",
                                    Type = $"Storage Cycle {i}"
                                });
                            }
                        }
                    }
                }
            }

            var result = new LotOwnerPaymentsResultDto
            {
                Pending = dtoList.Where(x => x.Status == "Pending").OrderByDescending(x => x.Date).ToList(),
                Upcoming = dtoList.Where(x => x.Status == "Upcoming").OrderBy(x => x.Date).ToList(),
                Paid = dtoList.Where(x => x.Status == "Paid").OrderByDescending(x => x.Date).ToList(),
            };

            return BaseResponse<LotOwnerPaymentsResultDto>.Ok(result, "Payments fetched successfully.");
        }
    }
}
