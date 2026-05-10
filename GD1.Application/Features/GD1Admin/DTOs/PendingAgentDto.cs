namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class PendingAgentDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }
}
