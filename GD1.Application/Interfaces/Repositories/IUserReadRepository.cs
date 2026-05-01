using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.Auth.DTOs;

namespace GD1.Application.Interfaces.Repositories
{
    public interface IUserReadRepository
    {
        Task<UserDto?> GetByIdAsync(long id);
        Task<bool> EmailExistsAsync(string email);
    }
}
