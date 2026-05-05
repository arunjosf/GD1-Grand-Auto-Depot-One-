using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Complaints.DTOs
{
    public class ComplaintDto
    {
        public long Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminResponse { get; set; }
        public string ComplainantName { get; set; } = string.Empty;
        public string LotName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
