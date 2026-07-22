using GD1.Application.Common;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using System.Linq;
using System.Collections.Generic;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class CreateBookingResponse
    {
        public long BookingId { get; set; }
        public long AgreementId { get; set; }
        public string AgreementContent { get; set; } = string.Empty;
    }

    public class CreateBookingCommand : IRequest<BaseResponse<CreateBookingResponse>>
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

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BaseResponse<CreateBookingResponse>>
    {
        private readonly IGenericRepository<Booking> _repo;
        private readonly IGenericRepository<GD1.Domain.Entities.Agreement> _agreementRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;

        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IGenericRepository<StoredVehicle> _storedVehicleRepo;
        private readonly INotificationService _notificationService;

        public CreateBookingCommandHandler(
            IGenericRepository<Booking> repo,
            IGenericRepository<GD1.Domain.Entities.Agreement> agreementRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IGenericRepository<StoredVehicle> storedVehicleRepo,
            INotificationService notificationService)
        {
            _repo = repo;
            _agreementRepo = agreementRepo;
            _propertyRepo = propertyRepo;
            _slotRepo = slotRepo;
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _storedVehicleRepo = storedVehicleRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<CreateBookingResponse>> Handle(CreateBookingCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

            // Booking will be created in a temporary state until agreement is signed

            var property = await _propertyRepo.GetByIdAsync(req.PropertyId);
            if (property == null) return BaseResponse<CreateBookingResponse>.Fail("Selected property not found.");

            var vehicle = await _vehicleRepo.GetByIdAsync(req.VehicleId);
            if (vehicle == null) return BaseResponse<CreateBookingResponse>.Fail("Vehicle not found.");

            // Check if property has any slot that fits the vehicle
            var allSlots = await _slotRepo.FindAsync(s => s.PropertyId == property.Id);
            bool hasCompatibleSlot = allSlots.Any(s => 
                s.SquareFeet >= (vehicle.LengthFeet * vehicle.WidthFeet) && 
                s.HeightFeet >= vehicle.HeightFeet);
            
            if (!hasCompatibleSlot)
                return BaseResponse<CreateBookingResponse>.Fail("The selected property does not have any slots large enough for your vehicle.");

            // ── Clean up any stale AwaitingAgreement bookings for this vehicle by this user ──
            // These are ghost bookings where the user never signed or declined the agreement.
            var staleAwaitingBookings = await _repo.FindAsync(b =>
                b.VehicleId == req.VehicleId &&
                b.OwnerId == cmd.OwnerId &&
                b.Status == BookingStatus.AwaitingAgreement);

            foreach (var stale in staleAwaitingBookings)
            {
                // Also cancel any associated pending agreements
                var staleAgreements = await _agreementRepo.FindAsync(a =>
                    a.ReferenceId == stale.Id &&
                    a.Type == AgreementType.LotBooking &&
                    a.Status == AgreementStatus.Pending);

                foreach (var staleAgreement in staleAgreements)
                {
                    staleAgreement.Status = AgreementStatus.Rejected;
                    await _agreementRepo.UpdateAsync(staleAgreement);
                }

                stale.Status = BookingStatus.AgreementDeclined;
                await _repo.UpdateAsync(stale);
            }

            // Check for Vehicle Overlap — exclude AwaitingAgreement since those are not yet confirmed
            var vehicleBookings = await _repo.FindAsync(b => 
                b.VehicleId == req.VehicleId && 
                b.Status != BookingStatus.Cancelled && 
                b.Status != BookingStatus.AgreementDeclined && 
                b.Status != BookingStatus.AdminRejected && 
                b.Status != BookingStatus.AwaitingAgreement &&
                b.Status != BookingStatus.Completed);
                
            bool hasVehicleOverlap = vehicleBookings.Any(b => 
                req.StartDate < b.EndDate && req.EndDate > b.StartDate);
                
            if (hasVehicleOverlap)
                return BaseResponse<CreateBookingResponse>.Fail("This vehicle already has a confirmed booking during the selected dates.");

            // Verify Slot availability
            if (req.SlotId.HasValue)
            {
                var slot = await _slotRepo.GetByIdAsync(req.SlotId.Value);
                if (slot == null || slot.PropertyId != req.PropertyId)
                    return BaseResponse<CreateBookingResponse>.Fail("Selected garage (slot) is invalid for this property.");

                // Check specifically if THIS slot fits the vehicle
                if (slot.SquareFeet < (vehicle.LengthFeet * vehicle.WidthFeet) || slot.HeightFeet < vehicle.HeightFeet)
                {
                    return BaseResponse<CreateBookingResponse>.Fail("The specifically selected garage is not large enough for your vehicle.");
                }

                // Check for Slot Overlap — exclude AwaitingAgreement (not yet confirmed)
                var slotBookings = await _repo.FindAsync(b => 
                    b.SlotId == req.SlotId.Value && 
                    b.Status != BookingStatus.Cancelled && 
                    b.Status != BookingStatus.AgreementDeclined && 
                    b.Status != BookingStatus.AdminRejected && 
                    b.Status != BookingStatus.AwaitingAgreement &&
                    b.Status != BookingStatus.Pending &&
                    b.Status != BookingStatus.Completed);

                bool hasSlotOverlap = slotBookings.Any(b => 
                    req.StartDate < b.EndDate && req.EndDate > b.StartDate);
                    
                if (hasSlotOverlap)
                    return BaseResponse<CreateBookingResponse>.Fail("The selected garage is already booked during the selected dates.");
                    
                // STRICTLY VALIDATE IF SLOT IS CURRENTLY OCCUPIED
                if (slot.IsOccupied)
                {
                    var activeStored = await _storedVehicleRepo.FindAsync(sv => sv.SlotId == req.SlotId.Value && sv.IsActive);
                    var currentStored = activeStored.FirstOrDefault();
                    if (currentStored != null)
                    {
                        if (req.StartDate <= currentStored.ExpiryDate)
                        {
                            return BaseResponse<CreateBookingResponse>.Fail($"This slot is currently occupied. You can only book it for dates after {currentStored.ExpiryDate:dd MMM yyyy}.");
                        }
                    }
                }
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
                Status = BookingStatus.PendingVerification,
                IsPickupRequested = false,
                IsAgreementSigned = 0
            };

            await _repo.AddAsync(booking);

            var user = await _userRepo.GetByIdAsync(cmd.OwnerId);
            if (user == null) return BaseResponse<CreateBookingResponse>.Fail("User not found.");

            var response = new CreateBookingResponse
            {
                BookingId = booking.Id,
                AgreementId = 0,
                AgreementContent = string.Empty
            };

            // Notify Lot Owner
            try
            {
                await _notificationService.SendAsync(
                    userId: property.LotOwnerId,
                    title: "Booking pending verification",
                    body: $"A new booking requires your verification for {vehicle.Brand} {vehicle.Model} at {property.Name}.",
                    actionType: "ViewBookings",
                    referenceId: booking.Id,
                    actionUrl: "/lot-owner/bookings");
            }
            catch { /* Ignore notification failure */ }

            return BaseResponse<CreateBookingResponse>.Ok(response, "Booking request submitted. Awaiting Lot Admin verification.");
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
