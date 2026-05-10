using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class CancelAssignmentCommand : IRequest<BaseResponse<string>>
    {
        public long AssignmentId { get; set; }
        public string? Reason { get; set; }
        public long AdminId { get; set; }
    }

    public class CancelAssignmentCommandHandler : IRequestHandler<CancelAssignmentCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;

        public CancelAssignmentCommandHandler(
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo)
        {
            _assignRepo = assignRepo;
            _appRepo = appRepo;
        }

        public async Task<BaseResponse<string>> Handle(CancelAssignmentCommand cmd, CancellationToken ct)
        {
            var assignment = await _assignRepo.GetByIdAsync(cmd.AssignmentId);
            if (assignment == null) return BaseResponse<string>.Fail("Assignment not found.");

            if (assignment.Status != "Assigned")
                return BaseResponse<string>.Fail("Only assignments in 'Assigned' status can be cancelled.");

            assignment.Status = "Cancelled";
            await _assignRepo.UpdateAsync(assignment);

            // Revert application status
            var app = await _appRepo.GetByIdAsync(assignment.ApplicationId);
            if (app != null)
            {
                app.Status = "Pending";
                await _appRepo.UpdateAsync(app);
            }

            return BaseResponse<string>.Ok("Assignment cancelled successfully.");
        }
    }
}
