using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class AssignedAgentDto
    {
        public long AssignmentId { get; set; }
        public long AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
    }

    public class AssignAgentCommand : IRequest<BaseResponse<AssignedAgentDto>>
    {
        public long ApplicationId { get; set; }
        public long AgentId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public long AdminId { get; set; }
    }

    public class AssignAgentCommandHandler : IRequestHandler<AssignAgentCommand, BaseResponse<AssignedAgentDto>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Agent> _agentRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Notification> _notifRepo;
        private readonly ISmsService _sms;
        private readonly IEmailService _email;

        public AssignAgentCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<GD1.Domain.Entities.Agent> agentRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IGenericRepository<GD1.Domain.Entities.Notification> notifRepo,
            ISmsService sms,
            IEmailService email)
        {
            _appRepo = appRepo;
            _assignRepo = assignRepo;
            _agentRepo = agentRepo;
            _userRepo = userRepo;
            _notifRepo = notifRepo;
            _sms = sms;
            _email = email;
        }

        public async Task<BaseResponse<AssignedAgentDto>> Handle(AssignAgentCommand cmd, CancellationToken cancellationToken)
        {
            var application = await _appRepo.GetByIdAsync(cmd.ApplicationId);
            if (application is null) throw new KeyNotFoundException("Application not found.");

            var agent = await _agentRepo.GetByIdAsync(cmd.AgentId);
            if (agent is null) throw new KeyNotFoundException("Agent not found.");

            var applicant = await _userRepo.GetByIdAsync(application.ApplicantId);
            if (applicant is null) throw new KeyNotFoundException("Applicant user not found.");

            // 1. Create a new assignment
            var assignment = new InspectionAssignment
            {
                ApplicationId = cmd.ApplicationId,
                AgentId = cmd.AgentId,
                ScheduledDate = cmd.ScheduledDate,
                Status = "Assigned"
            };
            await _assignRepo.AddAsync(assignment);

            // 2. Update application status
            application.Status = FranchiseStatus.Assigned;
            await _appRepo.UpdateAsync(application);

            // 3. Notify Agent
            var agentMsg = $"GD1: You are assigned to inspect {application.BusinessName} on {cmd.ScheduledDate:dd MMM yyyy}. Log in for details.";
            await _sms.SendAsync(agent.PhoneNumber, agentMsg);
            if (!string.IsNullOrEmpty(agent.Email))
            {
                await _email.SendAsync(agent.Email, "New Inspection Assignment", agentMsg);
            }

            // 4. Notify Applicant (Mail + Website Notification)
            var applicantMsg = $"An agent has been assigned for your franchise inspection.\n\n" +
                               $"Agent: {agent.FullName}\n" +
                               $"Scheduled Date: {cmd.ScheduledDate:dd MMM yyyy} at your property.\n" +
                               $"Agent Photo: {agent.SelfieUrl}\n" +
                               $"Agent ID Proof: {agent.IdProofUrl}\n\n" +
                               $"Please ensure someone is available at the property.";

            // Send Email
            await _email.SendAsync(applicant.Email, "Inspection Scheduled - GD1 Auto Hub", applicantMsg);

            // Save Website Notification
            await _notifRepo.AddAsync(new Notification
            {
                UserId = applicant.Id,
                Title = "Inspection Scheduled",
                Body = $"Agent {agent.FullName} will visit on {cmd.ScheduledDate:dd MMM yyyy}.",
                IsRead = false
            });

            return BaseResponse<AssignedAgentDto>.Ok(new AssignedAgentDto 
            { 
                AssignmentId = assignment.Id,
                AgentId = agent.Id, 
                AgentName = agent.FullName 
            }, $"Agent assigned and applicant notified successfully.");
        }
    }
}
