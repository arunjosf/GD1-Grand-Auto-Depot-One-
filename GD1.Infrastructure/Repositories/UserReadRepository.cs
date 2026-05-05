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
    }
}

