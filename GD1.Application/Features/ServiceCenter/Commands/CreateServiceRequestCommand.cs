using System;
using System.Threading;
using System.Threading.Tasks;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using MediatR;
using GD1.Application.Interfaces.Services;

namespace GD1.Application.Features.ServiceCenter.Commands
{
    public class CreateServiceRequestCommand : IRequest<long>
    {
        public long BookingId { get; set; }
        public long ServiceCenterId { get; set; }
        public long VehicleId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public long RequestedBy { get; set; }
    }

    public class CreateServiceRequestCommandHandler : IRequestHandler<CreateServiceRequestCommand, long>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _repository;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly INotificationService _notificationService;

        public CreateServiceRequestCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> repository,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            INotificationService notificationService)
        {
            _repository = repository;
            _scRepo = scRepo;
            _notificationService = notificationService;
        }

        public async Task<long> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = new GD1.Domain.Entities.ServiceRequest
            {
                BookingId = request.BookingId,
                ServiceCenterId = request.ServiceCenterId,
                VehicleId = request.VehicleId,
                ServiceType = request.ServiceType,
                Notes = request.Notes,
                ScheduledDate = request.ScheduledDate,
                RequestedBy = request.RequestedBy,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.AddAsync(entity);

            var serviceCenter = await _scRepo.GetByIdAsync(request.ServiceCenterId);
            if (serviceCenter != null && serviceCenter.AdminId > 0)
            {
                await _notificationService.SendAsync(
                    serviceCenter.AdminId,
                    "New Service Request",
                    $"A new service request has been booked.",
                    "ServiceRequest",
                    entity.Id,
                    "/service-center/bookings"
                );
            }

            return entity.Id;
        }
    }
}
