using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace GD1.Application.Features.Pickup.Commands
{
    public record VerifyOtpCommand(
        long PickupRequestId,
        string Otp
    ) : IRequest<GD1.Application.Common.BaseResponse<string>>;

    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.Otp).NotEmpty();
        }
    }

    public class VerifyOtpCommandHandler
        : IRequestHandler<VerifyOtpCommand, GD1.Application.Common.BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IOtpService _otp;

        public VerifyOtpCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IOtpService otp)
        {
            _pickupRepo = pickupRepo;
            _otp = otp;
        }

        public async Task<GD1.Application.Common.BaseResponse<string>> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            if (!_otp.VerifyOtp(request.Otp, pickup.OtpHash!))
                throw new Exception("Invalid OTP. The code is either incorrect or has expired.");

            pickup.IsOtpVerified = true;
            pickup.Status = PickupStatus.VehiclePicked;
            
            // Secure Cleanup
            pickup.OtpHash = null;
            pickup.OwnerSubmittedOtp = null;
            pickup.OtpExpiry = null;

            await _pickupRepo.UpdateAsync(pickup);

            return GD1.Application.Common.BaseResponse<string>.Ok("Handover verified successfully. You may now start the ride to the lot.");
        }
    }
}
