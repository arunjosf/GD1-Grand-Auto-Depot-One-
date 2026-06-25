using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using GD1.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class CancelMyApplicationCommand : IRequest<BaseResponse<string>>
    {
        public long ApplicationId { get; set; }
        public long ApplicantId { get; set; }
    }

    public class CancelMyApplicationCommandHandler : IRequestHandler<CancelMyApplicationCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IPaymentService _paymentService;

        public CancelMyApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<Agent> agentRepo,
            IGenericRepository<User> userRepo,
            IEmailService emailService,
            IGenericRepository<Notification> notificationRepo,
            IPaymentService paymentService)
        {
            _appRepo = appRepo;
            _assignRepo = assignRepo;
            _agentRepo = agentRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _notificationRepo = notificationRepo;
            _paymentService = paymentService;
        }

        public async Task<BaseResponse<string>> Handle(CancelMyApplicationCommand cmd, CancellationToken ct)
        {
            var app = await _appRepo.GetByIdAsync(cmd.ApplicationId);
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            if (app.ApplicantId != cmd.ApplicantId)
                return BaseResponse<string>.Fail("You are not authorized to cancel this application.");

            if (app.Status == GD1.Domain.Entities.Enums.FranchiseStatus.Approved)
                return BaseResponse<string>.Fail("Approved applications cannot be cancelled. Please contact support.");

            // 1. Check for active assignments to notify agent
            var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id);
            foreach (var assign in assignments)
            {
                var agent = await _agentRepo.GetByIdAsync(assign.AgentId);
                if (agent != null)
                {
                    var agentUser = await _userRepo.GetByIdAsync(agent.Id);
                    if (agentUser != null && !string.IsNullOrEmpty(agentUser.Email))
                    {
                        string subject = "Franchise Inspection Cancelled";
                        string body = $@"
                            <h3>Hello {agentUser.FullName},</h3>
                            <p>The franchise inspection for <b>{app.BusinessName}</b> scheduled for {assign.ScheduledDate:dd MMM yyyy} has been cancelled by the applicant.</p>
                            <p>This assignment has been removed from your list.</p>
                            <p>Best regards,<br/>GD1 Team</p>";
                        
                        await _emailService.SendAsync(agentUser.Email, subject, body);
                    }
                }
            }
            
            app.Status = GD1.Domain.Entities.Enums.FranchiseStatus.Cancelled;
            app.AdminNotes = "Cancelled by User.";
            app.UpdatedAt = DateTime.UtcNow;

            if (!assignments.Any() && !string.IsNullOrEmpty(app.FeeTransactionId))
            {
                try
                {
                    await _paymentService.RefundPaymentAsync(app.FeeTransactionId, 2000);
                }
                catch
                {
                    // If refund fails, log it or handle it. We still mark as Cancelled.
                }
            }
            
            await _appRepo.UpdateAsync(app);

            foreach (var assign in assignments)
            {
                assign.IsDeleted = true;
                assign.Status = "Cancelled";
                assign.UpdatedAt = DateTime.UtcNow;
                await _assignRepo.UpdateAsync(assign);
            }

            var notifications = await _notificationRepo.FindAsync(n => n.ReferenceId == app.Id && (n.ActionType == "ReviewFranchise" || n.ActionType == "TrackApplication"));
            foreach (var notif in notifications)
            {
                await _notificationRepo.DeleteAsync(notif);
            }

            return BaseResponse<string>.Ok(string.Empty, "Application cancelled successfully.");
        }
    }
}
