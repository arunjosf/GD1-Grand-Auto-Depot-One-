using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotManager : BaseEntity
    {
        public Guid LotId { get; set; }
        public Guid ManagerId { get; set; }
        public Guid AddedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public StorageLot LotOwner { get; set; } = null!;
        public User Manager { get; set; } = null!;
    }
}
