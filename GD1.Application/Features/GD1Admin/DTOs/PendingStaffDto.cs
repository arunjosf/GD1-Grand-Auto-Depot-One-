using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    /// <summary>
    /// Unified DTO returned for both pending agents (GD1Admin view) 
    /// and pending managers (LotOwner view).
    /// </summary>
    public class PendingStaffDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;

        // Agent-specific (null for managers)
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        // Manager-specific (null for agents)
        public long? PropertyId { get; set; }
        public string? PropertyName { get; set; }
    }
}
