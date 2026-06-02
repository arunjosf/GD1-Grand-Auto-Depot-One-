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
    }

    public class CompleteServiceRequestCommandHandler : IRequestHandler<CompleteServiceRequestCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;

        public CompleteServiceRequestCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IFileService fileService,
            IEmailService emailService)
        {
            _requestRepo = requestRepo;
            _centerRepo = centerRepo;
            _journeyRepo = journeyRepo;
            _fileService = fileService;
            _emailService = emailService;
        }

        public async Task<BaseResponse<string>> Handle(CompleteServiceRequestCommand request, CancellationToken ct)
        {
            var serviceRequests = await _requestRepo.FindAsync(r => r.Id == request.ServiceRequestId, 
                "Booking.Vehicle.Owner");
            var serviceRequest = serviceRequests.FirstOrDefault();

            if (serviceRequest == null)
                return BaseResponse<string>.Fail("Service Request not found.");

            var center = await _centerRepo.GetByIdAsync(serviceRequest.ServiceCenterId);
            if (center == null || center.AdminId != request.ServiceCenterAdminId)
                return BaseResponse<string>.Fail("You are not authorized to complete this request.");

            if (serviceRequest.Status != "Approved" && serviceRequest.Status != "MechanicArrived")
                return BaseResponse<string>.Fail($"Cannot complete request in '{serviceRequest.Status}' status. Must be Approved or MechanicArrived.");

            if (request.BillFile == null || request.BillFile.Length == 0)
                return BaseResponse<string>.Fail("A bill file is required to complete the service.");

            // Upload the bill file
            var billUrl = await _fileService.SaveFileAsync(request.BillFile, "Bills");

            serviceRequest.Status = "Completed";
            serviceRequest.IsCompleted = true;
            serviceRequest.CompletionNotes = request.CompletionNotes;
            serviceRequest.BillUrl = billUrl;

            await _requestRepo.UpdateAsync(serviceRequest);

            // Add Journey Event
            if (serviceRequest.Booking?.VehicleId != null)
            {
                var journeyEvent = new VehicleJourneyEvent
                {
                    VehicleId = serviceRequest.Booking.VehicleId,
                    EventType = "Service Completed",
                    Description = request.CompletionNotes,
                    CreatedAt = System.DateTime.UtcNow,
                    Images = new System.Collections.Generic.List<VehicleImage>()
                };

                // Add the Bill as a "document" image link so the owner can access it
                journeyEvent.Images.Add(new VehicleImage
                {
                    Label = "Service Bill",
                    ImageUrl = billUrl
                });

                await _journeyRepo.AddAsync(journeyEvent);
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
