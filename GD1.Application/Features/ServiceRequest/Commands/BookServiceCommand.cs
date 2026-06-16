using GD1.Application.Common;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class BookServiceCommand : IRequest<BaseResponse<string>>
    {
        public long VehicleId { get; set; }
        public long OwnerId { get; set; }
        public long ServiceCenterId { get; set; }
        public string? ServiceType { get; set; }
        public string? Notes { get; set; }
        public DateTime RequestedDate { get; set; }
    }

    public class BookServiceCommandHandler : IRequestHandler<BookServiceCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly INotificationService _notificationService;

        public BookServiceCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            INotificationService notificationService)
        {
            _vehicleRepo = vehicleRepo;
            _requestRepo = requestRepo;
            _scRepo = scRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<string>> Handle(BookServiceCommand request, CancellationToken ct)
        {
            var vehicles = await _vehicleRepo.FindAsync(v => v.Id == request.VehicleId && v.OwnerId == request.OwnerId, "Bookings");
            var vehicle = vehicles.FirstOrDefault();

            if (vehicle == null)
                return BaseResponse<string>.Fail("Vehicle not found or you are not the owner.");

            var activeBooking = vehicle.Bookings.OrderByDescending(b => b.Id).FirstOrDefault(b => b.Status == BookingStatus.InLot);

            if (activeBooking == null)
                return BaseResponse<string>.Fail("Vehicle is not currently stored. Only actively stored vehicles can request service.");

            var serviceRequest = new GD1.Domain.Entities.ServiceRequest
            {
                BookingId = activeBooking.Id,
                VehicleId = request.VehicleId,
                ServiceCenterId = request.ServiceCenterId,
                RequestedBy = request.OwnerId,
                ServiceType = request.ServiceType ?? string.Empty,
                Notes = request.Notes,
                ScheduledDate = request.RequestedDate,
                Status = "Pending"
            };

            await _requestRepo.AddAsync(serviceRequest);

            // Clear manager's recommendation flag if it exists, since the owner has taken action.
            if (vehicle.HasServiceRecommendation)
            {
                vehicle.HasServiceRecommendation = false;
                vehicle.ManagerServiceRemarks = null;
                await _vehicleRepo.UpdateAsync(vehicle);
            }

            var serviceCenter = await _scRepo.GetByIdAsync(request.ServiceCenterId);
            if (serviceCenter != null && serviceCenter.AdminId > 0)
            {
                await _notificationService.SendAsync(
                    serviceCenter.AdminId,
                    "New Service Request",
                    $"A new service request has been booked for {vehicle.Brand} {vehicle.Model}.",
                    "ServiceRequest",
                    serviceRequest.Id,
                    "/service-center/bookings"
                );
            }

            return BaseResponse<string>.Ok("Service request booked successfully.");
        }
    }
}
