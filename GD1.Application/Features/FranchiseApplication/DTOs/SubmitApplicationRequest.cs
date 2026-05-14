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

        public DateTime? PreferredInspectionDate { get; set; }

        [Url(ErrorMessage = "Invalid BusinessRegistration Image URL.")]
        public string? BusinessRegistrationUrl { get; set; }

        [Url(ErrorMessage = "Invalid LicenseDocument URL.")]
        public string? LicenseDocumentUrl { get; set; }

        [Url(ErrorMessage = "Invalid OwnerIdProof URL.")]
        public string? OwnerIdProofUrl { get; set; }

        [Url(ErrorMessage = "Invalid PropertyProof Image URL.")]
        public string? PropertyProofUrl { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid FrontImage Image URL.")]
        public string FrontImageUrl { get; set; } = string.Empty;

        public List<string>? OtherImageUrls { get; set; }



        [MinLength(1, ErrorMessage = "At least one lot unit is required.")]
        public List<LotUnitRequest> LotUnits { get; set; } = [];
    }
}
