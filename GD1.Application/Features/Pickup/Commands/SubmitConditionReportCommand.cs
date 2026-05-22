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

namespace GD1.Application.Features.Pickup.Commands
{
    public record SubmitConditionReportCommand(
        long PickupRequestId,
        string FrontImageUrl,
        string RearImageUrl,
        string LeftSideImageUrl,
        string RightSideImageUrl,
        string? SelfieUrl
    ) : IRequest<BaseResponse<string>>;

    public class SubmitConditionReportCommandValidator : AbstractValidator<SubmitConditionReportCommand>
    {
        public SubmitConditionReportCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.FrontImageUrl).NotEmpty();
            RuleFor(x => x.RearImageUrl).NotEmpty();
            RuleFor(x => x.LeftSideImageUrl).NotEmpty();
            RuleFor(x => x.RightSideImageUrl).NotEmpty();
        }
    }

    public class SubmitConditionReportCommandHandler : IRequestHandler<SubmitConditionReportCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IOtpService _otp;
        private readonly ISmsService _sms;
        private readonly IEmailService _email;
        private readonly IGeminiService _gemini;

        public SubmitConditionReportCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<PickupVerification> verificationRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IOtpService otp,
            ISmsService sms,
            IEmailService email,
            IGeminiService gemini)
        {
            _pickupRepo = pickupRepo;
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _verificationRepo = verificationRepo;
            _journeyRepo = journeyRepo;
            _otp = otp;
            _sms = sms;
            _email = email;
            _gemini = gemini;
        }

        public async Task<BaseResponse<string>> Handle(SubmitConditionReportCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null) throw new Exception("Pickup request not found");

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking == null) throw new Exception("Booking not found");

            if (pickup.Status == PickupStatus.Assigned || pickup.Status == PickupStatus.Approved || pickup.Status == PickupStatus.ManagerScheduled || pickup.Status == PickupStatus.OtpSent)
            {
                // -- PICKUP PHASE (Send OTP) --
                if (string.IsNullOrEmpty(request.SelfieUrl))
                    throw new Exception("Manager Selfie is required for the initial pickup condition report.");

                var frontTask = _gemini.VerifyImageReadabilityAsync(request.FrontImageUrl, "Car Exterior Front");
                var rearTask = _gemini.VerifyImageReadabilityAsync(request.RearImageUrl, "Car Exterior Rear");
                var leftTask = _gemini.VerifyImageReadabilityAsync(request.LeftSideImageUrl, "Car Exterior Left Side");
                var rightTask = _gemini.VerifyImageReadabilityAsync(request.RightSideImageUrl, "Car Exterior Right Side");
                var selfieTask = _gemini.VerifyImageReadabilityAsync(request.SelfieUrl, "Manager Selfie");

                await Task.WhenAll(frontTask, rearTask, leftTask, rightTask, selfieTask);

                if (!frontTask.Result.IsReadable || frontTask.Result.ConfidenceScore < 80) throw new Exception($"Front Image: {frontTask.Result.Reason}");
                if (!rearTask.Result.IsReadable || rearTask.Result.ConfidenceScore < 80) throw new Exception($"Rear Image: {rearTask.Result.Reason}");
                if (!leftTask.Result.IsReadable || leftTask.Result.ConfidenceScore < 80) throw new Exception($"Left Image: {leftTask.Result.Reason}");
                if (!rightTask.Result.IsReadable || rightTask.Result.ConfidenceScore < 80) throw new Exception($"Right Image: {rightTask.Result.Reason}");
                if (!selfieTask.Result.IsReadable || selfieTask.Result.ConfidenceScore < 80) throw new Exception($"Selfie Image: {selfieTask.Result.Reason}");

                var owner = await _userRepo.GetByIdAsync(booking.OwnerId);
                var otp = _otp.GenerateOtp();

                pickup.OtpHash = _otp.HashOtp(otp);
                pickup.OtpExpiry = _otp.GetExpiry();
                pickup.Status = PickupStatus.OtpSent;

                await _pickupRepo.UpdateAsync(pickup);

                var verification = new PickupVerification
                {
                    BookingId = pickup.BookingId,
                    ManagerId = pickup.ManagerId ?? 0,
                    Type = ReportType.Pickup,
                    FrontImageUrl = request.FrontImageUrl,
                    RearImageUrl = request.RearImageUrl,
                    LeftSideImageUrl = request.LeftSideImageUrl,
                    RightSideImageUrl = request.RightSideImageUrl,
                    SelfieUrl = request.SelfieUrl,
                    VerifiedAt = DateTime.UtcNow
                };
                await _verificationRepo.AddAsync(verification);

                var journeyEvent = new VehicleJourneyEvent
                {
                    VehicleId = booking.VehicleId,
                    BookingId = booking.Id,
                    EventType = "VehiclePickedUp",
                    Description = "Manager has arrived at the pickup location and captured the initial condition report.",
                    TriggeredBy = pickup.ManagerId,
                    Images = new List<VehicleImage>
                    {
                        new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.FrontImageUrl, Label = "Front" },
                        new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.RearImageUrl, Label = "Rear" },
                        new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.LeftSideImageUrl, Label = "LeftSide" },
                        new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.RightSideImageUrl, Label = "RightSide" },
                        new VehicleImage { VehicleId = booking.VehicleId, ImageUrl = request.SelfieUrl, Label = "ManagerSelfie" }
                    }
                };
                await _journeyRepo.AddAsync(journeyEvent);

                if (!string.IsNullOrEmpty(owner.Email))
                {
                    var body = $@"
                        <h3>Your Manager has arrived!</h3>
                        <p>Please review the condition report images below:</p>
                        <ul>
                            <li><a href='{request.FrontImageUrl}'>Front</a></li>
                            <li><a href='{request.RearImageUrl}'>Rear</a></li>
                            <li><a href='{request.LeftSideImageUrl}'>Left Side</a></li>
                            <li><a href='{request.RightSideImageUrl}'>Right Side</a></li>
                            <li><a href='{request.SelfieUrl}'>Manager Selfie</a></li>
                        </ul>
                        <p>If you approve of this handover, your OTP is: <strong>{otp}</strong></p>
                        <p>Submit this OTP in your GD1 Vehicle Owner app to authorize the pickup.</p>
                    ";
                    await _email.SendAsync(owner.Email, "GD1 Vehicle Pickup Verification", body);
                }
                
                if (!string.IsNullOrEmpty(owner.PhoneNumber))
                    await _sms.SendAsync(owner.PhoneNumber, $"Your GD1 pickup OTP is {otp}");

                return BaseResponse<string>.Ok("Pickup condition report saved. OTP sent successfully to the vehicle owner.");
            }
            else
            {
                throw new Exception($"Cannot submit a condition report while the pickup is in the '{pickup.Status}' status. Expected a pickup phase status.");
            }
        }
    }
}
