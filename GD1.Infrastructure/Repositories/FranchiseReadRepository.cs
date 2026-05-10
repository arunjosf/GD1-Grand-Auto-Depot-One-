using GD1.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GD1.Application.Interfaces.Repositories;
using GD1.Application.Features.FranchiseApplication.DTOs;

namespace GD1.Infrastructure.Repositories
{
    public class FranchiseReadRepository : IFranchiseReadRepository
    {
        private readonly IDbConnection _db;

        public FranchiseReadRepository(IDbConnection db) => _db = db;

        public async Task<ApplicationDto?> GetByIdAsync(long applicationId, long applicantId)
        {
            const string sql = @"
                SELECT Id, ApplicationType, BusinessName, OwnerName,
                       ContactEmail, PhoneNumber, AddressLine, City,
                       State, PostalCode, Status, AdminNotes, ApplicationFee,
                       FeeStatus, CreatedAt, Latitude, Longitude, PreferredInspectionDate
                FROM   FranchiseApplications
                WHERE  Id = @ApplicationId AND (@ApplicantId = 0 OR ApplicantId = @ApplicantId)";

            var app = await _db.QuerySingleOrDefaultAsync<ApplicationDto>(sql, new { ApplicationId = applicationId, ApplicantId = applicantId });

            if (app is null) return null;

            await HydrateLotUnitsAsync(app);
            await HydrateAssignmentsAsync(app);
            await HydratePastRejectionsAsync(app, applicationId);

            return app;
        }

        private async Task HydrateAssignmentsAsync(ApplicationDto app)
        {
            const string assignSql = @"
                SELECT ia.Id, ia.AgentId, a.FullName as AgentName, a.City as AgentCity, 
                       a.SelfieUrl as AgentSelfieUrl, a.IdProofUrl as AgentIdProofUrl,
                       a.PhoneNumber,
                       ia.ScheduledDate, ia.Status
                FROM   InspectionAssignments ia
                INNER JOIN GD1Agents a ON ia.AgentId = a.Id
                WHERE  ia.ApplicationId = @ApplicationId
                ORDER BY ia.CreatedAt DESC";

            var assignments = (await _db.QueryAsync<InspectionAssignmentDto>(assignSql, new { ApplicationId = app.Id })).ToList();

            foreach (var assign in assignments)
            {
                // Hydrate Report
                const string reportSql = @"
                    SELECT Id, StartedAt, CompletedAt, AgentRemarks, OverallDescription,
                           AdminDecision, AdminRemarks
                    FROM   InspectionReports
                    WHERE  AssignmentId = @AssignmentId";
                
                assign.Report = await _db.QuerySingleOrDefaultAsync<InspectionReportDto>(reportSql, new { AssignmentId = assign.Id });

                if (assign.Report != null)
                {
                    const string itemSql = @"
                        SELECT ii.Id, ii.LotUnitId, lu.Label as LotLabel, ii.TaskName, ii.IsVerified, ii.Remarks
                        FROM   InspectionItems ii
                        INNER JOIN LotUnits lu ON ii.LotUnitId = lu.Id
                        WHERE  ii.ReportId = @ReportId";
                    
                    var items = await _db.QueryAsync<InspectionItemDto>(itemSql, new { ReportId = assign.Report.Id });
                    assign.Report.Items = items.ToList();
                }

                // Hydrate Requests (Appeals/Reschedules)
                const string reqSql = @"
                    SELECT Id, Description, RequestedDate, Status, AdminRemarks, CreatedAt
                    FROM   AgentRequests
                    WHERE  AssignmentId = @AssignmentId
                    ORDER BY CreatedAt ASC";
                
                var requests = (await _db.QueryAsync<dynamic>(reqSql, new { AssignmentId = assign.Id }))
                    .Select(r => new AgentRequestDto
                    {
                        Id = r.Id,
                        Description = r.Description,
                        RequestedDate = r.RequestedDate,
                        Status = ((GD1.Domain.Entities.Enums.AppealStatus)r.Status).ToString(),
                        AdminRemarks = r.AdminRemarks,
                        CreatedAt = r.CreatedAt
                    }).ToList();
                assign.Requests = requests;
            }

            app.Assignments = assignments;
        }

        private async Task HydrateLotUnitsAsync(ApplicationDto app)
        {
            const string unitSql = @"
                SELECT Id, Label, Tier, Capacity, HasCCTV, HasSecurity, HasWorkshop, HasWashingArea, HasFireSafety, Status
                FROM   LotUnits
                WHERE  FranchiseApplicationId = @ApplicationId";

            var units = await _db.QueryAsync<LotUnitDto>(unitSql, new { ApplicationId = app.Id });

            foreach (var unit in units)
            {
                unit.OwnerImages = (await GetUnitImagesAsync(unit.Id, "Owner")).ToList();
                unit.AgentImages = (await GetUnitImagesAsync(unit.Id, "Agent")).ToList();
                
                var unitExtra = await _db.QuerySingleOrDefaultAsync<string>("SELECT ExtraFacilities FROM LotUnits WHERE Id = @Id", new { Id = unit.Id });
                unit.ExtraFacilities = unitExtra?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];
            }

            var propImages = (await GetPropertyImagesAsync(app.Id)).ToList();
            app.FrontImageUrl = propImages.FirstOrDefault(i => i.Label == "Front View")?.ImageUrl ?? "";
            app.OtherImageUrls = propImages.Where(i => i.Label != "Front View").Select(i => i.ImageUrl).ToList();

            var appExtra = await _db.QuerySingleOrDefaultAsync<string>("SELECT ExtraFacilities FROM FranchiseApplications WHERE Id = @Id", new { Id = app.Id });
            app.ExtraFacilities = appExtra?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

            app.LotUnits = units.ToList();
        }

        private async Task<IEnumerable<PropertyImageDto>> GetUnitImagesAsync(long lotUnitId, string uploadedBy)
        {
            const string sql = @"
                SELECT Id, CASE WHEN IsMain = 1 THEN 'Front View' ELSE 'Lot Unit Image' END as Label, 
                       ImageUrl, UploadedBy, Remark
                FROM   LotUnitImages
                WHERE  LotUnitId = @LotUnitId AND UploadedBy = @UploadedBy";

            return await _db.QueryAsync<PropertyImageDto>(sql, new { LotUnitId = lotUnitId, UploadedBy = uploadedBy });
        }

        private async Task<IEnumerable<PropertyImageDto>> GetPropertyImagesAsync(long applicationId)
        {
            const string sql = @"SELECT Id, Label, ImageUrl, UploadedBy, Remark FROM PropertyImages WHERE ApplicationId = @ApplicationId";
            return await _db.QueryAsync<PropertyImageDto>(sql, new { ApplicationId = applicationId });
        }

        public async Task<IEnumerable<ApplicationDto>> GetAllApplicationsAsync(GD1.Domain.Entities.Enums.FranchiseStatus? status, string? searchTerm, string? sortBy, bool descending)
        {
            var statusStr = status?.ToString();
            var sql = @"
                SELECT fa.Id, fa.ApplicationType, fa.BusinessName, fa.OwnerName,
                       fa.ContactEmail, fa.PhoneNumber, fa.AddressLine, fa.City,
                       fa.State, fa.PostalCode, fa.Status, fa.AdminNotes, fa.ApplicationFee,
                       fa.FeeStatus, fa.CreatedAt, fa.PreferredInspectionDate,
                       pi.ImageUrl as FrontImageUrl
                FROM FranchiseApplications fa
                LEFT JOIN PropertyImages pi ON fa.Id = pi.ApplicationId AND pi.IsMain = 1
                WHERE (@Status IS NULL OR fa.Status = @Status)
                AND (@SearchTerm IS NULL 
                     OR fa.BusinessName LIKE '%' + @SearchTerm + '%' 
                     OR fa.OwnerName LIKE '%' + @SearchTerm + '%'
                     OR fa.City LIKE '%' + @SearchTerm + '%'
                     OR fa.State LIKE '%' + @SearchTerm + '%')
                ORDER BY fa.CreatedAt DESC";

            var apps = (await _db.QueryAsync<ApplicationDto>(sql, new { Status = statusStr, SearchTerm = searchTerm })).ToList();
            foreach (var app in apps) 
            {
                await HydrateLotUnitsAsync(app);
                await HydrateAssignmentsAsync(app);
                await HydratePastRejectionsAsync(app, app.Id);
            }
            return apps;
        }

        public async Task<IEnumerable<ApplicationDto>> GetAgentAssignedApplicationsAsync(long agentId)
        {
            const string sql = @"
                SELECT fa.Id, fa.ApplicationType, fa.BusinessName, fa.OwnerName,
                       fa.ContactEmail, fa.PhoneNumber, fa.AddressLine, fa.City,
                       fa.State, fa.PostalCode, fa.Status, fa.AdminNotes, fa.ApplicationFee,
                       fa.FeeStatus, fa.CreatedAt, fa.PreferredInspectionDate
                FROM FranchiseApplications fa
                INNER JOIN InspectionAssignments ia ON fa.Id = ia.ApplicationId
                WHERE ia.AgentId = @AgentId AND ia.Status IN ('Assigned', 'InProgress')
                ORDER BY ia.ScheduledDate DESC";

            var apps = (await _db.QueryAsync<ApplicationDto>(sql, new { AgentId = agentId })).ToList();
            foreach (var app in apps)
            {
                await HydrateLotUnitsAsync(app);
                await HydrateAssignmentsAsync(app);
            }
            return apps;
        }

        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetAllAgentsAsync(bool onlyActive, string? city, string? state)
        {
            var sql = @"
                SELECT Id, FullName, PhoneNumber, Email, City, State, PostalCode, CoverageArea, Latitude, Longitude, IsActive,
                       (SELECT COUNT(*) FROM InspectionAssignments WHERE AgentId = GD1Agents.Id AND (Status = 'Assigned' OR Status = 'InProgress')) as PendingInspectionsCount
                FROM GD1Agents
                WHERE (@OnlyActive = 0 OR IsActive = 1)
                AND (@City IS NULL OR City = @City)
                AND (@State IS NULL OR State = @State)";
            return await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.AgentDto>(sql, new { OnlyActive = onlyActive, City = city, State = state });
        }

        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetNearbyAgentsAsync(double lat, double lon)
        {
            var sql = @"
                SELECT Id, FullName, PhoneNumber, Email, City, State, PostalCode, CoverageArea, Latitude, Longitude, IsActive,
                       (SELECT COUNT(*) FROM InspectionAssignments WHERE AgentId = GD1Agents.Id AND (Status = 'Assigned' OR Status = 'InProgress')) as PendingInspectionsCount
                FROM GD1Agents
                WHERE IsActive = 1";

            var agents = (await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.AgentDto>(sql)).ToList();
            foreach (var agent in agents)
            {
                if (agent.Latitude.HasValue && agent.Longitude.HasValue && (agent.Latitude != 0 || agent.Longitude != 0))
                    agent.DistanceKm = HaversineDistance(lat, lon, agent.Latitude.Value, agent.Longitude.Value);
            }
            return agents.OrderBy(a => a.DistanceKm == null).ThenBy(a => a.DistanceKm);
        }

        private async Task HydratePastRejectionsAsync(ApplicationDto app, long currentAppId)
        {
            var applicantId = await _db.QuerySingleAsync<long>("SELECT ApplicantId FROM FranchiseApplications WHERE Id = @Id", new { Id = currentAppId });
            const string sql = @"
                SELECT Id, CreatedAt, AdminNotes, Status as RejectionReason FROM FranchiseApplications
                WHERE ApplicantId = @ApplicantId AND Id != @CurrentAppId AND (Status = 'Rejected' OR IsDeleted = 1)
                ORDER BY CreatedAt DESC";
            var past = await _db.QueryAsync<PastRejectionDto>(sql, new { ApplicantId = applicantId, CurrentAppId = currentAppId });
            app.PastRejections = past.ToList();
        }

        private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; var dLat = ToRadians(lat2 - lat1); var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRadians(double angle) => (Math.PI / 180) * angle;

        public async Task<IEnumerable<LotUnit>> GetLotUnitsByApplicationIdAsync(long applicationId)
        {
            const string sql = "SELECT * FROM LotUnits WHERE FranchiseApplicationId = @ApplicationId";
            return await _db.QueryAsync<LotUnit>(sql, new { ApplicationId = applicationId });
        }

        public async Task<InspectionReport?> GetReportByIdAsync(long reportId)
        {
            const string sql = "SELECT * FROM InspectionReports WHERE Id = @Id";
            return await _db.QuerySingleOrDefaultAsync<InspectionReport>(sql, new { Id = reportId });
        }

        public async Task<IEnumerable<InspectionReport>> GetReportsByApplicationIdAsync(long applicationId)
        {
            const string sql = @"
                SELECT ir.* FROM InspectionReports ir
                INNER JOIN InspectionAssignments ia ON ir.AssignmentId = ia.Id
                WHERE ia.ApplicationId = @ApplicationId";
            return await _db.QueryAsync<InspectionReport>(sql, new { ApplicationId = applicationId });
        }

        public async Task<IEnumerable<ApplicationDto>> GetAllPendingAsync()
        {
            const string sql = @"
                SELECT fa.Id, fa.ApplicationType, fa.BusinessName, fa.OwnerName,
                       fa.ContactEmail, fa.PhoneNumber, fa.AddressLine, fa.City,
                       fa.State, fa.PostalCode, fa.Status, fa.AdminNotes, fa.ApplicationFee,
                       fa.FeeStatus, fa.CreatedAt, fa.PreferredInspectionDate,
                       fa.Latitude, fa.Longitude,
                       pi.ImageUrl as FrontImageUrl
                FROM FranchiseApplications fa
                LEFT JOIN PropertyImages pi ON fa.Id = pi.ApplicationId AND pi.IsMain = 1
                WHERE fa.Status = 'Pending'
                ORDER BY fa.CreatedAt ASC";
            var apps = (await _db.QueryAsync<ApplicationDto>(sql)).ToList();
            foreach (var app in apps) await HydrateLotUnitsAsync(app);
            return apps;
        }

        public async Task<IEnumerable<ApplicationDto>> GetByApplicantIdAsync(long applicantId)
        {
            const string sql = @"
                SELECT fa.Id, fa.ApplicationType, fa.BusinessName, fa.OwnerName,
                       fa.ContactEmail, fa.PhoneNumber, fa.AddressLine, fa.City,
                       fa.State, fa.PostalCode, fa.Status, fa.AdminNotes, fa.ApplicationFee,
                       fa.FeeStatus, fa.CreatedAt, fa.PreferredInspectionDate,
                       fa.Latitude, fa.Longitude,
                       pi.ImageUrl as FrontImageUrl
                FROM FranchiseApplications fa
                LEFT JOIN PropertyImages pi ON fa.Id = pi.ApplicationId AND pi.IsMain = 1
                WHERE fa.ApplicantId = @ApplicantId
                ORDER BY fa.CreatedAt DESC";
            var apps = (await _db.QueryAsync<ApplicationDto>(sql, new { ApplicantId = applicantId })).ToList();
            foreach (var app in apps) await HydrateLotUnitsAsync(app);
            return apps;
        }
    }
}
