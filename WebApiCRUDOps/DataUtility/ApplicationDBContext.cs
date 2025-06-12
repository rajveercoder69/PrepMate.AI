using Microsoft.EntityFrameworkCore;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSet properties for each entity you want to interact with
        public DbSet<Person> personDetail { get; set; }
        public DbSet<PdfUpload> PdfUploads { get; set; }
        public DbSet<LearningPdf> LearningPdfs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.firstName)
                .IsUnique();
            modelBuilder.Entity<PdfUpload>()
          .HasOne(p => p.Person)
          .WithMany()
          .HasForeignKey(p => p.PersonId)
          .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LearningPdf>() 
                .HasOne(p=>p.Person)
                .WithMany()
                .HasForeignKey(p => p.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
