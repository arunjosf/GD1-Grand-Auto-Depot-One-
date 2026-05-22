using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using GD1.Application.Common;

namespace GD1.Application.Features.Pickup.Commands
{
    public record AssignManagerCommand(
         long PickupRequestId,
         long ManagerId,
         DateTime ArrivalTime
     ) : IRequest<BaseResponse<string>>;

    public class AssignManagerCommandValidator : AbstractValidator<AssignManagerCommand>
    {
        public AssignManagerCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
            RuleFor(x => x.ManagerId).GreaterThan(0);
            RuleFor(x => x.ArrivalTime).GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5));
        }
    }

    public class AssignManagerCommandHandler
        : IRequestHandler<AssignManagerCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;

        public AssignManagerCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _lotManagerRepo = lotManagerRepo;
        }

        public async Task<BaseResponse<string>> Handle(
            AssignManagerCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking == null)
                throw new Exception("Booking not found");

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property == null)
                throw new Exception("Property not found");

            // Safely resolve the LotManager record regardless of whether the frontend passed the User ID or the LotManager ID
            var managerRecords = await _lotManagerRepo.FindAsync(m => 
                m.PropertyId == property.Id && 
                (m.ManagerId == request.ManagerId || m.Id == request.ManagerId));
                
            var actualManager = managerRecords.FirstOrDefault();
            if (actualManager == null)
                throw new Exception("The specified manager is not assigned to this property.");

            pickup.ManagerId = actualManager.Id;
            pickup.ManagerArrivalTime = request.ArrivalTime;
            pickup.Status = PickupStatus.Assigned;

            await _pickupRepo.UpdateAsync(pickup);

            return BaseResponse<string>.Ok("Manager assigned successfully");
        }
    }
}
