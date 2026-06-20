using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class CompleteServiceRequestCommand : IRequest<BaseResponse<string>>
    {
        public long ServiceRequestId { get; set; }
        public long ServiceCenterAdminId { get; set; }
        public string CompletionNotes { get; set; } = string.Empty;
        public IFormFile BillFile { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class CompleteServiceRequestCommandHandler : IRequestHandler<CompleteServiceRequestCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Notification> _notificationRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PickupRequest> _pickupRepo;

        public CompleteServiceRequestCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IFileService fileService,
            IEmailService emailService,
            IGenericRepository<MaintenanceTask> taskRepo,
            IGenericRepository<GD1.Domain.Entities.Notification> notificationRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<GD1.Domain.Entities.PickupRequest> pickupRepo)
        {
            _requestRepo = requestRepo;
            _centerRepo = centerRepo;
            _journeyRepo = journeyRepo;
            _fileService = fileService;
            _emailService = emailService;
            _taskRepo = taskRepo;
            _notificationRepo = notificationRepo;
            _lotManagerRepo = lotManagerRepo;
            _pickupRepo = pickupRepo;
        }

        public async Task<BaseResponse<string>> Handle(CompleteServiceRequestCommand request, CancellationToken ct)
        {
            var serviceRequests = await _requestRepo.FindAsync(r => r.Id == request.ServiceRequestId, 
                "Booking.Vehicle.Owner", "Booking.Property.LotOwner");
            var serviceRequest = serviceRequests.FirstOrDefault();

            if (serviceRequest == null)
                return BaseResponse<string>.Fail("Service Request not found.");

            var center = await _centerRepo.GetByIdAsync(serviceRequest.ServiceCenterId);
            if (center == null || center.AdminId != request.ServiceCenterAdminId)
                return BaseResponse<string>.Fail("You are not authorized to complete this request.");

            var allowedStatuses = new[] { "Approved", "MechanicArrived", "Mechanic Arrived Garage", "Assigned Mechanic", "Assigned", "OTP Verified" };
            if (!allowedStatuses.Contains(serviceRequest.Status))
                return BaseResponse<string>.Fail($"Cannot complete request in '{serviceRequest.Status}' status.");

            if (request.BillFile == null || request.BillFile.Length == 0)
                return BaseResponse<string>.Fail("A bill file is required to complete the service.");

            // Upload the bill file
            var billUrl = await _fileService.SaveFileAsync(request.BillFile, "Bills");

            serviceRequest.Status = "Service Completed";
            serviceRequest.IsCompleted = true;
            serviceRequest.CompletionNotes = request.CompletionNotes;
            serviceRequest.BillUrl = billUrl;
            serviceRequest.Amount = request.Amount;
            serviceRequest.ServiceCost = request.Amount;
            serviceRequest.PlatformFee = request.Amount * 0.10m;
            serviceRequest.CenterEarning = request.Amount;
            serviceRequest.IsPaid = false;

            await _requestRepo.UpdateAsync(serviceRequest);

            // The Journey Event for After Service Condition will be generated when the Manager submits the task.

            // Generate MaintenanceTask for Manager
            var managerUserId = serviceRequest.Booking?.AssignedManagerId;
            if (managerUserId == null && serviceRequest.Booking != null)
            {
                var pickups = await _pickupRepo.FindAsync(p => p.BookingId == serviceRequest.Booking.Id);
                var activePickup = pickups.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (activePickup != null && activePickup.ManagerId.HasValue)
                {
                    var lm = await _lotManagerRepo.GetByIdAsync(activePickup.ManagerId.Value);
                    managerUserId = lm?.ManagerId;
                }
            }

            if (serviceRequest.Booking != null && managerUserId != null)
            {
                var lotManagers = await _lotManagerRepo.FindAsync(lm => lm.ManagerId == managerUserId.Value);
                var lotManager = lotManagers.FirstOrDefault();

                if (lotManager != null)
                {
                    await _taskRepo.AddAsync(new MaintenanceTask
                    {
                        VehicleId = serviceRequest.Booking.VehicleId,
                        BookingId = serviceRequest.Booking.Id,
                        ManagerId = lotManager.Id,
                        Type = GD1.Domain.Entities.Enums.MaintenanceTaskType.AfterServiceCondition,
                        Status = GD1.Domain.Entities.Enums.MaintenanceTaskStatus.Pending,
                        RequestedAt = System.DateTime.UtcNow,
                        ManagerRemarks = request.CompletionNotes // Passing notes so it can be viewed if needed
                    });
                    
                    await _notificationRepo.AddAsync(new GD1.Domain.Entities.Notification
                    {
                        UserId = managerUserId.Value,
                        Title = "Service Completed - Condition Check Required",
                        Body = $"Service for {serviceRequest.Booking?.Vehicle?.RegistrationNo} is completed. Please submit a condition report.",
                        ActionUrl = $"/lot-manager/tasks",
                        CreatedAt = System.DateTime.UtcNow
                    });
                }
            }

            // Notify Lot Owner
            if (serviceRequest.Booking?.Property?.LotOwner != null)
            {
                await _notificationRepo.AddAsync(new GD1.Domain.Entities.Notification
                {
                    UserId = serviceRequest.Booking.Property.LotOwner.Id,
                    Title = "Vehicle Serviced at your Lot",
                    Body = $"Vehicle {serviceRequest.Booking?.Vehicle?.RegistrationNo} has completed its service.",
                    ActionUrl = $"/lot-owner/tracking",
                    CreatedAt = System.DateTime.UtcNow
                });
            }

            // Send Email to Vehicle Owner
            var ownerEmail = serviceRequest.Booking?.Vehicle?.Owner?.Email;
            if (!string.IsNullOrEmpty(ownerEmail))
            {
                var subject = "Your Vehicle Service is Completed";
                var body = $"<p>Hello,</p>" +
                           $"<p>Your vehicle <b>{serviceRequest.Booking?.Vehicle?.Brand} {serviceRequest.Booking?.Vehicle?.Model}</b> has been successfully serviced.</p>" +
                           $"<p><b>Service Center Description:</b> {request.CompletionNotes}</p>" +
                           $"<p><b>Bill Document:</b> <a href=\"{billUrl}\">Click here to view your bill</a></p>" +
                           $"<p>Thank you for using Grand Auto Storage!</p>";
                await _emailService.SendAsync(ownerEmail, subject, body);
            }

            return BaseResponse<string>.Ok("Service request completed successfully.");
        }
    }
}
