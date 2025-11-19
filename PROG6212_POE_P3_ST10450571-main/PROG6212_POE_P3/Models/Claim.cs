using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PROG6212_POE_P3.Models
{
    // ClaimStatus Enum for workflow stages
    public enum ClaimStatus
    {
        Pending,    // Waiting for Coordinator Review
        Verified,   // Coordinator Approved, Waiting for Manager Review
        Approved,   // Final Manager Approval
        Rejected    // Rejected at any stage
    }

    public class Claim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer name is required.")]
        [Display(Name = "Lecturer Name")]
        public string? LecturerName { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [Range(1, 180, ErrorMessage = "Hours worked must be between 1 and 180.")]
        [Display(Name = "Hours Worked")]
        public int HoursWorked { get; set; }

        public decimal HourlyRate { get; set; }

        // Auto-calculated total payment
        [Display(Name = "Total Amount (R)")]
        public decimal TotalPayment => HoursWorked * HourlyRate;

        [Display(Name = "Notes / Additional Info")]
        public string? Notes { get; set; }

        // Original file name uploaded by user
        public string? OriginalFileName { get; set; }

        // Unique saved file name
        public string? UploadedFileName { get; set; }

        // Optional: Path for download/display (relative to wwwroot/uploads)
        public string? FilePath { get; set; }

        // File binding in forms
        [NotMapped]
        [Display(Name = "Supporting Document")]
        public IFormFile? DocumentUpload { get; set; }

        // Claim status (Pending, Verified, Approved, Rejected)
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        [Display(Name = "Coordinator / Manager Remarks")]
        public string? CoordinatorRemarks { get; set; }
    }
}
