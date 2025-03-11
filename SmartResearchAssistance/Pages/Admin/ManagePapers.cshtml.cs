using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartResearchAssistance.Data;
using SmartResearchAssistance.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartResearchAssistance.Pages.Admin
{
    public class ManagePapersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public List<Paper> Papers { get; set; } = new();

        public ManagePapersModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public void OnGet()
        {
            Papers = _context.Papers
                .OrderByDescending(p => p.UploadDate)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var paper = _context.Papers.FirstOrDefault(p => p.Id == id);
            if (paper != null)
            {
                // Delete the file from disk
                var filePath = Path.Combine(_env.WebRootPath, "uploads", paper.StoredFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                // Remove from database
                _context.Papers.Remove(paper);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}
