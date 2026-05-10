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
                       a.City, a.State,
                       (SELECT COUNT(*) FROM InspectionAssignments WHERE AgentId = a.Id AND Status IN ('Assigned', 'InProgress')) as PendingInspections
                FROM Users u
                LEFT JOIN GD1Agents a ON u.Id = a.UserId
                WHERE u.Role != 5 
                AND (@Role IS NULL OR u.Role = @Role)
                AND (@Search IS NULL 
                     OR u.FullName LIKE '%' + @Search + '%' 
                     OR u.Email LIKE '%' + @Search + '%' 
                     OR a.City LIKE '%' + @Search + '%' 
                     OR a.State LIKE '%' + @Search + '%')
                ORDER BY u.CreatedAt DESC";

            return await _db.QueryAsync<GD1.Application.Features.GD1Admin.DTOs.UserListDto>(sql, new 
            { 
                Role = role, 
                Search = search 
            });
        }
    }
}

