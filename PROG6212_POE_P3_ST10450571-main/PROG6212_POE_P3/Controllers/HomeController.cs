using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PROG6212_POE_P3.Models;

// PDF libraries
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PROG6212_POE_P3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        // --- In-memory claims list ---
        private static List<Claim> _claims = new List<Claim>();

        // --- In-memory users list ---
        private static List<User> _users = new List<User>()
        {
            // Note: Added Surname to the initial Lecturer
            new User { Id = 1, Username = "lecturer1", Password = "pass123", Role = "Lecturer", FirstName = "Lec 1", Surname = "Jones", HourlyRate = 150 },
            new User { Id = 2, Username = "hr1", Password = "hr123", Role = "HR", FirstName = "HR", Surname = "Admin", HourlyRate = 0 },
            new User { Id = 3, Username = "admin1", Password = "coord123", Role = "Admin1", FirstName = "Coord 1", Surname = "Smith", HourlyRate = 0 },
            new User { Id = 4, Username = "admin2", Password = "manager123", Role = "Admin2", FirstName = "Manager 1", Surname = "Brown", HourlyRate = 0 }
        };

        static HomeController()
        {
            if (!_claims.Any())
            {
                // Added a claim using the initial user's first name
                _claims.Add(new Claim { Id = 1, LecturerName = "Lec 1", DateSubmitted = DateTime.Now.AddDays(-5), HoursWorked = 10, HourlyRate = 150.00m, Status = ClaimStatus.Pending, UploadedFileName = "doc1.pdf", CoordinatorRemarks = "" });
                _claims.Add(new Claim { Id = 2, LecturerName = "Lec 2", DateSubmitted = DateTime.Now.AddDays(-4), HoursWorked = 25, HourlyRate = 180.00m, Status = ClaimStatus.Verified, UploadedFileName = "doc2.docx", CoordinatorRemarks = "Coordinator: Verified hours and document." });
                _claims.Add(new Claim { Id = 3, LecturerName = "Lec 3", DateSubmitted = DateTime.Now.AddDays(-3), HoursWorked = 5, HourlyRate = 100.00m, Status = ClaimStatus.Approved, UploadedFileName = "doc3.pdf", CoordinatorRemarks = "Coordinator: Verified. Manager: Final Approved." });
                _claims.Add(new Claim { Id = 4, LecturerName = "Lec 4", DateSubmitted = DateTime.Now.AddDays(-2), HoursWorked = 30, HourlyRate = 200.00m, Status = ClaimStatus.Rejected, UploadedFileName = "doc4.pdf", CoordinatorRemarks = "Coordinator: Invalid hours." });
            }
        }

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        // ----------------------------
        // NAVIGATION & LOGIN
        // ----------------------------
        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Name", user.FirstName);

            // Conditional Redirect: HR users go straight to HRMain
            if (user.Role == "HR")
            {
                return RedirectToAction("HRMain");
            }

            // Other users (Lecturer, Admin1, Admin2) go to the general MainMenu
            return RedirectToAction("MainMenu");
        }

        // ----------------------------
        // MAIN MENU
        // ----------------------------
        [HttpGet]
        public IActionResult MainMenu()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            ViewBag.Username = username;
            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.Error = TempData["Error"];
            return View();
        }

        // ----------------------------
        // ROLE CHECK HELPER
        // ----------------------------
        private IActionResult AuthorizeRole(params string[] roles)
        {
            // Get the user role from the session
            var userRole = HttpContext.Session.GetString("Role");

            // Check if the role exists and if it's one of the allowed roles
            if (string.IsNullOrEmpty(userRole) || !roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                // If the user doesn't have access, redirect them to the MainMenu
                TempData["Error"] = "You do not have access to this page.";
                return RedirectToAction("MainMenu");
            }

            // User is authorized
            return null;
        }


        // ----------------------------
        // DASHBOARD & CLAIMS
        // ----------------------------
        [HttpGet]
        public IActionResult Dashboard()
        {
            // Role-based authorization: Check the role of the logged-in user
            var redirect = AuthorizeRole("Lecturer", "HR", "Admin1", "Admin2");
            if (redirect != null) return redirect;

            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            // Admins can see all claims
            if (role == "Admin1" || role == "Admin2")
            {
                return View(_claims); // Show all claims to admins
            }

            // Lecturers can only see their own claims
            if (role == "Lecturer")
            {
                var lecturer = _users.FirstOrDefault(u => u.Username == username && u.Role == "Lecturer");
                if (lecturer == null) return RedirectToAction("MainMenu");

                var lecturerClaims = _claims.Where(c => c.LecturerName == lecturer.FirstName).ToList();
                return View(lecturerClaims); // Show claims specific to the logged-in lecturer
            }

            // HR should not be accessing this page directly, but included for completeness if role access changes
            if (role == "HR")
            {
                return RedirectToAction("HRMain");
            }

            return RedirectToAction("MainMenu");
        }


        [HttpGet]
        public IActionResult ClaimForm()
        {
            var redirect = AuthorizeRole("Lecturer");
            if (redirect != null) return redirect;

            return View(new Claim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClaimForm(Claim claim)
        {
            var redirect = AuthorizeRole("Lecturer");
            if (redirect != null) return redirect;

            var username = HttpContext.Session.GetString("Username");
            var lecturer = _users.FirstOrDefault(u => u.Username == username && u.Role == "Lecturer");

            if (lecturer == null)
            {
                ViewBag.Error = "Lecturer not found.";
                return View(claim);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please correct the highlighted validation errors.";
                return View(claim);
            }

            claim.LecturerName = lecturer.FirstName;
            claim.HourlyRate = lecturer.HourlyRate;

            if (claim.HoursWorked > 180)
            {
                ViewBag.Error = "Hours worked cannot exceed 180 per month.";
                return View(claim);
            }

            if (claim.DocumentUpload != null && claim.DocumentUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(claim.DocumentUpload.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    claim.DocumentUpload.CopyTo(fileStream);
                    claim.UploadedFileName = uniqueFileName;
                    claim.Notes = string.IsNullOrEmpty(claim.Notes)
                        ? "File uploaded successfully."
                        : claim.Notes + " | File uploaded successfully.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save document for claim.");
                    ViewBag.Error = "Failed to upload file. Please try again.";
                    return View(claim);
                }
            }

            claim.Id = _claims.Count + 1;
            claim.DateSubmitted = DateTime.Now;
            claim.Status = ClaimStatus.Pending;

            _claims.Add(claim);
            return RedirectToAction("Dashboard");
        }

        // ----------------------------
        // COORDINATOR (Admin1)
        // ----------------------------
        [HttpGet]
        public IActionResult Coordinator()
        {
            // Authorization: Only Admin1 (Coordinator) can access this page
            var redirect = AuthorizeRole("Admin1");
            if (redirect != null) return redirect;

            // Filter claims: Show claims that are in the 'Pending' status (ready for Coordinator review)
            var claimsToReview = _claims.Where(c => c.Status == ClaimStatus.Pending).ToList();

            // You will need a view named 'Coordinator.cshtml' to display this list
            return View(claimsToReview);
        }

        // ----------------------------
        // ACADEMIC MANAGER (Admin2)
        // ----------------------------
        [HttpGet]
        public IActionResult AcademicManager()
        {
            // Authorization: Only Admin2 (Academic Manager) can access this page
            var redirect = AuthorizeRole("Admin2");
            if (redirect != null) return redirect;

            // Filter claims: Show claims that are in the 'Verified' status (ready for final approval)
            var claimsToReview = _claims.Where(c => c.Status == ClaimStatus.Verified).ToList();

            // You will need a view named 'AcademicManager.cshtml' to display this list
            return View(claimsToReview);
        }

        // ----------------------------
        // UPDATE CLAIM STATUS
        // ----------------------------
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string remarks)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return RedirectToAction("MainMenu");

            var originalStatus = claim.Status;
            if (Enum.TryParse(status, out ClaimStatus newStatus))
                claim.Status = newStatus;

            if (!string.IsNullOrEmpty(remarks))
            {
                string prefix = originalStatus switch
                {
                    ClaimStatus.Pending => $"Coordinator ({DateTime.Now:yyyy-MM-dd}): ",
                    ClaimStatus.Verified => $"Manager ({DateTime.Now:yyyy-MM-dd}): ",
                    _ => ""
                };

                claim.CoordinatorRemarks = prefix + remarks +
                    (string.IsNullOrEmpty(claim.CoordinatorRemarks) ? "" : "\n---PREVIOUS---\n" + claim.CoordinatorRemarks);
            }

            return originalStatus switch
            {
                ClaimStatus.Pending => RedirectToAction("Coordinator"),
                ClaimStatus.Verified => RedirectToAction("AcademicManager"),
                _ => RedirectToAction("MainMenu")
            };
        }

        // ----------------------------
        // HR / USER MANAGEMENT
        // ----------------------------
        [HttpGet]
        public IActionResult HRMain()
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];

            return View(_users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User user)
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return View(user);

            user.Id = _users.Count + 1;
            _users.Add(user);

            return RedirectToAction("HRMain");
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User updatedUser)
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return View(updatedUser);

            var user = _users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null) return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.Surname = updatedUser.Surname;
            user.Email = updatedUser.Email;
            user.Username = updatedUser.Username;
            user.Password = updatedUser.Password;
            user.Role = updatedUser.Role;
            user.HourlyRate = updatedUser.HourlyRate;

            return RedirectToAction("HRMain");
        }

        // ----------------------------
        // DELETE USER
        // ----------------------------
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("HRMain");
            }

            var currentUser = HttpContext.Session.GetString("Username");
            if (user.Username == currentUser)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("HRMain");
            }

            _users.Remove(user);

            TempData["Success"] = $"User '{user.Username}' deleted successfully.";
            return RedirectToAction("HRMain");
        }

        // ------------------------------------------------------
        // PDF EXPORT FEATURE
        // ------------------------------------------------------

        /// <summary>
        /// Generates a PDF report summarizing all claims with 'Approved' status for payroll purposes.
        /// Accessible only by HR role.
        /// </summary>
        [HttpGet]
        public IActionResult GenerateReport()
        {
            var redirect = AuthorizeRole("HR");
            if (redirect != null) return redirect;

            // Filter to approved claims that are ready for payroll
            var approvedClaims = _claims.Where(c => c.Status == ClaimStatus.Approved).ToList();

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Report Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                document.Add(new Paragraph("Payroll Claims Report\n\n", titleFont) { Alignment = Element.ALIGN_CENTER });

                if (!approvedClaims.Any())
                {
                    document.Add(new Paragraph("No approved claims available for payroll at this time.", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    document.Close();
                    return File(ms.ToArray(), "application/pdf", $"Payroll_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                }

                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Table for Claims Data
                PdfPTable table = new PdfPTable(7); // Corrected to 7 columns
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 2f, 2f, 1.5f, 1.5f, 1.5f, 2f }); // Corrected widths for 7 columns

                // Table Headers
                foreach (string header in new[] { "ID", "First Name", "Surname", "Date", "Hours", "Rate (R)", "Amount (R)" }) // Corrected headers
                {
                    var cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)))
                    {
                        BackgroundColor = new Color(108, 99, 255), // #6C63FF
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8,
                        Border = 0
                    };
                    table.AddCell(cell);
                }

                decimal grandTotal = 0;

                // Table Rows
                foreach (var claim in approvedClaims)
                {
                    // FIX: Look up the user to get first name and surname
                    var user = _users.FirstOrDefault(u => u.FirstName == claim.LecturerName);

                    string firstName = user?.FirstName ?? claim.LecturerName;
                    string surname = user?.Surname ?? "N/A";

                    table.AddCell(new Phrase(claim.Id.ToString(), normal));
                    table.AddCell(new Phrase(firstName, normal));
                    table.AddCell(new Phrase(surname, normal));
                    table.AddCell(new Phrase(claim.DateSubmitted.ToString("yyyy-MM-dd"), normal));
                    table.AddCell(new Phrase(claim.HoursWorked.ToString(), normal));
                    table.AddCell(new Phrase(claim.HourlyRate.ToString("0.00"), normal));

                    decimal totalAmount = claim.HoursWorked * claim.HourlyRate;
                    table.AddCell(new Phrase(totalAmount.ToString("0.00"), normal));
                    grandTotal += totalAmount;
                }

                document.Add(table);

                // Summary/Total
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                document.Add(new Paragraph("\n---", totalFont) { Alignment = Element.ALIGN_RIGHT });
                document.Add(new Paragraph($"Grand Total for Approved Claims: R{grandTotal:0.00}", totalFont) { Alignment = Element.ALIGN_RIGHT });

                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Payroll_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
        }

        [HttpGet]
        public IActionResult GenerateClaimPDF(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound("Claim not found.");

            if (role == "Lecturer" && claim.LecturerName != HttpContext.Session.GetString("Name"))
                return Unauthorized();

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                var header = new Paragraph("Claim Summary Report\n\n", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(header);

                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                // FIX: Look up the user to get first name and surname
                var user = _users.FirstOrDefault(u => u.FirstName == claim.LecturerName);
                string firstName = user?.FirstName ?? claim.LecturerName;
                string surname = user?.Surname ?? "N/A";


                document.Add(new Paragraph($"Claim ID: {claim.Id}", normal));
                document.Add(new Paragraph($"Name: {firstName}", normal));
                document.Add(new Paragraph($"Surname: {surname}", normal));
                document.Add(new Paragraph($"Date Submitted: {claim.DateSubmitted:yyyy-MM-dd}", normal));
                document.Add(new Paragraph($"Hours Worked: {claim.HoursWorked}", normal));
                document.Add(new Paragraph($"Hourly Rate: R{claim.HourlyRate}", normal));
                document.Add(new Paragraph($"Total Amount: R{claim.HoursWorked * claim.HourlyRate}", normal));
                document.Add(new Paragraph($"Status: {claim.Status}", normal));
                document.Add(new Paragraph("\nCoordinator / Manager Remarks:", normal));
                document.Add(new Paragraph(claim.CoordinatorRemarks ?? "None", normal));

                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Claim_{claim.Id}.pdf");
            }
        }

        // ----------------------------
        // LOGOUT
        // ----------------------------
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}