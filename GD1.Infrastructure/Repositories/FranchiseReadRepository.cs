using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GD1.Application.Interfaces.Repositories;
using FranchiseDTOs = GD1.Application.Features.FranchiseApplication.DTOs;
using AdminDTOs = GD1.Application.Features.GD1Admin.DTOs;

namespace GD1.Infrastructure.Repositories
{
    public class FranchiseReadRepository : IFranchiseReadRepository
    {
        private readonly IDbConnection _db;

        public FranchiseReadRepository(IDbConnection db) => _db = db;

        public async Task<FranchiseDTOs.ApplicationDto?> GetByIdAsync(long applicationId, long applicantId)
        {
            string appSql = @"
                SELECT * FROM FranchiseApplications 
                WHERE Id = @ApplicationId" + (applicantId > 0 ? " AND ApplicantId = @ApplicantId" : "");

            var app = await _db.QuerySingleOrDefaultAsync<FranchiseDTOs.ApplicationDto>(appSql, new { ApplicationId = applicationId, ApplicantId = applicantId });

            if (app != null)
            {
                await HydrateLotUnitsAsync(app);
                await HydrateAssignmentsAsync(app);
                
                // Hydrate Rejection History from the same table (using soft-deleted records)
                // Match on owner details AND property details to identify true re-applications
                const string historySql = @"
                    SELECT Id, CreatedAt, AdminNotes, Status as RejectionReason 
                    FROM   FranchiseApplications 
                    WHERE  OwnerName = @OwnerName 
                      AND  ContactEmail = @Email 
                      AND  PhoneNumber = @PhoneNumber
                      AND  AddressLine = @AddressLine
                      AND  City = @City
                      AND  State = @State
                      AND  PostalCode = @PostalCode
                      AND  Id != @CurrentId 
                      AND  Status = @RejectedStatus
                    ORDER BY CreatedAt DESC";
                
                app.PastRejections = (await _db.QueryAsync<FranchiseDTOs.PastRejectionDto>(historySql, new 
                { 
                    OwnerName = app.OwnerName,
                    Email = app.ContactEmail, 
                    PhoneNumber = app.PhoneNumber,
                    AddressLine = app.AddressLine,
                    City = app.City,
                    State = app.State,
                    PostalCode = app.PostalCode,
                    CurrentId = app.Id,
                    RejectedStatus = FranchiseStatus.Rejected.ToString()
                })).ToList();
            }

            return app;
        }

        public async Task<IEnumerable<FranchiseDTOs.ApplicationDto>> GetByApplicantIdAsync(long applicantId)
        {
            const string sql = "SELECT * FROM FranchiseApplications WHERE ApplicantId = @ApplicantId ORDER BY CreatedAt DESC";
            var apps = await _db.QueryAsync<FranchiseDTOs.ApplicationDto>(sql, new { ApplicantId = applicantId });

            foreach (var app in apps)
            {
                await HydrateLotUnitsAsync(app);
            }

            return apps;
        }

        public async Task<IEnumerable<FranchiseDTOs.ApplicationDto>> GetAllPendingAsync()
        {
            const string sql = "SELECT * FROM FranchiseApplications WHERE Status = 'Pending' ORDER BY CreatedAt DESC";
            var apps = await _db.QueryAsync<FranchiseDTOs.ApplicationDto>(sql);

            foreach (var app in apps)
            {
                await HydrateLotUnitsAsync(app);
            }

            return apps;
        }

        private async Task HydrateAssignmentsAsync(FranchiseDTOs.ApplicationDto app)
        {
            const string assignSql = @"
                SELECT ia.Id, ia.ScheduledDate, ia.Status, ia.AgentId,
                       a.FullName as AgentName, a.City as AgentCity, 
                       a.SelfieUrl as AgentSelfieUrl, a.IdProofUrl as AgentIdProofUrl, a.PhoneNumber
                FROM   InspectionAssignments ia
                LEFT JOIN GD1Agents a ON ia.AgentId = a.Id
                WHERE  ia.ApplicationId = @ApplicationId
                ORDER BY ia.CreatedAt DESC";

            var assignments = (await _db.QueryAsync<FranchiseDTOs.InspectionAssignmentDto>(assignSql, new { ApplicationId = app.Id })).ToList();

            foreach (var assign in assignments)
            {
                // Hydrate Report
                const string reportSql = @"
                    SELECT Id, StartedAt, CompletedAt, AgentRemarks, OverallDescription,
                           AdminDecision, AdminRemarks
                    FROM   InspectionReports
                    WHERE  AssignmentId = @AssignmentId";
                
                assign.Report = await _db.QuerySingleOrDefaultAsync<FranchiseDTOs.FranchiseInspectionReportDto>(reportSql, new { AssignmentId = assign.Id });

                if (assign.Report != null)
                {
                    const string itemSql = @"
                        SELECT ii.Id, ii.LotUnitId, lu.Label as LotLabel, ii.TaskName, ii.IsVerified, ii.Remarks
                        FROM   InspectionItems ii
                        INNER JOIN LotUnits lu ON ii.LotUnitId = lu.Id
                        WHERE  ii.ReportId = @ReportId";

                    var items = (await _db.QueryAsync<FranchiseDTOs.FranchiseInspectionItemDto>(itemSql, new { ReportId = assign.Report.Id })).ToList();
                    
                    foreach (var item in items)
                    {
                        item.UnitImages = (await GetUnitImagesAsync(item.LotUnitId, "Agent")).ToList();
                    }
                    
                    assign.Report.Items = items;
                    
                    // Hydrate Agent uploaded property images for this report
                    assign.Report.PropertyImages = (await GetPropertyImagesAsync(app.Id)).Where(i => i.UploadedBy == "Agent").ToList();
                }

            }

            app.Assignments = assignments;
        }

        private async Task HydrateLotUnitsAsync(FranchiseDTOs.ApplicationDto app)
        {
            const string unitSql = @"
                SELECT Id, Label, Tier, Capacity, HasCCTV, HasSecurity, HasWorkshop, HasWashingArea, HasFireSafety, Status
                FROM   LotUnits
                WHERE  FranchiseApplicationId = @ApplicationId";

            var units = await _db.QueryAsync<FranchiseDTOs.LotUnitDto>(unitSql, new { ApplicationId = app.Id });

            foreach (var unit in units)
            {
                // Pull ALL unit images regardless of who uploaded them to ensure visibility
                var allUnitImages = (await GetUnitImagesAsync(unit.Id, null)).ToList();
                
                unit.OwnerImages = allUnitImages.Where(i => i.UploadedBy == "Owner" || i.UploadedBy == "Applicant").ToList();
                unit.AgentImages = allUnitImages.Where(i => i.UploadedBy == "Agent").ToList();
                
                var unitExtra = await _db.QuerySingleOrDefaultAsync<string>("SELECT ExtraFacilities FROM LotUnits WHERE Id = @Id", new { Id = unit.Id });
                unit.ExtraFacilities = unitExtra?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];
            }

            var propImages = (await GetPropertyImagesAsync(app.Id)).ToList();
            app.FrontImageUrl = propImages.FirstOrDefault(i => i.Label == "Front View")?.ImageUrl ?? "";
            app.OtherImageUrls = propImages.Where(i => i.Label != "Front View").Select(i => i.ImageUrl).ToList();

            app.LotUnits = units.ToList();
        }

        private async Task<IEnumerable<FranchiseDTOs.PropertyImageDto>> GetUnitImagesAsync(long lotUnitId, string? uploadedBy)
        {
            var sql = new StringBuilder(@"
                SELECT Id, 
                       CASE WHEN IsMain = 1 OR IsMain = 'true' THEN 'Front View' ELSE 'Lot Unit Image' END as Label, 
                       ImageUrl, UploadedBy, Remark
                FROM   LotUnitImages
                WHERE  LotUnitId = @LotUnitId");

            if (!string.IsNullOrEmpty(uploadedBy))
            {
                sql.Append(" AND TRIM(UploadedBy) = @UploadedBy");
            }

            return await _db.QueryAsync<FranchiseDTOs.PropertyImageDto>(sql.ToString(), new { LotUnitId = lotUnitId, UploadedBy = uploadedBy });
        }

        public async Task<IEnumerable<string>> GetUnitImageUrlsAsync(long lotUnitId)
        {
            const string sql = "SELECT ImageUrl FROM LotUnitImages WHERE LotUnitId = @LotUnitId AND ImageUrl IS NOT NULL AND ImageUrl != ''";
            return await _db.QueryAsync<string>(sql, new { LotUnitId = lotUnitId });
        }

        private async Task<IEnumerable<FranchiseDTOs.PropertyImageDto>> GetPropertyImagesAsync(long applicationId)
        {
            const string sql = @"
                SELECT Id, Label, ImageUrl, UploadedBy, Remark
                FROM   PropertyImages
                WHERE  ApplicationId = @ApplicationId";

            return await _db.QueryAsync<FranchiseDTOs.PropertyImageDto>(sql, new { ApplicationId = applicationId });
        }

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
                SELECT ir.* 
                FROM InspectionReports ir
                JOIN InspectionAssignments ia ON ir.AssignmentId = ia.Id
                WHERE ia.ApplicationId = @ApplicationId";
            return await _db.QueryAsync<InspectionReport>(sql, new { ApplicationId = applicationId });
        }

        public async Task<IEnumerable<FranchiseDTOs.ApplicationDto>> GetAllApplicationsAsync(GD1.Domain.Entities.Enums.FranchiseStatus? status, string? searchTerm, string? sortBy, bool descending)
        {
            var sql = new StringBuilder("SELECT * FROM FranchiseApplications WHERE 1=1");
            var parameters = new DynamicParameters();

            if (status.HasValue)
            {
                sql.Append(" AND Status = @Status");
                parameters.Add("Status", status.Value.ToString());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql.Append(" AND (BusinessName LIKE @Search OR OwnerName LIKE @Search OR ContactEmail LIKE @Search)");
                parameters.Add("Search", $"%{searchTerm}%");
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                sql.Append($" ORDER BY {sortBy} {(descending ? "DESC" : "ASC")}");
            }
            else
            {
                sql.Append(" ORDER BY CreatedAt DESC");
            }

            var apps = await _db.QueryAsync<FranchiseDTOs.ApplicationDto>(sql.ToString(), parameters);
            
            foreach(var app in apps)
            {
                await HydrateLotUnitsAsync(app);
            }

            return apps;
        }

        public async Task<IEnumerable<AdminDTOs.UserListDto>> GetAllAgentsAsync(bool onlyActive, string? city, string? state)
        {
            var sql = new StringBuilder(@"
                SELECT u.Id, a.FullName as Name, u.Email, a.PhoneNumber, a.City, a.State, 
                       a.Id as AgentId, a.IsActive, a.SelfieUrl
                FROM Users u
                JOIN GD1Agents a ON u.Id = a.UserId
                WHERE 1=1");

            var parameters = new DynamicParameters();

            if (onlyActive)
            {
                sql.Append(" AND a.IsVerified = 1");
            }

            if (!string.IsNullOrEmpty(city))
            {
                sql.Append(" AND a.City = @City");
                parameters.Add("City", city);
            }

            if (!string.IsNullOrEmpty(state))
            {
                sql.Append(" AND a.State = @State");
                parameters.Add("State", state);
            }

            var raw = await _db.QueryAsync<dynamic>(sql.ToString(), parameters);
            return MapToUserListDto(raw);
        }

        public async Task<IEnumerable<AdminDTOs.UserListDto>> GetNearbyAgentsAsync(double lat, double lon)
        {
            const string sql = @"
                SELECT TOP 10 u.Id, a.FullName as Name, u.Email, a.PhoneNumber, a.City, a.State, 
                       a.Id as AgentId, a.IsActive, a.SelfieUrl, u.Role,
                       (ABS(a.Latitude - @Lat) + ABS(a.Longitude - @Lon)) * 111 as DistanceKm,
                       (SELECT COUNT(*) FROM InspectionAssignments WHERE AgentId = a.Id AND Status IN ('Assigned', 'InProgress')) as PendingInspections
                FROM Users u
                JOIN GD1Agents a ON u.Id = a.UserId
                WHERE a.IsVerified = 1
                ORDER BY DistanceKm ASC";

            var raw = await _db.QueryAsync<dynamic>(sql, new { Lat = lat, Lon = lon });
            var agents = MapToUserListDto(raw).ToList();

            foreach (var agent in agents)
            {
                if (agent.AgentId.HasValue)
                {
                    const string assignSql = @"
                        SELECT ia.ScheduledDate, fa.BusinessName, ia.Status
                        FROM InspectionAssignments ia
                        JOIN FranchiseApplications fa ON ia.ApplicationId = fa.Id
                        WHERE ia.AgentId = @AgentId AND ia.Status IN ('Assigned', 'InProgress')";
                    
                    agent.CurrentAssignments = (await _db.QueryAsync<AdminDTOs.AgentAssignmentSummaryDto>(assignSql, new { AgentId = agent.AgentId.Value })).ToList();
                }
            }

            return agents;
        }

        public async Task<IEnumerable<FranchiseDTOs.ApplicationDto>> GetAgentAssignedApplicationsAsync(long agentId)
        {
            const string sql = @"
                SELECT fa.* 
                FROM FranchiseApplications fa
                JOIN InspectionAssignments ia ON fa.Id = ia.ApplicationId
                WHERE ia.AgentId = @AgentId";
            
            var apps = await _db.QueryAsync<FranchiseDTOs.ApplicationDto>(sql, new { AgentId = agentId });

            foreach (var app in apps)
            {
                await HydrateLotUnitsAsync(app);
                await HydrateAssignmentsAsync(app);
            }

            return apps;
        }


        private IEnumerable<AdminDTOs.UserListDto> MapToUserListDto(IEnumerable<dynamic> raw)
        {
            return raw.Select(r => new AdminDTOs.UserListDto
            {
                Id = r.Id,
                Name = r.Name,
                FullName = r.Name,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                City = r.City,
                State = r.State,
                AgentId = r.AgentId,
                IsActive = r.IsActive,
                SelfieUrl = r.SelfieUrl,
                DistanceKm = r.DistanceKm ?? 0,
                Role = (UserRole)r.Role,
                PendingInspections = r.PendingInspections
            });
        }
    }
}
