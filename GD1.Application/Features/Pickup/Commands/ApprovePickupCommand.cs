using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Commands
{
    public record ApprovePickupCommand(long PickupRequestId) : IRequest<string>;

    public class ApprovePickupCommandHandler
        : IRequestHandler<ApprovePickupCommand, string>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;

        public ApprovePickupCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo)
        {
            _pickupRepo = pickupRepo;
        }

        public async Task<string> Handle(
            ApprovePickupCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            pickup.IsApprovedByLotOwner = true;
            pickup.Status = PickupStatus.Approved;

            await _pickupRepo.UpdateAsync(pickup);

            return "Pickup approved";
        }
    }
}
