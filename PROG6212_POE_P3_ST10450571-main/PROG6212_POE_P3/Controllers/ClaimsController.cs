
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using PROG6212_POE_P2.Models;
using PROG6212_POE_P2.Services;
using System.Linq;

namespace PROG6212_POE_P3.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ClaimStore _store;
        private readonly IWebHostEnvironment _env;
        private readonly long MAX_FILE_BYTES = 5 * 1024 * 1024; // 5MB
        private readonly string[] ALLOWED = new[] { ".pdf", ".docx", ".doc", ".xlsx" };

        public ClaimsController(ClaimStore store, IWebHostEnvironment env)
        {
            _store = store;
            _env = env;
        }

        // Lecturer: form page
        [HttpGet]
        public IActionResult Create() => View();

        // Lecturer: handle form post
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile uploadedFile, string lecturerName, int hoursWorked, decimal hourlyRate, string notes)
        {
            if (string.IsNullOrWhiteSpace(lecturerName) || hoursWorked <= 0 || hourlyRate <= 0)
            {
                ModelState.AddModelError("", "Please provide valid inputs.");
            }

            string savedFileName = null;
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                if (uploadedFile.Length > MAX_FILE_BYTES)
                    ModelState.AddModelError("upload", "File too large (max 5 MB).");

                var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
                if (!ALLOWED.Contains(ext))
                    ModelState.AddModelError("upload", "File type not allowed.");

                if (ModelState.IsValid)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    savedFileName = $"{Path.GetRandomFileName()}{ext}";
                    var filePath = Path.Combine(uploads, savedFileName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fs);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var claim = new Claim
            {
                LecturerName = lecturerName,
                HoursWorked = hoursWorked,
                HourlyRate = hourlyRate,
                Notes = notes,
                UploadedFileName = savedFileName
            };

            _store.Add(claim);
            TempData["Success"] = "Claim submitted successfully.";
            return RedirectToAction("Create");
        }

        // Coordinator: review page
        [HttpGet]
        public IActionResult Review()
        {
            var claims = _store.GetAll();
            return View(claims);
        }

        // AJAX approve/reject
        [HttpPost]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest req)
        {
            var claim = _store.Get(req.Id);
            if (claim == null) return Json(new { success = false, message = "Claim not found" });

            if (req.Action == "approve") claim.Status = ClaimStatus.Approved;
            else if (req.Action == "reject") claim.Status = ClaimStatus.Rejected;
            else return Json(new { success = false, message = "Invalid action" });

            claim.CoordinatorRemarks = req.Remarks;
            _store.Update(claim);
            return Json(new { success = true, status = claim.Status.ToString() });
        }

        // Serve file for preview
        public IActionResult Preview(string file)
        {
            if (string.IsNullOrEmpty(file)) return NotFound();
            var path = Path.Combine(_env.WebRootPath, "uploads", file);
            if (!System.IO.File.Exists(path)) return NotFound();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var contentType = ext == ".pdf" ? "application/pdf" : "application/octet-stream";
            return PhysicalFile(path, contentType);
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Optional: validate credentials here
            return View();
        }
    }

    


    public class UpdateStatusRequest { public int Id { get; set; } public string Action { get; set; } public string Remarks { get; set; } }
}
