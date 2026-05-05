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

        public async Task<ApplicationDto?> GetByIdAsync(
            long applicationId, long applicantId)
        {
            const string sql = @"
                SELECT Id, ApplicationType, BusinessName, OwnerName,
                       ContactEmail, PhoneNumber, AddressLine, City,
                       State, Status, AdminNotes, ApplicationFee,
                       FeeStatus, CreatedAt
                FROM   FranchiseApplications
                WHERE  Id = @ApplicationId AND (@ApplicantId = 0 OR ApplicantId = @ApplicantId)";

            var app = await _db.QuerySingleOrDefaultAsync<ApplicationDto>(
                sql, new
                {
                    ApplicationId = applicationId,
                    ApplicantId = applicantId
                });

            if (app is null) return null;
            await HydrateLotUnitsAsync(app);
            return app;
        }

        public async Task<IEnumerable<ApplicationDto>> GetByApplicantIdAsync(
            long applicantId)
        {
            const string sql = @"
                SELECT Id, ApplicationType, BusinessName, OwnerName,
                       ContactEmail, PhoneNumber, AddressLine, City,
                       State, Status, AdminNotes, ApplicationFee,
                       FeeStatus, CreatedAt
                FROM   FranchiseApplications
                WHERE  ApplicantId = @ApplicantId
                ORDER BY CreatedAt DESC";

            return await _db.QueryAsync<ApplicationDto>(
                sql, new { ApplicantId = applicantId });
        }

        public async Task<IEnumerable<ApplicationDto>> GetAllPendingAsync()
        {
            const string sql = @"
                SELECT Id, ApplicationType, BusinessName, OwnerName,
                       ContactEmail, PhoneNumber, AddressLine, City,
                       State, Status, AdminNotes, ApplicationFee,
                       FeeStatus, CreatedAt
                FROM   FranchiseApplications
                WHERE  Status = 'Pending'
                ORDER BY CreatedAt ASC";

            return await _db.QueryAsync<ApplicationDto>(sql);
        }

        public async Task<IEnumerable<LotUnit>> GetLotUnitsByApplicationIdAsync(
            long applicationId)
        {
            const string sql = @"
                SELECT Id, FranchiseApplicationId, Label, Description,
                       Tier, Capacity, HasCCTV, HasSecurity,
                       HasWorkshop, HasWashingArea, Status,
                       AssignedLotCode, CreatedAt, UpdatedAt
                FROM   LotUnits
                WHERE  FranchiseApplicationId = @ApplicationId";

            return await _db.QueryAsync<LotUnit>(
                sql, new { ApplicationId = applicationId });
        }

        public async Task<InspectionReport?> GetReportByTokenAsync(string accessToken)
        {
            const string sql = @"
                SELECT Id, ApplicationId, LotUnitId, AgentId,
                       AssignedBy, AccessToken, PasscodeHash,
                       ExpiryDate, ScheduledDate, StartedAt,
                       CompletedDate, ChecklistJson, AgentRemarks,
                       Status, Result, AdminDecision, AdminRemarks,
                       DecisionAt, CreatedAt, UpdatedAt
                FROM   InspectionReports
                WHERE  AccessToken = @AccessToken";

            return await _db.QuerySingleOrDefaultAsync<InspectionReport>(
                sql, new { AccessToken = accessToken });
        }

        public async Task<IEnumerable<InspectionReport>> GetReportsByApplicationIdAsync(
            long applicationId)
        {
            const string sql = @"
                SELECT Id, ApplicationId, LotUnitId, Status
                FROM   InspectionReports
                WHERE  ApplicationId = @ApplicationId";

            return await _db.QueryAsync<InspectionReport>(
                sql, new { ApplicationId = applicationId });
        }

        private async Task HydrateLotUnitsAsync(ApplicationDto app)
        {
            const string unitSql = @"
                SELECT Id, Label, Tier, Capacity,
                       HasCCTV, HasSecurity, HasWorkshop,
                       HasWashingArea, Status
                FROM   LotUnits
                WHERE  FranchiseApplicationId = @ApplicationId";

            var units = await _db.QueryAsync<LotUnitDto>(
                unitSql, new { ApplicationId = app.Id });

            foreach (var unit in units)
            {
                unit.OwnerImages = (await GetImagesAsync(
                    app.Id, unit.Id, "Owner")).ToList();
                unit.AgentImages = (await GetImagesAsync(
                    app.Id, unit.Id, "Agent")).ToList();
            }

            app.OverallImages = (await GetOverallImagesAsync(app.Id)).ToList();
            app.LotUnits = units.ToList();
        }

        private async Task<IEnumerable<PropertyImageDto>> GetImagesAsync(
            long applicationId, long lotUnitId, string uploadedBy)
        {
            const string sql = @"
                SELECT Id, Label, ImageUrl, UploadedBy, Remark
                FROM   PropertyImages
                WHERE  ApplicationId = @ApplicationId
                AND    LotUnitId     = @LotUnitId
                AND    UploadedBy    = @UploadedBy";

            return await _db.QueryAsync<PropertyImageDto>(sql, new
            {
                ApplicationId = applicationId,
                LotUnitId = lotUnitId,
                UploadedBy = uploadedBy
            });
        }

        private async Task<IEnumerable<PropertyImageDto>> GetOverallImagesAsync(
            long applicationId)
        {
            const string sql = @"
                SELECT Id, Label, ImageUrl, UploadedBy, Remark
                FROM   PropertyImages
                WHERE  ApplicationId = @ApplicationId
                AND    LotUnitId IS NULL";

            return await _db.QueryAsync<PropertyImageDto>(
                sql, new { ApplicationId = applicationId });
        }
        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.ApplicationListDto>> GetAllApplicationsAsync(string? status, string? searchTerm, string? sortBy, bool descending)
        {
            var sql = @"
                SELECT Id, ApplicationType, BusinessName, OwnerName, City, State, Status, CreatedAt
                FROM FranchiseApplications
                WHERE (@Status IS NULL OR Status = @Status)
                AND (@SearchTerm IS NULL OR BusinessName LIKE '%' + @SearchTerm + '%' OR OwnerName LIKE '%' + @SearchTerm + '%')
                ORDER BY CreatedAt DESC"; // Simplified sorting

            return await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.ApplicationListDto>(sql, new { Status = status, SearchTerm = searchTerm });
        }

        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetAllAgentsAsync(bool onlyActive, string? city, string? state)
        {
            var sql = @"
                SELECT Id, FullName, PhoneNumber, Email, City, State, CoverageArea, Latitude, Longitude, IsActive
                FROM GD1Agents
                WHERE (@OnlyActive = 0 OR IsActive = 1)
                AND (@City IS NULL OR City = @City)
                AND (@State IS NULL OR State = @State)";

            return await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.AgentDto>(sql, new { OnlyActive = onlyActive, City = city, State = state });
        }

        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.AgentDto>> GetNearbyAgentsAsync(string city, string state)
        {
            var sql = @"
                SELECT Id, FullName, PhoneNumber, Email, City, State, CoverageArea, Latitude, Longitude, IsActive
                FROM GD1Agents
                WHERE IsActive = 1 AND State = @State"; // Basic approximation

            return await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.AgentDto>(sql, new { State = state });
        }
    }
}
