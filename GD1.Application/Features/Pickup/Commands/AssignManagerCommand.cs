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

namespace GD1.Application.Features.Pickup.Commands
{
    public record AssignManagerCommand(
         long PickupRequestId,
         long ManagerId,
         DateTime ArrivalTime
     ) : IRequest<string>;

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
        : IRequestHandler<AssignManagerCommand, string>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;

        public AssignManagerCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo)
        {
            _pickupRepo = pickupRepo;
        }

        public async Task<string> Handle(
            AssignManagerCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            pickup.ManagerId = request.ManagerId;
            pickup.ManagerArrivalTime = request.ArrivalTime;
            pickup.Status = PickupStatus.Assigned;

            await _pickupRepo.UpdateAsync(pickup);

            return "Manager assigned successfully";
        }
    }
}
