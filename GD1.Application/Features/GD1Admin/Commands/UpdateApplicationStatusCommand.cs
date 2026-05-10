using GD1.Application.Common;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class UpdateApplicationStatusCommand : IRequest<BaseResponse<string>>
    {
        public long Id { get; set; }
        public FranchiseStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public long AdminId { get; set; }
    }

    public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _repo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;

        public UpdateApplicationStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> repo,
            IGenericRepository<InspectionAssignment> assignRepo)
        {
            _repo = repo;
            _assignRepo = assignRepo;
        }

        public async Task<BaseResponse<string>> Handle(UpdateApplicationStatusCommand cmd, CancellationToken ct)
        {
            var app = await _repo.GetByIdAsync(cmd.Id);
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            if (cmd.Status == FranchiseStatus.Rejected)
            {
                // Check if an inspection has already been assigned
                var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id);
                
                if (!assignments.Any())
                {
                    // Case 1: Rejected BEFORE inspection assignment -> Hard Delete
                    await _repo.DeleteAsync(app);
                    return BaseResponse<string>.Ok(string.Empty, "Application rejected and data removed permanently.");
                }
                else
                {
                    // Case 2: Rejected AFTER/DURING inspection (though usually handled in ReviewInspectionCommand)
                    // -> Soft Delete
                    app.Status = FranchiseStatus.Rejected.ToString();
                    app.IsDeleted = true;
                    app.AdminNotes = cmd.AdminNotes;
                    app.ReviewedBy = cmd.AdminId;
                    app.ReviewedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(app);
                    return BaseResponse<string>.Ok(string.Empty, "Application rejected (Soft Delete applied as inspection records exist).");
                }
            }

            app.Status = cmd.Status.ToString();
            app.AdminNotes = cmd.AdminNotes;
            app.ReviewedBy = cmd.AdminId;
            app.ReviewedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(app);
            return BaseResponse<string>.Ok(string.Empty, "Status updated successfully.");
        }
    }
}
