using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PROG6212_POE_P2.Models;

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
            new User { Id = 1, Username = "lecturer1", Password = "pass123", Role = "Lecturer", Name="Lec 1", HourlyRate=150 },
            new User { Id = 2, Username = "hr1", Password = "admin123", Role = "HR", Name="HR Admin", HourlyRate=0 },
            new User { Id = 3, Username = "coordinator1", Password = "coord123", Role = "Coordinator", Name="Coord 1", HourlyRate=0 },
            new User { Id = 4, Username = "manager1", Password = "manager123", Role = "Manager", Name="Manager 1", HourlyRate=0 },
        };

        // --- Dummy claims for testing ---
        static HomeController()
        {
            if (!_claims.Any())
            {
                _claims.Add(new Claim
                {
                    Id = 1,
                    LecturerName = "Lec 1",
                    DateSubmitted = DateTime.Now.AddDays(-5),
                    HoursWorked = 10,
                    HourlyRate = 150.00m,
                    Status = ClaimStatus.Pending,
                    UploadedFileName = "doc1.pdf",
                    CoordinatorRemarks = ""
                });
                _claims.Add(new Claim
                {
                    Id = 2,
                    LecturerName = "Lec 2",
                    DateSubmitted = DateTime.Now.AddDays(-4),
                    HoursWorked = 25,
                    HourlyRate = 180.00m,
                    Status = ClaimStatus.Verified,
                    UploadedFileName = "doc2.docx",
                    CoordinatorRemarks = "Coordinator: Verified hours and document."
                });
                _claims.Add(new Claim
                {
                    Id = 3,
                    LecturerName = "Lec 3",
                    DateSubmitted = DateTime.Now.AddDays(-3),
                    HoursWorked = 5,
                    HourlyRate = 100.00m,
                    Status = ClaimStatus.Approved,
                    UploadedFileName = "doc3.pdf",
                    CoordinatorRemarks = "Coordinator: Verified. Manager: Final Approved."
                });
                _claims.Add(new Claim
                {
                    Id = 4,
                    LecturerName = "Lec 4",
                    DateSubmitted = DateTime.Now.AddDays(-2),
                    HoursWorked = 30,
                    HourlyRate = 200.00m,
                    Status = ClaimStatus.Rejected,
                    UploadedFileName = "doc4.pdf",
                    CoordinatorRemarks = "Coordinator: Invalid hours."
                });
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

            // Store user session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect by role
            return user.Role switch
            {
                "Lecturer" => RedirectToAction("Dashboard"),
                "HR" => RedirectToAction("HRMain"),
                "Coordinator" => RedirectToAction("Coordinator"),
                "Manager" => RedirectToAction("AcademicManager"),
                _ => RedirectToAction("Index")
            };
        }

        [HttpGet]
        public IActionResult MainMenu() => View();

        // ----------------------------
        // LECTURER DASHBOARD & CLAIMS
        // ----------------------------
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!CheckRole("Lecturer")) return RedirectToAction("Index");

            string username = HttpContext.Session.GetString("Username");
            var lecturer = _users.FirstOrDefault(u => u.Username == username);
            var lecturerClaims = _claims.Where(c => c.LecturerName == lecturer.Name).ToList();
            return View(lecturerClaims);
        }

        [HttpGet]
        public IActionResult ClaimForm()
        {
            if (!CheckRole("Lecturer")) return RedirectToAction("Index");
            return View(new Claim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClaimForm(Claim claim)
        {
            if (!CheckRole("Lecturer")) return RedirectToAction("Index");

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please correct the highlighted validation errors.";
                return View(claim);
            }

            string username = HttpContext.Session.GetString("Username");
            var lecturer = _users.FirstOrDefault(u => u.Username == username && u.Role == "Lecturer");
            if (lecturer == null)
            {
                ViewBag.Error = "Lecturer not found.";
                return View(claim);
            }

            claim.LecturerName = lecturer.Name;
            claim.HourlyRate = lecturer.HourlyRate;

            if (claim.HoursWorked > 180)
            {
                ViewBag.Error = "Hours worked cannot exceed 180 per month.";
                return View(claim);
            }

            // File upload
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
                    claim.Notes = string.IsNullOrEmpty(claim.Notes) ? "File uploaded successfully." : claim.Notes + " | File uploaded successfully.";
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
        // COORDINATOR DASHBOARD
        // ----------------------------
        [HttpGet]
        public IActionResult Coordinator()
        {
            if (!CheckRole("Coordinator")) return RedirectToAction("Index");

            var pendingClaims = _claims.Where(c => c.Status == ClaimStatus.Pending).ToList();
            return View(pendingClaims);
        }

        // ----------------------------
        // MANAGER DASHBOARD
        // ----------------------------
        [HttpGet]
        public IActionResult AcademicManager()
        {
            if (!CheckRole("Manager")) return RedirectToAction("Index");

            var verifiedClaims = _claims.Where(c => c.Status == ClaimStatus.Verified).ToList();
            return View(verifiedClaims);
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
            if (!CheckRole("HR")) return RedirectToAction("Index");

            return View(_users);
        }

        [HttpGet]
        public IActionResult AddUser() => View(new User());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User user)
        {
            if (!CheckRole("HR")) return RedirectToAction("Index");

            if (!ModelState.IsValid) return View(user);

            user.Id = _users.Count + 1;
            _users.Add(user);

            return RedirectToAction("HRMain");
        }

        // ----------------------------
        // HELPER METHODS
        // ----------------------------
        private bool CheckRole(string role)
        {
            string sessionRole = HttpContext.Session.GetString("Role");
            return sessionRole == role;
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}