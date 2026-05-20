using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using System.Linq;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class CreateBookingCommand : IRequest<BaseResponse<long>>
    {
        public CreateBookingRequest Request { get; set; } = null!;
        public long OwnerId { get; set; }
    }

    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.OwnerId).GreaterThan(0);
            RuleFor(x => x.Request.VehicleId).GreaterThan(0);
            RuleFor(x => x.Request.PropertyId).GreaterThan(0);
            RuleFor(x => x.Request.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past.");
            RuleFor(x => x.Request.EndDate).GreaterThan(x => x.Request.StartDate).WithMessage("End date must be after start date.");
        }
    }

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<Booking> _repo;
        private readonly IGenericRepository<BookingAgreement> _agreementRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;

        public CreateBookingCommandHandler(
            IGenericRepository<Booking> repo,
            IGenericRepository<BookingAgreement> agreementRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo)
        {
            _repo = repo;
            _agreementRepo = agreementRepo;
            _propertyRepo = propertyRepo;
            _slotRepo = slotRepo;
            _vehicleRepo = vehicleRepo;
        }

        public async Task<BaseResponse<long>> Handle(CreateBookingCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

            // Booking will be created in a temporary state until agreement is signed

            var property = await _propertyRepo.GetByIdAsync(req.PropertyId);
            if (property == null) return BaseResponse<long>.Fail("Selected property not found.");

            var vehicle = await _vehicleRepo.GetByIdAsync(req.VehicleId);
            if (vehicle == null) return BaseResponse<long>.Fail("Vehicle not found.");

            // Check if property has any slot that fits the vehicle
            var allSlots = await _slotRepo.FindAsync(s => s.PropertyId == property.Id);
            bool hasCompatibleSlot = allSlots.Any(s => 
                s.SquareFeet >= (vehicle.LengthFeet * vehicle.WidthFeet) && 
                s.HeightFeet >= vehicle.HeightFeet);
            
            if (!hasCompatibleSlot)
                return BaseResponse<long>.Fail("The selected property does not have any slots large enough for your vehicle.");

            // Check for Vehicle Overlap
            var vehicleBookings = await _repo.FindAsync(b => 
                b.VehicleId == req.VehicleId && 
                b.Status != BookingStatus.Cancelled && 
                b.Status != BookingStatus.AgreementDeclined && 
                b.Status != BookingStatus.Completed);
                
            bool hasVehicleOverlap = vehicleBookings.Any(b => 
                req.StartDate < b.EndDate && req.EndDate > b.StartDate);
                
            if (hasVehicleOverlap)
                return BaseResponse<long>.Fail("This vehicle is already booked during the selected dates.");

            // Verify Slot availability
            if (req.SlotId.HasValue)
            {
                var slot = await _slotRepo.GetByIdAsync(req.SlotId.Value);
                if (slot == null || slot.PropertyId != req.PropertyId)
                    return BaseResponse<long>.Fail("Selected garage (slot) is invalid for this property.");

                // Check for Slot Overlap
                var slotBookings = await _repo.FindAsync(b => 
                    b.SlotId == req.SlotId.Value && 
                    b.Status != BookingStatus.Cancelled && 
                    b.Status != BookingStatus.AgreementDeclined && 
                    b.Status != BookingStatus.Completed);

                bool hasSlotOverlap = slotBookings.Any(b => 
                    req.StartDate < b.EndDate && req.EndDate > b.StartDate);
                    
                if (hasSlotOverlap)
                    return BaseResponse<long>.Fail("The selected garage is already booked during the selected dates.");
            }

            int days = Math.Max(1, (req.EndDate - req.StartDate).Days);
            decimal totalCost = property.PricePerDay * days;

            // Create and save the Booking
            var booking = new Booking
            {
                VehicleId = req.VehicleId,
                PropertyId = req.PropertyId,
                SlotId = req.SlotId,
                OwnerId = cmd.OwnerId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                PricePerDay = property.PricePerDay,
                TotalCost = totalCost,
                Status = BookingStatus.AwaitingAgreement,
                IsPickupRequested = false,
                IsAgreementSigned = false
            };

            await _repo.AddAsync(booking);

            return BaseResponse<long>.Ok(booking.Id, "Temporary booking created. Please generate and accept the agreement.");
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371d;
            var dLat = (lat2 - lat1) * (Math.PI / 180d);
            var dLon = (lon2 - lon1) * (Math.PI / 180d);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * (Math.PI / 180d)) * Math.Cos(lat2 * (Math.PI / 180d)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        }
    }
}
