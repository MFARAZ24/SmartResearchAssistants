using System.ComponentModel.DataAnnotations;

namespace SmartResearchAssistance.Models
{
    public class Paper
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = null!;

        [StringLength(500)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Authors { get; set; }

        public DateTime? PublicationDate { get; set; }

        public string? ExtractedText { get; set; }

        [StringLength(1000)]
        public string? Keywords { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [Required]
        [StringLength(450)]
        public string UploadedBy { get; set; } = null!;
    }
}