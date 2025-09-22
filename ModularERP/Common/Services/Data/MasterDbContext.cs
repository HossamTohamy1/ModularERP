using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Models;

namespace ModularERP.Common.Services.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        public DbSet<MasterCompany> MasterCompanies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MasterCompany>(entity =>
            {
                entity.ToTable("MasterCompanies"); 
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();
            });
        }
    }
}
