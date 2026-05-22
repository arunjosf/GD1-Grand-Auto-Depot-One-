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
    public record SubmitOwnerOtpCommand(
        long PickupRequestId,
        string Otp
    ) : IRequest<BaseResponse<string>>;

    public class SubmitOwnerOtpCommandValidator : AbstractValidator<SubmitOwnerOtpCommand>
    {
        public SubmitOwnerOtpCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.Otp).NotEmpty();
        }
    }

    public class SubmitOwnerOtpCommandHandler : IRequestHandler<SubmitOwnerOtpCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IOtpService _otp;

        public SubmitOwnerOtpCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IOtpService otp)
        {
            _pickupRepo = pickupRepo;
            _otp = otp;
        }

        public async Task<BaseResponse<string>> Handle(SubmitOwnerOtpCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null)
                throw new Exception("Pickup request not found");

            if (!_otp.VerifyOtp(request.Otp, pickup.OtpHash!))
                throw new Exception("Invalid OTP. The vehicle owner submitted an incorrect code.");

            pickup.OwnerSubmittedOtp = request.Otp;
            pickup.Status = PickupStatus.OwnerOtpSubmitted;

            await _pickupRepo.UpdateAsync(pickup);

            return BaseResponse<string>.Ok("Owner OTP successfully submitted. Waiting for Manager confirmation.");
        }
    }
}
