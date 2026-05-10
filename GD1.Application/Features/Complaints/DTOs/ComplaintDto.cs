using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Complaints.DTOs
{
    public class ComplaintDto
    {
        public long Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdminResponse { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z\s]*$",
            ErrorMessage = "Name must contain only letters and spaces.")]
        public string ComplainantName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string LotName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
