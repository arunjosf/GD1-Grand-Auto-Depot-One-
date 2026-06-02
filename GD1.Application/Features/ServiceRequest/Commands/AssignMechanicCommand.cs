using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class AssignMechanicResponse
    {
        public string Message { get; set; } = string.Empty;
        public string ShareableText { get; set; } = string.Empty;
    }

    public class AssignMechanicCommand : IRequest<BaseResponse<AssignMechanicResponse>>
    {
        public long ServiceRequestId { get; set; }
        public long ServiceCenterAdminId { get; set; }
        public string MechanicEmail { get; set; } = string.Empty;
    }

    public class AssignMechanicCommandHandler : IRequestHandler<AssignMechanicCommand, BaseResponse<AssignMechanicResponse>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;

        public AssignMechanicCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo)
        {
            _requestRepo = requestRepo;
            _centerRepo = centerRepo;
        }

        public async Task<BaseResponse<AssignMechanicResponse>> Handle(AssignMechanicCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.MechanicEmail))
                return BaseResponse<AssignMechanicResponse>.Fail("Mechanic email is required to approve the booking.");

            try
            {
                var emailAddress = new System.Net.Mail.MailAddress(request.MechanicEmail);
                if (emailAddress.Address != request.MechanicEmail)
                    return BaseResponse<AssignMechanicResponse>.Fail("Invalid email format.");
            }
            catch
            {
                return BaseResponse<AssignMechanicResponse>.Fail("Invalid email format.");
            }

            var serviceRequests = await _requestRepo.FindAsync(r => r.Id == request.ServiceRequestId, 
                "Booking.Vehicle", "Booking.Property.LotOwner");
                
            var serviceRequest = serviceRequests.FirstOrDefault();

            if (serviceRequest == null)
                return BaseResponse<AssignMechanicResponse>.Fail("Service Request not found.");

            var center = await _centerRepo.GetByIdAsync(serviceRequest.ServiceCenterId);
            if (center == null || center.AdminId != request.ServiceCenterAdminId)
                return BaseResponse<AssignMechanicResponse>.Fail("You are not authorized to make a decision for this request.");

            if (serviceRequest.Status != "Pending")
                return BaseResponse<AssignMechanicResponse>.Fail($"Cannot assign mechanic to request in '{serviceRequest.Status}' status.");

            serviceRequest.Status = "Approved";
            serviceRequest.MechanicEmail = request.MechanicEmail;
            
            await _requestRepo.UpdateAsync(serviceRequest);

            // Construct Shareable Text
            var vehicleBrand = serviceRequest.Booking?.Vehicle?.Brand ?? "Unknown";
            var vehicleModel = serviceRequest.Booking?.Vehicle?.Model ?? "Unknown";
            var vehicleReg = serviceRequest.Booking?.Vehicle?.RegistrationNo ?? "Unknown";
            var propName = serviceRequest.Booking?.Property?.Name ?? "Unknown";
            var propCity = serviceRequest.Booking?.Property?.City ?? "Unknown";
            var ownerPhone = serviceRequest.Booking?.Property?.LotOwner?.PhoneNumber ?? "Not Available";
            var lat = serviceRequest.Booking?.Property?.Latitude;
            var lon = serviceRequest.Booking?.Property?.Longitude;
            
            var mapLink = (lat.HasValue && lon.HasValue) ? $"https://www.google.com/maps/search/?api=1&query={lat},{lon}" : "No Map Link Available";

            var shareText = $"Service Booking Details\n" +
                            $"Vehicle: {vehicleBrand} {vehicleModel} ({vehicleReg})\n" +
                            $"Location: {propName} ({propCity})\n" +
                            $"Contact Lot Owner: {ownerPhone}\n" +
                            $"Map Location: {mapLink}";

            return BaseResponse<AssignMechanicResponse>.Ok(new AssignMechanicResponse
            {
                Message = "Mechanic assigned successfully.",
                ShareableText = shareText
            });
        }
    }
}
