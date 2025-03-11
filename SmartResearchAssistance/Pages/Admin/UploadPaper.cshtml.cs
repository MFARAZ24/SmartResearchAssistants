using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartResearchAssistance.Services;
using SmartResearchAssistance.Data;
using SmartResearchAssistance.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartResearchAssistance.Pages.Admin
{
    [Authorize]
    public class UploadPaperModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;
        private readonly IWebHostEnvironment _env;

        public UploadPaperModel(
            ApplicationDbContext context,
            PdfService pdfService,
            IWebHostEnvironment env)
        {
            _context = context;
            _pdfService = pdfService;
            _env = env;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select a file to upload.")]
        public IFormFile Upload { get; set; } = null!;

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Upload == null || Upload.Length == 0)
            {
                ModelState.AddModelError("Upload", "Please select a file to upload.");
                return Page();
            }

            // Manually check for .pdf extension (case-insensitive)
            var extension = Path.GetExtension(Upload.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Upload", "Only PDF files are allowed.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string filePath = string.Empty;

            try
            {
                // Generate a unique filename for storage
                var fileName = $"{Guid.NewGuid()}.pdf";
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);
                filePath = Path.Combine(uploadsDir, fileName);

                // Save the uploaded file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }

                // Extract text and metadata from the PDF
                var (text, metadata) = _pdfService.ExtractContent(filePath);

                // Fallback: If metadata doesn't provide a title, use the filename (without extension)
                string title = "";
                if (metadata.TryGetValue("Title", out var extractedTitle) && !string.IsNullOrWhiteSpace(extractedTitle))
                {
                    title = extractedTitle;
                }
                else
                {
                    title = Path.GetFileNameWithoutExtension(Upload.FileName);
                }

                var frequencies = _pdfService.AnalyzeWordFrequency(text);
                var keywords = _pdfService.ExtractKeywords(frequencies);

                // Create a new Paper object and save it to the database
                var paper = new Paper
                {
                    OriginalFileName = Upload.FileName,
                    StoredFileName = fileName,
                    Title = title,
                    Authors = metadata.ContainsKey("Author") ? metadata["Author"] : "",
                    PublicationDate = metadata.ContainsKey("Date") && DateTime.TryParse(metadata["Date"], out var date) ? date : null,
                    ExtractedText = text,
                    Keywords = string.Join(", ", keywords),
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = User.Identity?.Name ?? "System"
                };

                _context.Papers.Add(paper);
                await _context.SaveChangesAsync();

                Message = "File uploaded successfully!";
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";

                // If an error occurs, delete the saved file if it exists
                if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Page();
        }
    }
}
