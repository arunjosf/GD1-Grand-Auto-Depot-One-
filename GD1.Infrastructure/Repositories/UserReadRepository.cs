using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace GD1.Infrastructure.Repositories
{
    public class UserReadRepository : IUserReadRepository
    {
        private readonly IDbConnection _db;

        public UserReadRepository(IDbConnection db) => _db = db;

        public async Task<UserDto?> GetByIdAsync(long id)
        {
            const string sql = @"
                SELECT  Id,
                        FullName,
                        Email,
                        PhoneNumber,
                        AvatarUrl,
                        Role        AS RoleId,
                        IsActive
                FROM    Users
                WHERE   Id = @Id";

            return await _db.QuerySingleOrDefaultAsync<UserDto>(sql, new { Id = id });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM   Users
                WHERE  Email = @Email";

            var count = await _db.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<IEnumerable<GD1.Application.Features.GD1Admin.DTOs.UserListDto>> GetAllUsersAsync(GD1.Domain.Entities.Enums.UserRole? role, string? search)
        {
            const string sql = @"
                SELECT u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt,
                       a.Id as AgentId, a.City, a.State, a.PostalCode, a.SelfieUrl, a.IdProofUrl, a.ApprovalStatus, a.Latitude, a.Longitude,
                       CASE WHEN u.Role = 3 THEN (SELECT COUNT(*) FROM InspectionAssignments WHERE AgentId = a.Id AND Status IN ('Assigned', 'InProgress')) ELSE NULL END as PendingInspections
                FROM Users u
                LEFT JOIN GD1Agents a ON u.Id = a.UserId
                WHERE u.Role < 5 
                AND (@Role IS NULL OR u.Role = @Role)
                AND (@Search IS NULL 
                     OR u.FullName LIKE '%' + @Search + '%' 
                     OR u.Email LIKE '%' + @Search + '%' 
                     OR a.City LIKE '%' + @Search + '%' 
                     OR a.State LIKE '%' + @Search + '%')
                ORDER BY u.CreatedAt DESC";

            var raw = (await _db.QueryAsync<dynamic>(sql, new { Role = role, Search = search })).ToList();
            
            var users = raw.Select(u => new GD1.Application.Features.GD1Admin.DTOs.UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                AgentId = u.AgentId,
                Role = (GD1.Domain.Entities.Enums.UserRole)u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                City = u.City,
                State = u.State,
                PostalCode = u.PostalCode,
                PendingInspections = u.PendingInspections,
                SelfieUrl = u.SelfieUrl,
                IdProofUrl = u.IdProofUrl,
                ApprovalStatus = u.ApprovalStatus != null ? ((GD1.Domain.Entities.Enums.AgentApprovalStatus)u.ApprovalStatus).ToString() : null,
                Latitude = u.Latitude,
                Longitude = u.Longitude
            }).ToList();

            foreach (var user in users.Where(u => u.Role == GD1.Domain.Entities.Enums.UserRole.Agent))
            {
                const string assignSql = @"
                    SELECT ia.ScheduledDate, fa.BusinessName, ia.Status
                    FROM InspectionAssignments ia
                    INNER JOIN FranchiseApplications fa ON ia.ApplicationId = fa.Id
                    INNER JOIN GD1Agents a ON ia.AgentId = a.Id
                    WHERE a.UserId = @UserId AND ia.Status NOT IN ('Completed', 'Cancelled')
                    ORDER BY ia.ScheduledDate ASC";
                
                user.CurrentAssignments = (await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.AgentAssignmentSummaryDto>(assignSql, new { UserId = user.Id })).ToList();
            }

            return users;
        }
    }
}

