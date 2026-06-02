using GD1.Domain.Entities.Base;

namespace GD1.Domain.Entities
{
    public class ServiceCenterImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;

        public long? ApplicationId { get; set; }
        public ServiceCenterPartneringApplication? Application { get; set; }

        public long? ServiceCenterId { get; set; }
        public ServiceCenter? ServiceCenter { get; set; }
    }
}
