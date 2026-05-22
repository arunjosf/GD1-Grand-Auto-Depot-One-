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
        /// <summary>Null when called by GD1Admin. Set to LotOwner's UserId when called by LotOwner.</summary>
        public long? LotOwnerId { get; set; }
    }

    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignmentRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;

        public ToggleUserStatusCommandHandler(
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<Agent> agentRepo,
            IGenericRepository<InspectionAssignment> assignmentRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo)
        {
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _agentRepo = agentRepo;
            _assignmentRepo = assignmentRepo;
            _lotManagerRepo = lotManagerRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleUserStatusCommand req, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(req.UserId);
            if (user == null) return BaseResponse<bool>.Fail("User not found.");

            if (user.Role == UserRole.GD1Admin)
                return BaseResponse<bool>.Fail("Admins cannot be blocked.");

            // LotOwner scope check — can only toggle their own managers
            if (req.LotOwnerId.HasValue)
            {
                if (user.Role != UserRole.Manager)
                    return BaseResponse<bool>.Fail("As a LotOwner you can only block/unblock your own managers.");

                var ownedProps = await _propertyRepo.FindAsync(p => p.LotOwnerId == req.LotOwnerId.Value);
                var ownedPropIds = ownedProps.Select(p => p.Id).ToList();
                var managerRecord = await _lotManagerRepo.FindAsync(
                    m => m.ManagerId == req.UserId && ownedPropIds.Contains(m.PropertyId));

                if (!managerRecord.Any())
                    return BaseResponse<bool>.Fail("This user is not a manager of any of your properties.");
            }

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
                    var properties = await _propertyRepo.FindAsync(p => p.LotOwnerId == user.Id);
                    var propIds = properties.Select(p => p.Id).ToList();
                    
                    var activeLotBookings = await _bookingRepo.FindAsync(b => 
                        propIds.Contains(b.PropertyId) && 
                        b.Status != BookingStatus.Completed && 
                        b.Status != BookingStatus.Cancelled);
                    
                    if (activeLotBookings.Any())
                        return BaseResponse<bool>.Fail("Cannot block user. This property owner has sites with active bookings.");
                }
                else if (user.Role == UserRole.Agent)
                {
                    var agent = (await _agentRepo.FindAsync(a => a.Id == user.Id)).FirstOrDefault();
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
