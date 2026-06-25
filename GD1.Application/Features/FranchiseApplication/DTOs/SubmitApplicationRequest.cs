using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class SubmitApplicationRequest
    {
        [Required]
        public string ApplicationType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z\s]*$",
            ErrorMessage = "Owner name must contain only letters and spaces.")]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage ="Please Enter a valid address")]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City must contain only letters.")]
        public string City { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "State must contain only letters.")]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = "India";
        
        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal Code must be exactly 6 digits.")]
        public string PostalCode { get; set; } = string.Empty;

        public decimal PricePerDay { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime? PreferredInspectionDate { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? PropertyProofUrl { get; set; }

        [Required]
        public string FrontImageUrl { get; set; } = string.Empty;
        public List<string>? OtherImageUrls { get; set; }

        // Facilities
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }

        public List<GarageSlotRequest> Slots { get; set; } = [];

        [Required(ErrorMessage = "Razorpay payment is required.")]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Razorpay payment ID is required.")]
        public string RazorpayPaymentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Razorpay signature is required.")]
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class GarageSlotRequest
    {
        [Required]
        public string SlotNumber { get; set; } = string.Empty;
        
        [Range(1, double.MaxValue, ErrorMessage = "SquareFeet must be greater than 0")]
        public double SquareFeet { get; set; }
        
        [Range(1, double.MaxValue, ErrorMessage = "HeightFeet must be greater than 0")]
        public double HeightFeet { get; set; }
        
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
