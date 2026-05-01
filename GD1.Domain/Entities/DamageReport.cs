using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class DamageReport : BaseEntity
    {
        public Guid HandoffId { get; set; }

        public string? DamageImages { get; set; }

        public string? AIFindings { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public string? Description { get; set; }

        public string? ConsentFormUrl { get; set; } = null;
        public Handoff Handoff { get; set; } = null!;

    }
}
