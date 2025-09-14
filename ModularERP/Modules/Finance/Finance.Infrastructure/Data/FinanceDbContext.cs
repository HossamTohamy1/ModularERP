using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.AuditLogs.Models;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Modules.Finance.Features.Customer.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Modules.Finance.Features.LedgerEntries.Models;
using ModularERP.Modules.Finance.Features.RecurringSchedules.Models;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Vendor.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;


namespace ModularERP.Modules.Finance.Finance.Infrastructure.Data
{
    public class FinanceDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Company> Companies { get; set; }
        public DbSet<GlAccount> GlAccounts { get; set; }
        public DbSet<Treasury> Treasuries { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherTax> VoucherTaxes { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<VoucherAttachment> Attachments { get; set; }
        public DbSet<RecurringSchedule> RecurringSchedules { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables to use Guid
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
            });

            // Company Configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasMany(e => e.Treasuries).WithOne(e => e.Company).HasForeignKey(e => e.CompanyId);
                entity.HasMany(e => e.BankAccounts).WithOne(e => e.Company).HasForeignKey(e => e.CompanyId);
                entity.HasMany(e => e.GlAccounts).WithOne(e => e.Company).HasForeignKey(e => e.CompanyId);
                entity.HasMany(e => e.Vouchers).WithOne(e => e.Company).HasForeignKey(e => e.CompanyId);
            });

            // GlAccount Configuration
            builder.Entity<GlAccount>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();

                entity.HasMany(e => e.CategoryVouchers)
                      .WithOne(e => e.CategoryAccount)
                      .HasForeignKey(e => e.CategoryAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.JournalVouchers)
                      .WithOne(e => e.JournalAccount)
                      .HasForeignKey(e => e.JournalAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Treasury Configuration
            builder.Entity<Treasury>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();

                entity.Property(e => e.DepositAcl)
                      .HasColumnType("nvarchar(max)");

                entity.Property(e => e.WithdrawAcl)
                      .HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Currency)
                      .WithMany(e => e.Treasuries)
                      .HasForeignKey(e => e.CurrencyCode);
            });

            // BankAccount Configuration
            builder.Entity<BankAccount>(entity =>
            {
                entity.Property(e => e.DepositAcl)
                      .HasColumnType("nvarchar(max)");

                entity.Property(e => e.WithdrawAcl)
                      .HasColumnType("nvarchar(max)");
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne(e => e.Currency).WithMany(e => e.BankAccounts).HasForeignKey(e => e.CurrencyCode);
            });

            // Voucher Configuration
            builder.Entity<Voucher>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.WalletType).HasConversion<string>();
                entity.Property(e => e.CounterpartyType).HasConversion<string>();

                // User relationships
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.CreatedVouchers)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Poster)
                      .WithMany(e => e.PostedVouchers)
                      .HasForeignKey(e => e.PostedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reverser)
                      .WithMany(e => e.ReversedVouchers)
                      .HasForeignKey(e => e.ReversedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Currency).WithMany(e => e.Vouchers).HasForeignKey(e => e.CurrencyCode);
            });

            // VoucherTax Configuration
            builder.Entity<VoucherTax>(entity =>
            {
                entity.Property(e => e.Direction).HasConversion<string>();
                entity.HasOne(e => e.Voucher).WithMany(e => e.VoucherTaxes).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Tax).WithMany(e => e.VoucherTaxes).HasForeignKey(e => e.TaxId);
            });

            // LedgerEntry Configuration
            builder.Entity<LedgerEntry>(entity =>
            {
                entity.HasOne(e => e.Voucher).WithMany(e => e.LedgerEntries).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.GlAccount).WithMany(e => e.LedgerEntries).HasForeignKey(e => e.GlAccountId);
                entity.HasOne(e => e.Currency).WithMany(e => e.LedgerEntries).HasForeignKey(e => e.CurrencyCode);
            });

            // Tax Configuration
            builder.Entity<Tax>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();
            });

            // Vendor Configuration
            builder.Entity<Vendor>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Customer Configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Currency Configuration
            builder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Code);
            });

            // Attachment Configuration
            builder.Entity<VoucherAttachment>(entity =>
            {
                entity.HasOne(e => e.Voucher).WithMany(e => e.Attachments).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UploadedByUser).WithMany(e => e.UploadedAttachments).HasForeignKey(e => e.UploadedBy);
            });

            // RecurringSchedule Configuration
            builder.Entity<RecurringSchedule>(entity =>
            {
                entity.Property(e => e.Frequency).HasConversion<string>();
                entity.HasOne(e => e.Creator).WithMany(e => e.CreatedSchedules).HasForeignKey(e => e.CreatedBy);
            });

            // AuditLog Configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(e => e.Voucher).WithMany(e => e.AuditLogs).HasForeignKey(e => e.VoucherId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User).WithMany(e => e.AuditLogs).HasForeignKey(e => e.UserId);
            });
        }
    }
}
