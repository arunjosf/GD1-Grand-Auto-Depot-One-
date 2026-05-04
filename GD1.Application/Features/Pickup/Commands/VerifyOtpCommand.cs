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

namespace GD1.Application.Features.Pickup.Commands
{
    public record VerifyOtpCommand(
        long PickupRequestId,
        string Otp
    ) : IRequest<string>;

    public class VerifyOtpCommandHandler
        : IRequestHandler<VerifyOtpCommand, string>
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

        public async Task<string> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            if (!_otp.VerifyOtp(request.Otp, pickup.OtpHash!))
                throw new Exception("Invalid OTP");

            pickup.IsOtpVerified = true;
            pickup.Status = PickupStatus.Verified;

            await _pickupRepo.UpdateAsync(pickup);

            return "OTP verified";
        }
    }
}
