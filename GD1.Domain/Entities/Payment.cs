using GD1.Domain.Entities.Base;
using System;

namespace GD1.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public long BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public string RazorpayOrderId { get; set; } = string.Empty;
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }

        public decimal TotalAmount { get; set; } // The total advance amount the user paid (including pickup charge if any)
        public decimal AdminCutAmount { get; set; } // The 15% of the 3-day storage portion
        public decimal PropertyOwnerAmount { get; set; } // The 85% of the 3-day storage portion + Pickup Charge
        public decimal PickupChargeAmount { get; set; } // The portion of the amount that was for the pickup charge

        public string Status { get; set; } = "created"; // 'created', 'paid', 'failed'
    }
}
