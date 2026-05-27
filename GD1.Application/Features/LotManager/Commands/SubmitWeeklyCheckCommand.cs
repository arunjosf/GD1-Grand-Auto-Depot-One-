using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Commands
{
    public class SubmitWeeklyCheckCommand : IRequest<BaseResponse<string>>
    {
        public long? TaskId { get; set; }
        public long? VehicleId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public long ManagerId { get; set; }

        public bool CarWashCompleted { get; set; }
        public bool TyrePressureChecked { get; set; }
        public bool DailyStartupsCompleted { get; set; }
        public string ManagerRemarks { get; set; } = string.Empty;

        // 6 Images
        public string FrontImageUrl { get; set; } = string.Empty;
        public string RearImageUrl { get; set; } = string.Empty;
        public string LeftSideImageUrl { get; set; } = string.Empty;
        public string RightSideImageUrl { get; set; } = string.Empty;
        public string InteriorImageUrl { get; set; } = string.Empty;
        public string OdometerImageUrl { get; set; } = string.Empty;
    }

    public class SubmitWeeklyCheckCommandHandler : IRequestHandler<SubmitWeeklyCheckCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<VehicleImage> _imageRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public SubmitWeeklyCheckCommandHandler(
            IGenericRepository<MaintenanceTask> taskRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<VehicleImage> imageRepo,
            IGenericRepository<Booking> _bookingRepo)
        {
            _taskRepo = taskRepo;
            _journeyRepo = journeyRepo;
            _imageRepo = imageRepo;
            this._bookingRepo = _bookingRepo;
        }

        public async Task<BaseResponse<string>> Handle(SubmitWeeklyCheckCommand request, CancellationToken cancellationToken)
        {
            long vehicleId = 0;
            long bookingId = 0;

            if (request.TaskId.HasValue && request.TaskId.Value > 0)
            {
                var tasks = await _taskRepo.FindAsync(t => t.Id == request.TaskId.Value, "Manager");
                var task = tasks.FirstOrDefault();
                if (task == null || task.Manager == null || task.Manager.ManagerId != request.ManagerId)
                    return BaseResponse<string>.Fail("Task not found or unauthorized.");

                if (task.Status == MaintenanceTaskStatus.Completed)
                    return BaseResponse<string>.Fail("This task is already completed.");

                if (task.Type != MaintenanceTaskType.WeeklyConditionCheck)
                    return BaseResponse<string>.Fail("This task is not a Weekly Condition Check.");

                // Update Task
                task.Status = MaintenanceTaskStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;
                task.CarWashCompleted = request.CarWashCompleted;
                task.TyrePressureChecked = request.TyrePressureChecked;
                task.DailyStartupsCompleted = request.DailyStartupsCompleted;
                task.ManagerRemarks = request.ManagerRemarks;

                await _taskRepo.UpdateAsync(task);

                vehicleId = task.VehicleId;
                bookingId = task.BookingId;
            }
            else if (request.VehicleId.HasValue && request.VehicleId.Value > 0)
            {
                var bookings = await _bookingRepo.FindAsync(b => b.VehicleId == request.VehicleId.Value && b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot);
                var booking = bookings.FirstOrDefault();
                if (booking == null)
                    return BaseResponse<string>.Fail("No active storage booking found for this vehicle.");
                
                vehicleId = booking.VehicleId;
                bookingId = booking.Id;
            }
            else
            {
                return BaseResponse<string>.Fail("Either TaskId or VehicleId must be provided.");
            }

            // Log to Journey Timeline
            var description = $"Maintenance Check completed.\nCar Wash: {(request.CarWashCompleted ? "Yes" : "No")}\nTyre Pressure Checked: {(request.TyrePressureChecked ? "Yes" : "No")}\nDaily Startups: {(request.DailyStartupsCompleted ? "Yes" : "No")}\nRemarks: {request.ManagerRemarks}";

            var journeyEvent = new VehicleJourneyEvent
            {
                VehicleId = vehicleId,
                BookingId = bookingId,
                EventType = request.TaskId.HasValue && request.TaskId.Value > 0 ? "WeeklyUpdate" : "AdHocMaintenanceUpdate",
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _journeyRepo.AddAsync(journeyEvent);

            // Add images to VehicleImages
            var labels = new[] { "Front", "Rear", "LeftSide", "RightSide", "Interior", "Odometer" };
            var urls = new[] { request.FrontImageUrl, request.RearImageUrl, request.LeftSideImageUrl, request.RightSideImageUrl, request.InteriorImageUrl, request.OdometerImageUrl };

            // Strict URL Validation (Async loop checking for vehicleId)
            foreach (var url in urls)
            {
                if (!string.IsNullOrEmpty(url) && !url.Contains($"vehicle-{vehicleId}"))
                {
                    return BaseResponse<string>.Fail($"Upload Blocked: URL mismatch. The image does not belong to vehicle {vehicleId}.");
                }
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (!string.IsNullOrEmpty(urls[i]))
                {
                    await _imageRepo.AddAsync(new VehicleImage
                    {
                        VehicleId = vehicleId,
                        EventId = journeyEvent.Id,
                        Label = labels[i],
                        ImageUrl = urls[i],
                        UploadedBy = "LotManager",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            return BaseResponse<string>.Ok("Maintenance Check submitted successfully.");
        }
    }
}

