using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ToggleUserStatusCommand : IRequest<BaseResponse<bool>>
    {
        public long UserId { get; set; }
    }

    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<StorageLot> _lotRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignmentRepo;

        public ToggleUserStatusCommandHandler(
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<StorageLot> lotRepo,
            IGenericRepository<Agent> agentRepo,
            IGenericRepository<InspectionAssignment> assignmentRepo)
        {
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _lotRepo = lotRepo;
            _agentRepo = agentRepo;
            _assignmentRepo = assignmentRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleUserStatusCommand req, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(req.UserId);
            if (user == null) return BaseResponse<bool>.Fail("User not found.");

            if (user.Role == UserRole.GD1Admin)
                return BaseResponse<bool>.Fail("Admins cannot be blocked.");

            // If user is currently active and we are trying to block them (set IsActive to false)
            if (user.IsActive)
            {
                if (user.Role == UserRole.VehicleOwner)
                {
                    var activeBookings = await _bookingRepo.FindAsync(b => 
                        b.OwnerId == user.Id && 
                        b.Status != BookingStatus.Completed && 
                        b.Status != BookingStatus.Cancelled);
                    
                    if (activeBookings.Any())
                        return BaseResponse<bool>.Fail("Cannot block user. This vehicle owner has active bookings.");
                }
                else if (user.Role == UserRole.LotOwner)
                {
                    var lots = await _lotRepo.FindAsync(l => l.LotOwnerId == user.Id);
                    var lotIds = lots.Select(l => l.Id).ToList();
                    
                    var activeLotBookings = await _bookingRepo.FindAsync(b => 
                        lotIds.Contains(b.LotId) && 
                        b.Status != BookingStatus.Completed && 
                        b.Status != BookingStatus.Cancelled);
                    
                    if (activeLotBookings.Any())
                        return BaseResponse<bool>.Fail("Cannot block user. This lot owner has lots with active bookings.");
                }
                else if (user.Role == UserRole.Agent)
                {
                    var agent = (await _agentRepo.FindAsync(a => a.UserId == user.Id)).FirstOrDefault();
                    if (agent != null)
                    {
                        var activeAssignments = await _assignmentRepo.FindAsync(aa => 
                            aa.AgentId == agent.Id && 
                            aa.Status != "Completed" && 
                            aa.Status != "Cancelled");
                        
                        if (activeAssignments.Any())
                            return BaseResponse<bool>.Fail("Cannot block user. This agent is currently assigned to active inspections.");
                    }
                }
            }

            user.IsActive = !user.IsActive;
            await _userRepo.UpdateAsync(user);

            var status = user.IsActive ? "Activated" : "Blocked";
            return BaseResponse<bool>.Ok(user.IsActive, $"User successfully {status}.");
        }
    }
}
