using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Review : BaseEntity
    {
        public long LotId { get; set; }
        public long ReviewerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? SentimentScore { get; set; }

        public StorageLot Lot { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
    }
}

