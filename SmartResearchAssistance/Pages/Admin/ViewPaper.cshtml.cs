using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartResearchAssistance.Data;
using SmartResearchAssistance.Models;
using SmartResearchAssistance.Services;
using System.Linq;
using System.Collections.Generic;

namespace SmartResearchAssistance.Pages.Admin
{
    public class ViewPaperModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public Paper Paper { get; set; } = null!;
        public string WordCloudJson { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new List<string>();

        public ViewPaperModel(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public IActionResult OnGet(int id)
        {
            Paper? paper = _context.Papers.FirstOrDefault(p => p.Id == id);
            if (paper == null)
            {
                return NotFound();
            }

            Paper = paper;

            if (!string.IsNullOrEmpty(Paper.ExtractedText))
            {
                // Analyze word frequencies on the stored text
                var frequencies = _pdfService.AnalyzeWordFrequency(Paper.ExtractedText);
                WordCloudJson = _pdfService.GetWordCloudJson(frequencies);

                // Optionally extract top N keywords
                Keywords = _pdfService.ExtractKeywords(frequencies);
            }

            return Page();
        }
    }
}
