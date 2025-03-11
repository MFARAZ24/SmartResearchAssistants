using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartResearchAssistance.Models;

namespace SmartResearchAssistance.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Paper> Papers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Paper>(entity =>
            {
                entity.Property(p => p.ExtractedText).HasColumnType("nvarchar(max)");
                entity.Property(p => p.Keywords).HasMaxLength(1000);
                entity.Property(p => p.OriginalFileName).IsRequired().HasMaxLength(255);
            });
        }
    }
}