using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using GD1.Application.Interfaces.Services;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ToggleServiceCenterStatusCommand : IRequest<BaseResponse<bool>>
    {
        public long ServiceCenterId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "hide", "unhide", "block", "unblock"
    }

    public class ToggleServiceCenterStatusCommandHandler : IRequestHandler<ToggleServiceCenterStatusCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _serviceCenterRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;
        private readonly INotificationService _notificationService;

        public ToggleServiceCenterStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> serviceCenterRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo,
            INotificationService notificationService)
        {
            _serviceCenterRepo = serviceCenterRepo;
            _serviceRequestRepo = serviceRequestRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleServiceCenterStatusCommand request, CancellationToken cancellationToken)
        {
            var center = await _serviceCenterRepo.GetByIdAsync(request.ServiceCenterId);
            if (center == null)
            {
                return BaseResponse<bool>.Fail("Service Center not found.");
            }

            switch (request.ActionType.ToLower())
            {
                case "hide":
                    center.IsHidden = true;
                    await _notificationService.SendAsync(
                        center.AdminId, 
                        "Service Center Hidden", 
                        $"Your service center '{center.Name}' has been hidden by the admin. It will not be visible to vehicle owners.", 
                        "System");
                    break;
                case "unhide":
                    center.IsHidden = false;
                    break;
                case "block":
                    // Validation: check for active service requests
                    var activeRequests = await _serviceRequestRepo.FindAsync(sr => sr.ServiceCenterId == center.Id 
                        && sr.IsCompleted != true
                        && sr.Status != "Completed" 
                        && sr.Status != "Cancelled");
                        
                    if (activeRequests.Any())
                    {
                        return BaseResponse<bool>.Fail("Cannot block a service center that has active service requests. Please hide it instead until requests are completed.");
                    }
                    center.IsBlocked = true;
                    break;
                case "unblock":
                    center.IsBlocked = false;
                    break;
                default:
                    return BaseResponse<bool>.Fail("Invalid action type.");
            }

            await _serviceCenterRepo.UpdateAsync(center);

            return BaseResponse<bool>.Ok(true, "Service Center status updated successfully.");
        }
    }
}
