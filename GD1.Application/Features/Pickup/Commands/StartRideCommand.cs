using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Services;

namespace GD1.Application.Features.Pickup.Commands
{
    public record StartRideCommand(
        long PickupRequestId,
        string Description = ""
    ) : IRequest<BaseResponse<string>>;

    public class StartRideCommandValidator : AbstractValidator<StartRideCommand>
    {
        public StartRideCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
        }
    }

    public class StartRideCommandHandler : IRequestHandler<StartRideCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IEmailService _email;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;

        public StartRideCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<User> userRepo,
            IEmailService email,
            INotificationService notificationService,
            IGenericRepository<PickupVerification> verificationRepo)
        {
            _pickupRepo = pickupRepo;
            _journeyRepo = journeyRepo;
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _email = email;
            _notificationService = notificationService;
            _verificationRepo = verificationRepo;
        }

        public async Task<BaseResponse<string>> Handle(StartRideCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null)
                throw new Exception("Pickup request not found");

            if (pickup.Status != PickupStatus.VehiclePicked)
                throw new Exception("You must verify the OTP and complete the handover before starting the ride.");

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking == null) throw new Exception("Booking not found");

            // Enforce pre-ride condition photos exist
            var verifications = await _verificationRepo.FindAsync(
                v => v.BookingId == pickup.BookingId && v.Type == ReportType.Pickup);
            var verification = verifications.OrderByDescending(v => v.Id).FirstOrDefault();
            
            if (verification == null || string.IsNullOrWhiteSpace(verification.InteriorImageUrl) || string.IsNullOrWhiteSpace(verification.OdometerImageUrl))
            {
                throw new Exception("You must submit the pre-ride condition report (interior and odometer photos) before starting the ride.");
            }

            // Add journey event
            var journeyEvent = new VehicleJourneyEvent
            {
                VehicleId = booking.VehicleId,
                BookingId = pickup.BookingId,
                EventType = "RideStarted",
                Description = string.IsNullOrWhiteSpace(request.Description) ? "Transit started to garage." : $"Ride started: {request.Description}",
                TriggeredBy = pickup.ManagerId,
                Images = new List<VehicleImage>() // Removed redundancy with VehicleImages table
            };

            await _journeyRepo.AddAsync(journeyEvent);

            pickup.Status = PickupStatus.InTransit;
            await _pickupRepo.UpdateAsync(pickup);

            // Send Email to Vehicle Owner
            var owner = await _userRepo.GetByIdAsync(booking.OwnerId);
            if (owner != null)
            {
                string subject = "Your Ride Has Started";
                string body = $"Hello {owner.FullName},\n\nYour vehicle is now securely in transit to the storage lot. You can track its location in real-time through the application.\n\nThank you for using Grand Auto Depot One!";
                await _email.SendAsync(owner.Email, subject, body);
            }

            try
            {
                await _notificationService.SendAsync(
                    userId: booking.OwnerId,
                    title: "Ride Started",
                    body: "Your vehicle is now in transit to the storage lot.",
                    actionType: "TrackBooking",
                    referenceId: booking.Id);
            }
            catch { /* Ignore */ }

            return BaseResponse<string>.Ok("Ride started successfully. Drive safely to the storage lot.");
        }
    }
}
