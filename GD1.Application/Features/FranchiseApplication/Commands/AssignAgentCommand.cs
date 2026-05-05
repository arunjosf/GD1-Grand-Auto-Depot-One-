using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class AssignAgentCommand
    {
        public long ApplicationId { get; set; }
        public long AgentId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public long AdminId { get; set; }
    }

    public class AssignAgentCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.InspectionReport> _reportRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.GD1Agents> _agentRepo;
        private readonly IFranchiseReadRepository _franchiseRead;
        private readonly ISmsService _sms;
        private readonly IEmailService _email;

        public AssignAgentCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.InspectionReport> reportRepo,
            IGenericRepository<GD1.Domain.Entities.GD1Agents> agentRepo,
            IFranchiseReadRepository franchiseRead,
            ISmsService sms,
            IEmailService email)
        {
            _appRepo = appRepo;
            _reportRepo = reportRepo;
            _agentRepo = agentRepo;
            _franchiseRead = franchiseRead;
            _sms = sms;
            _email = email;
        }

        public async Task<BaseResponse<string>> HandleAsync(AssignAgentCommand cmd)
        {
            var application = await _appRepo.GetByIdAsync(cmd.ApplicationId);
            if (application is null)
                throw new KeyNotFoundException("Application not found.");

            if (application.Status != "Pending" &&
                application.Status != "UnderReview")
                throw new InvalidOperationException(
                    "Application is not in a reviewable state.");

            var agent = await _agentRepo.GetByIdAsync(cmd.AgentId);
            if (agent is null)
                throw new KeyNotFoundException("Agent not found.");

            var lotUnits = await _franchiseRead
                .GetLotUnitsByApplicationIdAsync(cmd.ApplicationId);

            if (!lotUnits.Any())
                throw new InvalidOperationException(
                    "No lot units found for this application.");

            var reportSummaries =
                new List<(string Token, string Label, string Passcode)>();

            foreach (var unit in lotUnits)
            {
                var accessToken = GenerateAccessToken();
                var plainPasscode = GeneratePlainPasscode();
                var passcodeHash = BCrypt.Net.BCrypt.HashPassword(plainPasscode);

                var report = new GD1.Domain.Entities.InspectionReport
                {
                    ApplicationId = cmd.ApplicationId,
                    LotUnitId = unit.Id,
                    AgentId = cmd.AgentId,
                    AssignedBy = cmd.AdminId,
                    AccessToken = accessToken,
                    PasscodeHash = passcodeHash,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    ScheduledDate = cmd.ScheduledDate,
                    ChecklistJson = GenerateChecklist(unit),
                    Status = "Assigned"
                };

                await _reportRepo.AddAsync(report);
                reportSummaries.Add((accessToken, unit.Label, plainPasscode));
            }

            application.Status = "UnderReview";
            await _appRepo.UpdateAsync(application);

            foreach (var (token, label, passcode) in reportSummaries)
            {
                var link = $"https://gd1.com/inspect/{token}";
                var message =
                    $"GD1 Inspection Assignment\n" +
                    $"Business : {application.BusinessName}\n" +
                    $"Lot Unit : {label}\n" +
                    $"Address  : {application.AddressLine}, {application.City}\n" +
                    $"Date     : {cmd.ScheduledDate:dd MMM yyyy}\n\n" +
                    $"Link     : {link}\n" +
                    $"Passcode : {passcode}\n\n" +
                    $"Link expires in 7 days.";

                await _sms.SendAsync(agent.PhoneNumber, message);

                if (!string.IsNullOrEmpty(agent.Email))
                    await _email.SendAsync(
                        agent.Email,
                        $"GD1 Inspection: {application.BusinessName} - {label}",
                        message);
            }

            return BaseResponse<string>.Ok(string.Empty,
                $"Agent assigned. {reportSummaries.Count} inspection link(s) sent.");
        }

        private static string GenerateAccessToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
               .Replace("+", "-").Replace("/", "_").Replace("=", "");

        private static string GeneratePlainPasscode()
            => new Random().Next(100000, 999999).ToString();

        private static string GenerateChecklist(GD1.Domain.Entities.LotUnit unit)
        {
            var items = new List<object>
            {
                new { item = "Address matches application",  answer = (bool?)null, remark = "" },
                new { item = "Capacity matches claim",       answer = (bool?)null, remark = "" },
                new { item = "Lot is clean and usable",      answer = (bool?)null, remark = "" },
            };

            if (unit.HasCCTV)
                items.Add(new { item = "CCTV installed and working", answer = (bool?)null, remark = "" });
            if (unit.HasSecurity)
                items.Add(new { item = "Security or access control present", answer = (bool?)null, remark = "" });
            if (unit.HasWorkshop)
                items.Add(new { item = "Workshop bay with tools present", answer = (bool?)null, remark = "" });
            if (unit.HasWashingArea)
                items.Add(new { item = "Washing area with drainage present", answer = (bool?)null, remark = "" });

            return System.Text.Json.JsonSerializer.Serialize(items);
        }
    }

}
