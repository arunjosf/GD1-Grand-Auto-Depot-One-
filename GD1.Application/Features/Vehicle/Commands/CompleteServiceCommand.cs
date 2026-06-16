using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class CompleteServiceCommand : IRequest<BaseResponse<string>>
    {
        public long ServiceRequestId { get; set; }
        public long CompletedBy { get; set; }
        public string? CompletionNotes { get; set; }
        // JSON array of image URLs or simply comma-separated
        public string? CompletionPhotos { get; set; } 
    }

    public class CompleteServiceCommandHandler : IRequestHandler<CompleteServiceCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleImage> _imageRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleJourneyEvent> _journeyRepo;

        public CompleteServiceCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleImage> imageRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleJourneyEvent> journeyRepo)
        {
            _serviceRepo = serviceRepo;
            _imageRepo = imageRepo;
            _journeyRepo = journeyRepo;
        }

        public async Task<BaseResponse<string>> Handle(CompleteServiceCommand cmd, CancellationToken cancellationToken)
        {
            var service = await _serviceRepo.GetByIdAsync(cmd.ServiceRequestId);
            if (service is null)
                throw new KeyNotFoundException("Service Request not found.");

            if (service.Status == "Completed" || service.IsCompleted == true)
                throw new InvalidOperationException("Service is already completed.");

            service.Status = "Completed";
            service.IsCompleted = true;
            service.CompletionNotes = cmd.CompletionNotes;
            // service.CompletionPhotos = cmd.CompletionPhotos;

            await _serviceRepo.UpdateAsync(service);

            if (!string.IsNullOrEmpty(cmd.CompletionPhotos))
            {
                var photos = cmd.CompletionPhotos.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in photos)
                {
                    await _imageRepo.AddAsync(new GD1.Domain.Entities.VehicleImage { VehicleId = service.VehicleId, ImageUrl = p.Trim() });
                }
                
                                await _journeyRepo.AddAsync(new GD1.Domain.Entities.VehicleJourneyEvent {
                    VehicleId = service.VehicleId,
                    BookingId = service.BookingId,
                    Description = service.CompletionNotes ?? "Service has been completed.",
                    EventType = "Service"
                });
            }
            
            return BaseResponse<string>.Ok(string.Empty, "Service marked as completed and images uploaded.");
        }
    }
}
