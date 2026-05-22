using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class RequestMaintenanceCommand : IRequest<BaseResponse<string>>
    {
        public long VehicleId { get; set; }
        public long OwnerId { get; set; }
    }

    public class RequestMaintenanceCommandHandler : IRequestHandler<RequestMaintenanceCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;

        public RequestMaintenanceCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<MaintenanceTask> taskRepo)
        {
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
            _taskRepo = taskRepo;
        }

        public async Task<BaseResponse<string>> Handle(RequestMaintenanceCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(request.VehicleId);
            if (vehicle == null || vehicle.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("Vehicle not found or unauthorized.");

            var bookings = await _bookingRepo.FindAsync(b => b.VehicleId == request.VehicleId && b.Status == BookingStatus.InLot, "Property.Managers");
            var activeBooking = bookings.FirstOrDefault();

            if (activeBooking == null)
            {
                // Fallback check if it's stored via PickupStatus
                var pickupBookings = await _bookingRepo.FindAsync(b => b.VehicleId == request.VehicleId, "Property.Managers");
                activeBooking = pickupBookings.FirstOrDefault();
                if (activeBooking == null)
                    return BaseResponse<string>.Fail("Vehicle is not currently actively stored at any lot.");
            }

            var manager = activeBooking.Property?.Managers?.FirstOrDefault();
            if (manager == null)
                return BaseResponse<string>.Fail("No lot manager assigned to this property.");

            // Check if there is already a pending weekly check
            var pendingWeeklyTasks = await _taskRepo.FindAsync(t => t.VehicleId == request.VehicleId && t.Status == MaintenanceTaskStatus.Pending && t.Type == MaintenanceTaskType.WeeklyConditionCheck);
            if (pendingWeeklyTasks.Any())
                return BaseResponse<string>.Fail("A weekly report update will be completed shortly. On-demand image request cancelled.");

            // Check if there is already a pending request
            var pendingTasks = await _taskRepo.FindAsync(t => t.VehicleId == request.VehicleId && t.Status == MaintenanceTaskStatus.Pending && t.Type == MaintenanceTaskType.OnDemandImage);
            if (pendingTasks.Any())
                return BaseResponse<string>.Fail("You already have an image update request pending.");

            var task = new MaintenanceTask
            {
                VehicleId = request.VehicleId,
                BookingId = activeBooking.Id,
                ManagerId = manager.Id,
                Type = MaintenanceTaskType.OnDemandImage,
                Status = MaintenanceTaskStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _taskRepo.AddAsync(task);
            return BaseResponse<string>.Ok("Image update requested successfully.");
        }
    }
}
