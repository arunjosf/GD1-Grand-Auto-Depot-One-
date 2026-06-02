using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class TriggerMechanicOtpCommand : IRequest<BaseResponse<string>>
    {
        public long ServiceRequestId { get; set; }
        public long LotManagerId { get; set; }
    }

    public class TriggerMechanicOtpCommandHandler : IRequestHandler<TriggerMechanicOtpCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> _propertyRepo;
        private readonly IEmailService _emailService;

        public TriggerMechanicOtpCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> propertyRepo,
            IEmailService emailService)
        {
            _requestRepo = requestRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _emailService = emailService;
        }

        public async Task<BaseResponse<string>> Handle(TriggerMechanicOtpCommand request, CancellationToken ct)
        {
            var serviceRequest = await _requestRepo.GetByIdAsync(request.ServiceRequestId);
            if (serviceRequest == null)
                return BaseResponse<string>.Fail("Service Request not found.");

            if (serviceRequest.Status != "Approved")
                return BaseResponse<string>.Fail("OTP can only be triggered for 'Approved' requests.");

            if (string.IsNullOrEmpty(serviceRequest.MechanicEmail))
                return BaseResponse<string>.Fail("No mechanic email is registered for this service request.");

            // Verify Lot Manager authorization
            // Assuming the LotManagerId corresponds to the LotOwner for simplicity, 
            // or we could check the Property managers list. Let's do a simple check.
            var booking = await _bookingRepo.GetByIdAsync(serviceRequest.BookingId);
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");
            
            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property == null || property.LotOwnerId != request.LotManagerId)
                return BaseResponse<string>.Fail("You are not authorized to trigger OTP for this lot.");

            // Generate 6 digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            serviceRequest.MechanicOtp = otp;

            await _requestRepo.UpdateAsync(serviceRequest);

            // Send Email
            var body = $@"
                <h3>Service Authorization OTP</h3>
                <p>Please provide this OTP to the Lot Manager upon arrival to authorize the service work on the vehicle.</p>
                <h2>{otp}</h2>
            ";
            await _emailService.SendAsync(serviceRequest.MechanicEmail, "Service OTP Verification - Grand Auto Depot", body);

            return BaseResponse<string>.Ok("OTP triggered and sent to the mechanic's email.");
        }
    }
}
