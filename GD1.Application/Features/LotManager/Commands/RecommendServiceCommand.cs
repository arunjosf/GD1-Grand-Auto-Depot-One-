using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Commands
{
    public class RecommendServiceCommand : IRequest<BaseResponse<string>>
    {
        public long VehicleId { get; set; }
        public string Remarks { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public long ManagerId { get; set; }
    }

    public class RecommendServiceCommandHandler : IRequestHandler<RecommendServiceCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IEmailService _emailService;

        public RecommendServiceCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IEmailService emailService)
        {
            _vehicleRepo = vehicleRepo;
            _emailService = emailService;
        }

        public async Task<BaseResponse<string>> Handle(RecommendServiceCommand request, CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleRepo.FindAsync(v => v.Id == request.VehicleId, "Owner");
            var vehicle = System.Linq.Enumerable.FirstOrDefault(vehicles);
            
            if (vehicle == null)
                return BaseResponse<string>.Fail("Vehicle not found.");

            if (vehicle.Owner == null)
                return BaseResponse<string>.Fail("Vehicle owner not found.");

            // Save the recommendation to the database
            vehicle.HasServiceRecommendation = true;
            vehicle.ManagerServiceRemarks = request.Remarks;
            await _vehicleRepo.UpdateAsync(vehicle);

            // Construct email message
            string subject = $"Service Recommendation for your {vehicle.Brand} {vehicle.Model}";
            string body = $@"
                <p>Hello {vehicle.Owner.FullName},</p>
                <p>The Lot Manager for your vehicle ({vehicle.RegistrationNo}) has recommended a service check.</p>
                <p><strong>Manager Remarks:</strong> {request.Remarks}</p>
                <p>Please log in to the Grand Auto Depot application to review and book a service request.</p>
            ";

            await _emailService.SendAsync(vehicle.Owner.Email, subject, body);

            // In-app notification could be added here if a NotificationRepo is injected

            return BaseResponse<string>.Ok("Service recommendation sent successfully to the vehicle owner.");
        }
    }
}
