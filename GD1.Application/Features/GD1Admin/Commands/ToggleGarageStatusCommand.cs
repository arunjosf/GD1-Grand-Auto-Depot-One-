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
    public class ToggleGarageStatusCommand : IRequest<BaseResponse<bool>>
    {
        public long GarageId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "hide", "unhide", "block", "unblock"
    }

    public class ToggleGarageStatusCommandHandler : IRequestHandler<ToggleGarageStatusCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly INotificationService _notificationService;

        public ToggleGarageStatusCommandHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<Booking> bookingRepo,
            INotificationService notificationService)
        {
            _propertyRepo = propertyRepo;
            _bookingRepo = bookingRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleGarageStatusCommand request, CancellationToken cancellationToken)
        {
            var property = await _propertyRepo.GetByIdAsync(request.GarageId);
            if (property == null)
            {
                return BaseResponse<bool>.Fail("Garage not found.");
            }

            switch (request.ActionType.ToLower())
            {
                case "hide":
                    property.IsHidden = true;
                    // Notify lot owner
                    await _notificationService.SendAsync(
                        property.LotOwnerId, 
                        "Property Hidden", 
                        $"Your property '{property.Name}' has been hidden by the admin. It will not be visible to vehicle owners.", 
                        "System");
                    break;
                case "unhide":
                    property.IsHidden = false;
                    break;
                case "block":
                    // Validation: check for active bookings
                    var activeBookings = await _bookingRepo.FindAsync(b => b.PropertyId == property.Id 
                        && b.Status != BookingStatus.Completed 
                        && b.Status != BookingStatus.Cancelled);
                        
                    if (activeBookings.Any())
                    {
                        return BaseResponse<bool>.Fail("Cannot block a garage that has active bookings. Please hide it instead until bookings are completed.");
                    }
                    property.IsBlocked = true;
                    break;
                case "unblock":
                    property.IsBlocked = false;
                    break;
                default:
                    return BaseResponse<bool>.Fail("Invalid action type.");
            }

            await _propertyRepo.UpdateAsync(property);

            return BaseResponse<bool>.Ok(true, "Garage status updated successfully.");
        }
    }
}
