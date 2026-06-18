using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Services;
using System.Linq;

namespace GD1.Application.Features.LotManager.Commands
{
    public class SubmitOnDemandImagesCommand : IRequest<BaseResponse<string>>
    {
        public long TaskId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public long ManagerId { get; set; }

        public string FrontImageUrl { get; set; } = string.Empty;
        public string RearImageUrl { get; set; } = string.Empty;
        public string LeftSideImageUrl { get; set; } = string.Empty;
        public string RightSideImageUrl { get; set; } = string.Empty;
        public string InteriorImageUrl { get; set; } = string.Empty;
        public string OdometerImageUrl { get; set; } = string.Empty;
    }

    public class SubmitOnDemandImagesCommandHandler : IRequestHandler<SubmitOnDemandImagesCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<VehicleImage> _imageRepo;
        private readonly INotificationService _notificationService;

        public SubmitOnDemandImagesCommandHandler(
            IGenericRepository<MaintenanceTask> taskRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<VehicleImage> imageRepo,
            INotificationService notificationService)
        {
            _taskRepo = taskRepo;
            _journeyRepo = journeyRepo;
            _imageRepo = imageRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<string>> Handle(SubmitOnDemandImagesCommand request, CancellationToken cancellationToken)
        {
            var tasks = await _taskRepo.FindAsync(t => t.Id == request.TaskId, "Manager", "Booking", "Vehicle");
            var task = tasks.FirstOrDefault();
            
            if (task == null || task.Manager == null || task.Manager.ManagerId != request.ManagerId)
                return BaseResponse<string>.Fail("Pending on-demand image task not found or unauthorized.");

            if (task.Status == MaintenanceTaskStatus.Completed)
                return BaseResponse<string>.Fail("This task is already completed.");

            if (task.Type != MaintenanceTaskType.OnDemandImage)
                return BaseResponse<string>.Fail("This task is not an On-Demand Image Request.");

            var labels = new[] { "Front", "Rear", "LeftSide", "RightSide", "Interior", "Odometer" };
            var urls = new[] { request.FrontImageUrl, request.RearImageUrl, request.LeftSideImageUrl, request.RightSideImageUrl, request.InteriorImageUrl, request.OdometerImageUrl };

            // URL validation removed since Cloudinary URLs don't contain vehicle ID

            task.Status = MaintenanceTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            await _taskRepo.UpdateAsync(task);

            var journeyEvent = new VehicleJourneyEvent
            {
                VehicleId = task.VehicleId,
                BookingId = task.BookingId,
                EventType = "OnDemandUpdate",
                Description = "On-demand image update requested by the vehicle owner has been fulfilled.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _journeyRepo.AddAsync(journeyEvent);

            // Send notification to the vehicle owner
            await _notificationService.SendAsync(
                userId: task.Booking.OwnerId,
                title: "On-Demand Images Updated",
                body: $"The lot manager has submitted the on-demand images you requested for {task.Vehicle.Brand} {task.Vehicle.Model}.",
                actionType: "VIEW_IMAGES",
                referenceId: task.VehicleId,
                actionUrl: $"/stored-vehicle/{task.BookingId}"
            );

            for (int i = 0; i < labels.Length; i++)
            {
                if (!string.IsNullOrEmpty(urls[i]))
                {
                    await _imageRepo.AddAsync(new VehicleImage
                    {
                        VehicleId = task.VehicleId,
                        EventId = journeyEvent.Id,
                        Label = labels[i],
                        ImageUrl = urls[i],
                        UploadedBy = "LotManager",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // Send Notification to Owner
            var ownerId = task.Booking?.OwnerId;
            if (ownerId.HasValue)
            {
                await _notificationService.SendAsync(
                    userId: ownerId.Value,
                    title: "On-Demand Images Ready",
                    body: $"Your on-demand image request for vehicle {task.Vehicle?.Brand} has been fulfilled by the manager. Check your dashboard.",
                    actionType: "Vehicle",
                    referenceId: task.VehicleId,
                    actionUrl: $"/vehicle/{task.VehicleId}"
                );
            }

            return BaseResponse<string>.Ok("On-Demand Images submitted successfully.");
        }
    }
}

