using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace GD1.Application.Features.Pickup.Commands
{
    public record SubmitLotArrivalConditionCommand(
        long PickupRequestId,
        string FrontImageUrl,
        string RearImageUrl,
        string LeftSideImageUrl,
        string RightSideImageUrl,
        string InteriorImageUrl,
        string OdometerImageUrl
    ) : IRequest<BaseResponse<string>>;

    public class SubmitLotArrivalConditionCommandValidator : AbstractValidator<SubmitLotArrivalConditionCommand>
    {
        public SubmitLotArrivalConditionCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.FrontImageUrl).NotEmpty();
            RuleFor(x => x.RearImageUrl).NotEmpty();
            RuleFor(x => x.LeftSideImageUrl).NotEmpty();
            RuleFor(x => x.RightSideImageUrl).NotEmpty();
            RuleFor(x => x.InteriorImageUrl).NotEmpty();
            RuleFor(x => x.OdometerImageUrl).NotEmpty();
        }
    }

    public class SubmitLotArrivalConditionCommandHandler : IRequestHandler<SubmitLotArrivalConditionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IEmailService _email;
        private readonly IGeminiService _gemini;

        public SubmitLotArrivalConditionCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<PickupVerification> verificationRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IEmailService email,
            IGeminiService gemini)
        {
            _pickupRepo = pickupRepo;
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _verificationRepo = verificationRepo;
            _journeyRepo = journeyRepo;
            _email = email;
            _gemini = gemini;
        }

        public async Task<BaseResponse<string>> Handle(SubmitLotArrivalConditionCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null) throw new Exception("Pickup request not found");

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking == null) throw new Exception("Booking not found");

            if (pickup.Status != PickupStatus.InTransit)
                throw new Exception($"Cannot submit a lot arrival condition report while the pickup is in the '{pickup.Status}' status. Expected 'InTransit'.");

            var frontTask = _gemini.VerifyImageReadabilityAsync(request.FrontImageUrl, "Car Exterior Front");
            var rearTask = _gemini.VerifyImageReadabilityAsync(request.RearImageUrl, "Car Exterior Rear");
            var leftTask = _gemini.VerifyImageReadabilityAsync(request.LeftSideImageUrl, "Car Exterior Left Side");
            var rightTask = _gemini.VerifyImageReadabilityAsync(request.RightSideImageUrl, "Car Exterior Right Side");
            var interiorTask = _gemini.VerifyImageReadabilityAsync(request.InteriorImageUrl, "Car Interior");
            var odometerTask = _gemini.VerifyImageReadabilityAsync(request.OdometerImageUrl, "Odometer Dashboard");

            await Task.WhenAll(frontTask, rearTask, leftTask, rightTask, interiorTask, odometerTask);

            if (!frontTask.Result.IsReadable || frontTask.Result.ConfidenceScore < 80) throw new Exception($"Front Image: {frontTask.Result.Reason}");
            if (!rearTask.Result.IsReadable || rearTask.Result.ConfidenceScore < 80) throw new Exception($"Rear Image: {rearTask.Result.Reason}");
            if (!leftTask.Result.IsReadable || leftTask.Result.ConfidenceScore < 80) throw new Exception($"Left Image: {leftTask.Result.Reason}");
            if (!rightTask.Result.IsReadable || rightTask.Result.ConfidenceScore < 80) throw new Exception($"Right Image: {rightTask.Result.Reason}");
            if (!interiorTask.Result.IsReadable || interiorTask.Result.ConfidenceScore < 80) throw new Exception($"Interior Image: {interiorTask.Result.Reason}");
            if (!odometerTask.Result.IsReadable || odometerTask.Result.ConfidenceScore < 80) throw new Exception($"Odometer Image: {odometerTask.Result.Reason}");

            var verification = new PickupVerification
            {
                BookingId = pickup.BookingId,
                ManagerId = pickup.ManagerId ?? 0,
                Type = ReportType.LotArrival,
                FrontImageUrl = request.FrontImageUrl,
                RearImageUrl = request.RearImageUrl,
                LeftSideImageUrl = request.LeftSideImageUrl,
                RightSideImageUrl = request.RightSideImageUrl,
                InteriorImageUrl = request.InteriorImageUrl,
                OdometerImageUrl = request.OdometerImageUrl,
                VerifiedAt = DateTime.UtcNow
            };
            await _verificationRepo.AddAsync(verification);

            var journeyEvent = new VehicleJourneyEvent
            {
                VehicleId = booking.VehicleId,
                BookingId = booking.Id,
                EventType = "VehicleStored",
                Description = "Vehicle has arrived at the storage lot and is now safely stored.",
                TriggeredBy = pickup.ManagerId,
                Images = new List<VehicleImage>
                {
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.FrontImageUrl, Label = "Front" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.RearImageUrl, Label = "Rear" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.LeftSideImageUrl, Label = "LeftSide" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.RightSideImageUrl, Label = "RightSide" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.InteriorImageUrl, Label = "Interior" },
                    new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.OdometerImageUrl, Label = "Odometer" }
                }
            };
            await _journeyRepo.AddAsync(journeyEvent);

            pickup.Status = PickupStatus.Stored;
            await _pickupRepo.UpdateAsync(pickup);

            booking.Status = BookingStatus.InLot;
            booking.StartDate = DateTime.UtcNow; // Record the actual start date!
            await _bookingRepo.UpdateAsync(booking);

            var owner = await _userRepo.GetByIdAsync(booking.OwnerId);
            if (owner != null && !string.IsNullOrEmpty(owner.Email))
            {
                var body = $@"
                    <h3>Your Vehicle is Safely Stored!</h3>
                    <p>Your vehicle has successfully arrived at the storage lot. Please review the arrival condition report images below:</p>
                    <ul>
                        <li><a href='{request.FrontImageUrl}'>Front</a></li>
                        <li><a href='{request.RearImageUrl}'>Rear</a></li>
                        <li><a href='{request.LeftSideImageUrl}'>Left Side</a></li>
                        <li><a href='{request.RightSideImageUrl}'>Right Side</a></li>
                        <li><a href='{request.InteriorImageUrl}'>Interior</a></li>
                        <li><a href='{request.OdometerImageUrl}'>Odometer</a></li>
                    </ul>
                    <p>Thank you for trusting GD1 Auto Depot!</p>
                ";
                await _email.SendAsync(owner.Email, "GD1 Vehicle Safely Stored", body);
            }

            return BaseResponse<string>.Ok("Vehicle successfully stored at the lot. Condition report saved.");
        }
    }
}
