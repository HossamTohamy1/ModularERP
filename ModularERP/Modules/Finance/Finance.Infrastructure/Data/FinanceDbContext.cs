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
using System.Reflection;

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

            // Company Configuration - master entity in tenant DB
            builder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                // Company is the root entity - has foreign key relationships to major entities
                entity.HasMany(e => e.Treasuries)
                      .WithOne(e => e.Company)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.BankAccounts)
                      .WithOne(e => e.Company)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.GlAccounts)
                      .WithOne(e => e.Company)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Vouchers)
                      .WithOne(e => e.Company)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // GlAccount Configuration - has CompanyId FK + TenantId for isolation
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

                entity.HasMany(e => e.LedgerEntries)
                      .WithOne(e => e.GlAccount)
                      .HasForeignKey(e => e.GlAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Treasury Configuration - has CompanyId FK + TenantId for isolation
            builder.Entity<Treasury>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.DepositAcl).HasColumnType("nvarchar(max)");
                entity.Property(e => e.WithdrawAcl).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Currency)
                      .WithMany(e => e.Treasuries)
                      .HasForeignKey(e => e.CurrencyCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BankAccount Configuration - has CompanyId FK + TenantId for isolation
            builder.Entity<BankAccount>(entity =>
            {
                entity.Property(e => e.DepositAcl).HasColumnType("nvarchar(max)");
                entity.Property(e => e.WithdrawAcl).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Status).HasConversion<string>();

                entity.HasOne(e => e.Currency)
                      .WithMany(e => e.BankAccounts)
                      .HasForeignKey(e => e.CurrencyCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Tax Configuration - has TenantId for isolation (no CompanyId FK as per BRSD)
            builder.Entity<Tax>(entity =>
            {
                entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();

                entity.HasMany(e => e.VoucherTaxes)
                      .WithOne(e => e.Tax)
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Vendor Configuration - has TenantId for isolation (no CompanyId FK as per BRSD)
            builder.Entity<Vendor>(entity =>
            {
                entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
            });

            // Customer Configuration - has TenantId for isolation (no CompanyId FK as per BRSD)
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
            });

            // Voucher Configuration - has CompanyId FK + TenantId for isolation
            builder.Entity<Voucher>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.WalletType).HasConversion<string>();
                entity.Property(e => e.CounterpartyType).HasConversion<string>();

                // User relationships for audit trail
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

                entity.HasOne(e => e.Currency)
                      .WithMany(e => e.Vouchers)
                      .HasForeignKey(e => e.CurrencyCode)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.VoucherTaxes)
                      .WithOne(e => e.Voucher)
                      .HasForeignKey(e => e.VoucherId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.LedgerEntries)
                      .WithOne(e => e.Voucher)
                      .HasForeignKey(e => e.VoucherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Attachments)
                      .WithOne(e => e.Voucher)
                      .HasForeignKey(e => e.VoucherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.AuditLogs)
                      .WithOne(e => e.Voucher)
                      .HasForeignKey(e => e.VoucherId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.Date).HasDatabaseName("IX_Voucher_Date");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Voucher_Status");
                entity.HasIndex(e => e.Type).HasDatabaseName("IX_Voucher_Type");
                entity.HasIndex(e => new { e.WalletType, e.WalletId }).HasDatabaseName("IX_Voucher_Wallet");
                entity.HasIndex(e => new { e.CounterpartyType, e.CounterpartyId }).HasDatabaseName("IX_Voucher_Counterparty");
            });

            // VoucherTax Configuration - has TenantId for isolation
            builder.Entity<VoucherTax>(entity =>
            {
                entity.Property(e => e.Direction).HasConversion<string>();
                entity.Property(e => e.IsWithholding).HasDefaultValue(false);
            });

            // LedgerEntry Configuration - has TenantId for isolation
            builder.Entity<LedgerEntry>(entity =>
            {
                entity.HasOne(e => e.Currency)
                      .WithMany(e => e.LedgerEntries)
                      .HasForeignKey(e => e.CurrencyCode)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.EntryDate).HasDatabaseName("IX_LedgerEntry_Date");
                entity.HasIndex(e => e.GlAccountId).HasDatabaseName("IX_LedgerEntry_Account");
                entity.HasIndex(e => e.VoucherId).HasDatabaseName("IX_LedgerEntry_Voucher");
            });

            // Currency Configuration - global entity (no TenantId)
            builder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // VoucherAttachment Configuration - has TenantId for isolation
            builder.Entity<VoucherAttachment>(entity =>
            {
                entity.HasOne(e => e.UploadedByUser)
                      .WithMany(e => e.UploadedAttachments)
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // RecurringSchedule Configuration - has TenantId for isolation
            builder.Entity<RecurringSchedule>(entity =>
            {
                entity.Property(e => e.Frequency).HasConversion<string>();
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.CreatedSchedules)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog Configuration - has TenantId for isolation
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany(e => e.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.VoucherId).HasDatabaseName("IX_AuditLog_Voucher");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AuditLog_Timestamp");
            });

            // Apply Global Query Filters for Multi-tenancy
            ApplyGlobalFilters(builder);
        }

        // دالة لتطبيق Global Filters للـ Multi-tenancy
        private void ApplyGlobalFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = SetGlobalFilterMethod.MakeGenericMethod(entityType.ClrType);
                    method.Invoke(this, new object[] { builder });
                }
            }
        }

        private static readonly MethodInfo SetGlobalFilterMethod = typeof(FinanceDbContext)
            .GetMethod(nameof(SetGlobalFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static void SetGlobalFilter<T>(ModelBuilder builder) where T : class, ITenantEntity
        {
            // Global filter to automatically filter by TenantId
            // builder.Entity<T>().HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == currentTenantId);
        }
    }
}