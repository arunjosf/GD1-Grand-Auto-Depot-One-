using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class CancelServiceBookingCommand : IRequest<BaseResponse<string>>
    {
        public long ServiceRequestId { get; set; }
        public long CurrentUserId { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelServiceBookingCommandHandler : IRequestHandler<CancelServiceBookingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;

        public CancelServiceBookingCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo)
        {
            _requestRepo = requestRepo;
            _centerRepo = centerRepo;
        }

        public async Task<BaseResponse<string>> Handle(CancelServiceBookingCommand request, CancellationToken ct)
        {
            var serviceRequests = await _requestRepo.FindAsync(r => r.Id == request.ServiceRequestId, "Booking.Vehicle");
            var serviceRequest = serviceRequests.FirstOrDefault();
            
            if (serviceRequest == null)
                return BaseResponse<string>.Fail("Service Request not found.");

            // Check if user is the Vehicle Owner
            bool isVehicleOwner = serviceRequest.Booking?.Vehicle?.OwnerId == request.CurrentUserId;
            
            // Check if user is the Service Center Admin
            bool isServiceCenterAdmin = false;
            var center = await _centerRepo.GetByIdAsync(serviceRequest.ServiceCenterId);
            if (center != null && center.AdminId == request.CurrentUserId)
            {
                isServiceCenterAdmin = true;
            }

            if (!isVehicleOwner && !isServiceCenterAdmin)
            {
                return BaseResponse<string>.Fail("You are not authorized to cancel this booking.");
            }

            if (serviceRequest.Status == "Completed" || serviceRequest.Status == "Cancelled" || serviceRequest.Status == "Rejected")
                return BaseResponse<string>.Fail($"Cannot cancel request in '{serviceRequest.Status}' status.");

            serviceRequest.Status = "Cancelled";
            serviceRequest.CancellationReason = request.Reason;
            
            await _requestRepo.UpdateAsync(serviceRequest);

            return BaseResponse<string>.Ok("Service request cancelled successfully.");
        }
    }
}
