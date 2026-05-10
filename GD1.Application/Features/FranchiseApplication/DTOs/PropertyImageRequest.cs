using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class PropertyImageRequest
    {

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [Url(ErrorMessage = "Invalid image URL.")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Remark { get; set; }

        public long? LotUnitId { get; set; }
    }
}
