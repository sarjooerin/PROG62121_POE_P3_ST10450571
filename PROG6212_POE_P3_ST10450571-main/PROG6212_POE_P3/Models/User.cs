using System;
using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE_P3.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public string? Role { get; set; } // HR, Lecturer, Coordinator, Manager

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string? Name { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Hourly rate must be greater than 0.")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; } = 0; // Only relevant for Lecturers

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}
