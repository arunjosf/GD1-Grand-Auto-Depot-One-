using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;

namespace GD1.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; } = UserRole.VehicleOwner;
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<Vehicle> Vehicles { get; set; } = [];
        public ICollection<FranchiseApplication> FranchiseApplications { get; set; } = [];

    }
}
