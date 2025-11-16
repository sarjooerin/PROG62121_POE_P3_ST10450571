using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE_P3.Models
{
    public class LecturerClaim
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0, 168, ErrorMessage = "Hours must be between 0 and 168")]
        public double HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0, 1000, ErrorMessage = "Hourly rate must be valid")]
        public double HourlyRate { get; set; }

        public string AdditionalNotes { get; set; }

        public List<ClaimDocuments> Documents { get; set; } = new List<ClaimDocuments>();

        public string Status { get; set; } = "Pending";
    }

    public class ClaimDocuments
    {
        public string FileName { get; set; }
        public string EncryptedContent { get; set; } // Base64 string of encrypted file
        public string ContentType { get; set; }
    }
}

