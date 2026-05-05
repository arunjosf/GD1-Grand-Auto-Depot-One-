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
    public record SendOtpCommand(long PickupRequestId) : IRequest<string>;

    public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
    {
        public SendOtpCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
        }
    }

    public class SendOtpCommandHandler
        : IRequestHandler<SendOtpCommand, string>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IOtpService _otp;
        private readonly ISmsService _sms;

        public SendOtpCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<User> userRepo,
            IOtpService otp,
            ISmsService sms)
        {
            _pickupRepo = pickupRepo;
            _userRepo = userRepo;
            _otp = otp;
            _sms = sms;
        }

        public async Task<string> Handle(
            SendOtpCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            var owner = await _userRepo.GetByIdAsync(pickup.Booking.OwnerId);

            var otp = _otp.GenerateOtp();

            pickup.OtpHash = _otp.HashOtp(otp);
            pickup.OtpExpiry = _otp.GetExpiry();
            pickup.Status = PickupStatus.OtpSent;

            await _pickupRepo.UpdateAsync(pickup);

            await _sms.SendAsync(owner.PhoneNumber!,
                $"Your GD1 pickup OTP is {otp}");

            return "OTP sent";
        }
    }
}
