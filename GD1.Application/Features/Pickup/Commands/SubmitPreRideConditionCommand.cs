using MediatR;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using GD1.Application.Common;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using GD1.Application.Interfaces.Services;

namespace GD1.Application.Features.Pickup.Commands
{
    public class SubmitPreRideConditionCommand : IRequest<BaseResponse<string>>
    {
        [Required]
        public long PickupRequestId { get; set; }

        public string? InteriorImageUrl { get; set; }
        public string? OdometerImageUrl { get; set; }
        public string? ManagerRemarks { get; set; }
    }

    public class SubmitPreRideConditionCommandHandler : IRequestHandler<SubmitPreRideConditionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;
        private readonly IGeminiService _gemini;

        public SubmitPreRideConditionCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<PickupVerification> verificationRepo,
            IGeminiService gemini)
        {
            _pickupRepo = pickupRepo;
            _verificationRepo = verificationRepo;
            _gemini = gemini;
        }

        public async Task<BaseResponse<string>> Handle(SubmitPreRideConditionCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null)
                return new BaseResponse<string> { Success = false, Message = "Pickup not found" };

            if (!string.IsNullOrEmpty(request.InteriorImageUrl) && !string.IsNullOrEmpty(request.OdometerImageUrl))
            {
                var interiorTask = _gemini.VerifyImageReadabilityAsync(request.InteriorImageUrl, "Car Interior");
                var odometerTask = _gemini.VerifyImageReadabilityAsync(request.OdometerImageUrl, "Odometer Reading");

                await Task.WhenAll(interiorTask, odometerTask);

                if (!interiorTask.Result.IsReadable || interiorTask.Result.ConfidenceScore < 80)
                    return new BaseResponse<string> { Success = false, Message = $"Interior Image Error: {interiorTask.Result.Reason}. Please capture a clearer photo." };
                
                if (!odometerTask.Result.IsReadable || odometerTask.Result.ConfidenceScore < 80)
                    return new BaseResponse<string> { Success = false, Message = $"Odometer Image Error: {odometerTask.Result.Reason}. Please capture a clearer photo." };
            }

            // Find the pickup verification record
            var verifications = await _verificationRepo.FindAsync(
                v => v.BookingId == pickup.BookingId && v.Type == GD1.Domain.Entities.Enums.ReportType.Pickup);
            var verification = verifications.OrderByDescending(v => v.Id).FirstOrDefault();

            if (verification == null)
            {
                // This shouldn't happen if they already did condition report, but just in case
                verification = new PickupVerification
                {
                    BookingId = pickup.BookingId,
                    ManagerId = pickup.ManagerId ?? 0,
                    Type = GD1.Domain.Entities.Enums.ReportType.Pickup,
                    InteriorImageUrl = request.InteriorImageUrl ?? string.Empty,
                    OdometerImageUrl = request.OdometerImageUrl ?? string.Empty,
                    ManagerRemarks = request.ManagerRemarks,
                    VerifiedAt = DateTime.UtcNow
                };
                await _verificationRepo.AddAsync(verification);
            }
            else
            {
                verification.InteriorImageUrl = request.InteriorImageUrl ?? string.Empty;
                verification.OdometerImageUrl = request.OdometerImageUrl ?? string.Empty;
                verification.ManagerRemarks = request.ManagerRemarks;
                await _verificationRepo.UpdateAsync(verification);
            }

            return new BaseResponse<string> { Success = true, Message = "Pre-ride condition submitted successfully." };
        }
    }
}
