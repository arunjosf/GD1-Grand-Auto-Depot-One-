using System;

namespace GD1.Application.Features.ServiceCenter.DTOs
{
    public class UserServiceCenterApplicationDto
    {
        public long Id { get; set; }
        public string ApplicationType { get; set; } = "ServiceCenter";
        public string BusinessName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
