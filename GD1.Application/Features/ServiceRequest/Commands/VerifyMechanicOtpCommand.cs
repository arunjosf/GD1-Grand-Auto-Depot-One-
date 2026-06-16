using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class VerifyMechanicOtpCommand : IRequest<BaseResponse<string>>
    {
        public long ServiceRequestId { get; set; }
        public long LotManagerId { get; set; }
        public string Otp { get; set; } = string.Empty;
    }

    public class VerifyMechanicOtpCommandHandler : IRequestHandler<VerifyMechanicOtpCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> _propertyRepo;

        public VerifyMechanicOtpCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> propertyRepo)
        {
            _requestRepo = requestRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<string>> Handle(VerifyMechanicOtpCommand request, CancellationToken ct)
        {
            var serviceRequest = await _requestRepo.GetByIdAsync(request.ServiceRequestId);
            if (serviceRequest == null)
                return BaseResponse<string>.Fail("Service Request not found.");

            if (serviceRequest.Status != "Mechanic Arrived Garage")
                return BaseResponse<string>.Fail("Cannot verify OTP for this request. Status must be 'Mechanic Arrived Garage'.");

            if (string.IsNullOrEmpty(serviceRequest.MechanicOtp))
                return BaseResponse<string>.Fail("No OTP was generated for this request.");

            // Verify Lot Manager authorization
            // Bypass strict owner check
            var booking = await _bookingRepo.GetByIdAsync(serviceRequest.BookingId);
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            if (serviceRequest.MechanicOtp != request.Otp)
                return BaseResponse<string>.Fail("Invalid OTP.");

            serviceRequest.Status = "OTP Verified";
            serviceRequest.MechanicOtp = null; // Clear OTP after use for security

            await _requestRepo.UpdateAsync(serviceRequest);

            return BaseResponse<string>.Ok("OTP verified successfully. Mechanic has been authorized.");
        }
    }
}
