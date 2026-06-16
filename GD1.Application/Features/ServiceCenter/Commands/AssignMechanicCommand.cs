using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Commands
{
    public class AssignMechanicCommand : IRequest<BaseResponse<string>>
    {
        public long AdminId { get; set; }
        public long ServiceRequestId { get; set; }
        public long MechanicId { get; set; }
        public string AdminNotes { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
    }

    public class AssignMechanicCommandHandler : IRequestHandler<AssignMechanicCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<Mechanics> _mechanicsRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> _propertyRepo;
        private readonly IEmailService _emailService;

        public AssignMechanicCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<Mechanics> mechanicsRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> propertyRepo,
            IEmailService emailService)
        {
            _scRepo = scRepo;
            _requestRepo = requestRepo;
            _mechanicsRepo = mechanicsRepo;
            _bookingRepo = bookingRepo;
            _vehicleRepo = vehicleRepo;
            _propertyRepo = propertyRepo;
            _emailService = emailService;
        }

        public async Task<BaseResponse<string>> Handle(AssignMechanicCommand request, CancellationToken cancellationToken)
        {
            var centers = await _scRepo.FindAsync(x => x.AdminId == request.AdminId);
            var sc = centers.FirstOrDefault();
            if (sc == null) return BaseResponse<string>.Fail("Service center not found");

            var sr = await _requestRepo.GetByIdAsync(request.ServiceRequestId);
            if (sr == null || sr.ServiceCenterId != sc.Id) return BaseResponse<string>.Fail("Service request not found");

            var mechanic = await _mechanicsRepo.GetByIdAsync(request.MechanicId);
            if (mechanic == null || mechanic.ServiceCenterId != sc.Id) return BaseResponse<string>.Fail("Mechanic not found");

            sr.MechanicId = mechanic.Id;
            sr.MechanicEmail = mechanic.Email;
            
            // Generate OTP
            var random = new Random();
            sr.MechanicOtp = random.Next(1000, 9999).ToString();
            sr.Status = "Assigned Mechanic";
            if (request.ScheduledDate.HasValue) sr.ScheduledDate = request.ScheduledDate.Value;
            sr.Instructions = request.AdminNotes;
            
            await _requestRepo.UpdateAsync(sr);

            var bk = await _bookingRepo.GetByIdAsync(sr.BookingId);
            var vehicle = await _vehicleRepo.GetByIdAsync(bk.VehicleId);
            var prop = await _propertyRepo.GetByIdAsync(bk.PropertyId);

            // Construct Email Content
            string lat = prop?.Latitude?.ToString() ?? "0";
            string lng = prop?.Longitude?.ToString() ?? "0";
            string scLat = sc.Latitude?.ToString() ?? "0";
            string scLng = sc.Longitude?.ToString() ?? "0";

            string mapUrl = $"https://www.google.com/maps/dir/?api=1&destination={lat},{lng}";
            
            string emailBody = $@"
            <h2>New Service Assignment</h2>
            <p>Hello {mechanic.FullName}, you have been assigned a new service request.</p>
            
            <h3>Vehicle Details</h3>
            <ul>
                <li><strong>Brand:</strong> {vehicle?.Brand}</li>
                <li><strong>Model:</strong> {vehicle?.Model}</li>
                <li><strong>Reg No:</strong> {vehicle?.RegistrationNo}</li>
                <li><strong>Service Type:</strong> {sr.ServiceType}</li>
            </ul>

            <h3>Location</h3>
            <p><strong>Property:</strong> {prop?.Name}, {prop?.AddressLine}, {prop?.City}</p>

            <h3>Notes</h3>
            <p><strong>Owner Notes:</strong> {sr.Notes ?? "None"}</p>
            <p><strong>Admin Notes:</strong> {request.AdminNotes ?? "None"}</p>

            <h3>Service Schedule</h3>
            <p><strong>Scheduled Date:</strong> {(request.ScheduledDate.HasValue ? request.ScheduledDate.Value.ToString("dd MMM yyyy, hh:mm tt") : "Not Scheduled")}</p>

            <br/>
            <a href='{mapUrl}' style='padding: 12px 24px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                Open in Google Maps
            </a>
            ";

            if (!string.IsNullOrEmpty(mechanic.Email))
            {
                await _emailService.SendAsync(mechanic.Email, $"New Service Assignment: {vehicle?.RegistrationNo}", emailBody);
            }

            return BaseResponse<string>.Ok("Success", "Mechanic assigned successfully");
        }
    }
}
