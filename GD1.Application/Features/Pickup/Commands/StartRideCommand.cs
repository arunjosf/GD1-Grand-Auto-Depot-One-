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

namespace GD1.Application.Features.Pickup.Commands
{
    public record StartRideCommand(
        long PickupRequestId,
        string InteriorImageUrl,
        string OdometerImageUrl,
        string Description = ""
    ) : IRequest<BaseResponse<string>>;

    public class StartRideCommandValidator : AbstractValidator<StartRideCommand>
    {
        public StartRideCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.InteriorImageUrl).NotEmpty();
            RuleFor(x => x.OdometerImageUrl).NotEmpty();
            RuleFor(x => x.Description).NotEmpty().WithMessage("Condition description is required.");
        }
    }

    public class StartRideCommandHandler : IRequestHandler<StartRideCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;
        private readonly IGeminiService _gemini;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IEmailService _email;

        public StartRideCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<PickupVerification> verificationRepo,
            IGeminiService gemini,
            IGenericRepository<User> userRepo,
            IEmailService email)
        {
            _pickupRepo = pickupRepo;
            _journeyRepo = journeyRepo;
            _bookingRepo = bookingRepo;
            _verificationRepo = verificationRepo;
            _gemini = gemini;
            _userRepo = userRepo;
            _email = email;
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

            // Verify Images with Gemini
            var interiorTask = _gemini.VerifyImageReadabilityAsync(request.InteriorImageUrl, "Car Interior");
            var odometerTask = _gemini.VerifyImageReadabilityAsync(request.OdometerImageUrl, "Odometer Reading");

            await Task.WhenAll(interiorTask, odometerTask);

            if (!interiorTask.Result.IsReadable || interiorTask.Result.ConfidenceScore < 80)
                throw new Exception($"Interior Image Error: {interiorTask.Result.Reason}. Please capture a clearer photo.");
            
            if (!odometerTask.Result.IsReadable || odometerTask.Result.ConfidenceScore < 80)
                throw new Exception($"Odometer Image Error: {odometerTask.Result.Reason}. Please capture a clearer photo.");

            // Add journey event
            var journeyEvent = new VehicleJourneyEvent
            {
                VehicleId = booking.VehicleId,
                BookingId = pickup.BookingId,
                EventType = "RideStarted",
                Description = $"Ride started: {request.Description}",
                TriggeredBy = pickup.ManagerId,
                Images = new List<VehicleImage>
                {
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.InteriorImageUrl, Label = "Interior" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.OdometerImageUrl, Label = "Odometer" }
                }
            };

            await _journeyRepo.AddAsync(journeyEvent);

            var verifications = await _verificationRepo.FindAsync(v => v.BookingId == pickup.BookingId && v.Type == ReportType.Pickup);
            var pickupVerification = System.Linq.Enumerable.FirstOrDefault(verifications);
            if (pickupVerification != null)
            {
                pickupVerification.InteriorImageUrl = request.InteriorImageUrl;
                pickupVerification.OdometerImageUrl = request.OdometerImageUrl;
                await _verificationRepo.UpdateAsync(pickupVerification);
            }

            pickup.Status = PickupStatus.InTransit;
            await _pickupRepo.UpdateAsync(pickup);

            // Send Email to Vehicle Owner
            var owner = await _userRepo.GetByIdAsync(booking.OwnerId);
            if (owner != null)
            {
                string subject = "Your Ride Has Started";
                string body = $"Hello {owner.FullName},\n\nYour vehicle is now securely in transit to the storage lot. You can track its location in real-time through the application.\n\nDescription: {request.Description}\n\nThank you for using Grand Auto Depot One!";
                await _email.SendAsync(owner.Email, subject, body);
            }

            return BaseResponse<string>.Ok("Ride started successfully. Drive safely to the storage lot.");
        }
    }
}
