using Dapper;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GD1.Infrastructure.Repositories
{
    public class FranchiseReadRepository : IFranchiseReadRepository
    {
        private readonly string _connectionString;

        public FranchiseReadRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<ApplicationDto?> GetByIdAsync(long id, long applicantId)
        {
            const string sql = @"
                SELECT * FROM FranchiseApplications WHERE Id = @Id;
                SELECT * FROM FranchiseSlots WHERE ApplicationId = @Id;
                SELECT * FROM PropertyImages WHERE ApplicationId = @Id;
                SELECT a.*, u.FullName as AgentName 
                FROM InspectionAssignments a 
                LEFT JOIN Users u ON a.AgentId = u.Id
                WHERE a.ApplicationId = @Id;";

            using var db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(sql, new { Id = id });
            
            var app = await multi.ReadSingleOrDefaultAsync<ApplicationDto>();
            if (app == null) return null;

            app.Slots = (await multi.ReadAsync<FranchiseSlotDto>()).ToList();
            app.PropertyImages = (await multi.ReadAsync<PropertyImageDto>()).ToList();
            app.Assignments = (await multi.ReadAsync<InspectionAssignmentDto>()).ToList();

            foreach (var assign in app.Assignments)
            {
                const string reportSql = "SELECT * FROM InspectionReports WHERE AssignmentId = @AssignId";
                assign.Report = await db.QuerySingleOrDefaultAsync<FranchiseInspectionReportDto>(reportSql, new { AssignId = assign.Id });
                
                if (assign.Report != null)
                {
                    const string slotVerSql = "SELECT * FROM InspectionSlotItems WHERE ReportId = @ReportId";
                    assign.Report.SlotVerifications = (await db.QueryAsync<InspectionSlotVerificationDto>(slotVerSql, new { ReportId = assign.Report.Id })).ToList();
                    
                    const string imgSql = "SELECT * FROM PropertyImages WHERE ApplicationId = @AppId AND UploadedBy = 'Agent'";
                    assign.Report.SiteImages = (await db.QueryAsync<PropertyImageDto>(imgSql, new { AppId = app.Id })).ToList();
                }
            }

            return app;
        }

        public async Task<IEnumerable<ApplicationDto>> GetByApplicantIdAsync(long applicantId)
        {
            const string sql = "SELECT * FROM FranchiseApplications WHERE ApplicantId = @ApplicantId ORDER BY CreatedAt DESC";
            using var db = CreateConnection();
            var apps = (await db.QueryAsync<ApplicationDto>(sql, new { ApplicantId = applicantId })).ToList();

            foreach (var app in apps)
            {
                const string slotSql = "SELECT * FROM FranchiseSlots WHERE ApplicationId = @AppId";
                app.Slots = (await db.QueryAsync<FranchiseSlotDto>(slotSql, new { AppId = app.Id })).ToList();
                
                const string imgSql = "SELECT TOP 1 ImageUrl FROM PropertyImages WHERE ApplicationId = @AppId AND IsMain = 1";
                app.FrontImageUrl = await db.QueryFirstOrDefaultAsync<string>(imgSql, new { AppId = app.Id }) ?? "";
            }

            return apps;
        }

        public async Task<IEnumerable<ApplicationDto>> GetAllApplicationsAsync(string? status)
        {
            var sql = "SELECT * FROM FranchiseApplications";
            if (!string.IsNullOrEmpty(status)) sql += " WHERE Status = @Status";
            sql += " ORDER BY CreatedAt DESC";

            using var db = CreateConnection();
            var apps = (await db.QueryAsync<ApplicationDto>(sql, new { Status = status })).ToList();

            foreach (var app in apps)
            {
                const string slotSql = "SELECT * FROM FranchiseSlots WHERE ApplicationId = @AppId";
                app.Slots = (await db.QueryAsync<FranchiseSlotDto>(slotSql, new { AppId = app.Id })).ToList();

                const string imgSql = "SELECT TOP 1 ImageUrl FROM PropertyImages WHERE ApplicationId = @AppId AND IsMain = 1";
                app.FrontImageUrl = await db.QueryFirstOrDefaultAsync<string>(imgSql, new { AppId = app.Id }) ?? "";
            }

            return apps;
        }

        public async Task<IEnumerable<PendingAgentDto>> GetPendingAgentsAsync()
        {
            const string sql = @"
                SELECT a.Id, u.FullName, u.Email, u.PhoneNumber, 
                       a.SelfieUrl, a.IdProofUrl, a.City, a.State, a.PostalCode
                FROM GD1Agents a
                JOIN Users u ON a.Id = u.Id
                WHERE a.ApprovalStatus = 0";

            using var db = CreateConnection();
            return await db.QueryAsync<PendingAgentDto>(sql);
        }

        public async Task<IEnumerable<UserListDto>> GetNearbyAgentsAsync(double lat, double lon)
        {
            // Simple bounding box or distance calculation using SQL
            const string sql = @"
                SELECT u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt
                FROM GD1Agents a
                JOIN Users u ON a.Id = u.Id
                WHERE a.IsVerified = 1 
                AND a.Latitude IS NOT NULL AND a.Longitude IS NOT NULL
                ORDER BY (ABS(a.Latitude - @Lat) + ABS(a.Longitude - @Lon)) ASC";

            using var db = CreateConnection();
            return await db.QueryAsync<UserListDto>(sql, new { Lat = lat, Lon = lon });
        }

        public async Task<IEnumerable<UserListDto>> GetAllAgentsAsync(bool verifiedOnly, string? city, string? state)
        {
            var sql = @"
                SELECT u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt
                FROM GD1Agents a
                JOIN Users u ON a.Id = u.Id
                WHERE 1=1";

            if (verifiedOnly) sql += " AND a.IsVerified = 1";
            if (!string.IsNullOrEmpty(city)) sql += " AND a.City = @City";
            if (!string.IsNullOrEmpty(state)) sql += " AND a.State = @State";

            using var db = CreateConnection();
            return await db.QueryAsync<UserListDto>(sql, new { City = city, State = state });
        }
    }
}
