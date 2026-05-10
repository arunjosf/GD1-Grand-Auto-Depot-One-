using GD1.Domain.Entities.Enums;
using System;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class UserListDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName => Role.ToString();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Optional role-specific summary
        public string? City { get; set; }
        public string? State { get; set; }
        public int? PendingInspections { get; set; }
    }
}
