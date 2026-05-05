using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class TermsAndConditions : BaseEntity
    {
        public string Type { get; set; } = "General";
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public bool IsActive { get; set; } = true;
    }
}
