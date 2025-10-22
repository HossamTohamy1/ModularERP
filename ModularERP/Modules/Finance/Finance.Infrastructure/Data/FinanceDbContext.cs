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
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Vendor.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Common.Services;
using System.Reflection;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Services.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using System.Reflection.Emit;
using ModularERP.Modules.Inventory.Features.Warehouses.Configurations;

namespace ModularERP.Modules.Finance.Finance.Infrastructure.Data
{
    public class FinanceDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ITenantService? _tenantService;

        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
        {
        }

        public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        // Finance DbSets
        public DbSet<Company> Companies { get; set; }
        public DbSet<GlAccount> GlAccounts { get; set; }
        public DbSet<Treasury> Treasuries { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherTax> VoucherTaxes { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<VoucherAttachment> Attachments { get; set; }
        public DbSet<RecurringSchedule> RecurringSchedules { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Inventory DbSets
        public DbSet<WarehouseStock> WarehouseStocks { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryAttachment> CategoryAttachments { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<UnitTemplate> UnitTemplates { get; set; }
        public DbSet<UnitConversion> UnitConversions { get; set; }
        public DbSet<BarcodeSettings> BarcodeSettings { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<TaxComponent> TaxComponents { get; set; }
        public DbSet<TaxProfile> TaxProfiles { get; set; }
        public DbSet<TaxProfileComponent> TaxProfileComponents { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ProductTaxProfile> ProductTaxProfiles { get; set; }
        public DbSet<ServiceTaxProfile> ServiceTaxProfiles { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<ItemGroupItem> ItemGroupItems { get; set; }
        public DbSet<ProductCustomFieldValue> ProductCustomFieldValues { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<ProductStats> ProductStats { get; set; }
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<RequisitionAttachment> RequisitionAttachments { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<RequisitionApprovalLog> RequisitionApprovalLogs { get; set; }
        public DbSet<StocktakingHeader> StocktakingHeaders { get; set; }
        public DbSet<StocktakingLine> StocktakingLines { get; set; }
        public DbSet<StocktakingAttachment> StocktakingAttachments { get; set; }
        public DbSet<StockSnapshot> StockSnapshots { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<PriceListItem> PriceListItems { get; set; }
        public DbSet<PriceListRule> PriceListRules { get; set; }
        public DbSet<BulkDiscount> BulkDiscounts { get; set; }
        public DbSet<PriceListAssignment> PriceListAssignments { get; set; }
        public DbSet<PriceCalculationLog> PriceCalculationLogs { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ProductTimeline> ProductTimelines { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables to use Guid with TenantId
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            builder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.ToTable("AspNetRoles");
            });

            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
            });

            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("AspNetUserClaims");
            });

            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("AspNetUserLogins");
            });

            builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("AspNetRoleClaims");
            });

            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("AspNetUserTokens");
            });

            // Company Configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();

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

                entity.HasMany(e => e.LedgerEntries)
                      .WithOne(e => e.GlAccount)
                      .HasForeignKey(e => e.GlAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Treasury Configuration
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

            // BankAccount Configuration
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



            // Vendor Configuration
            builder.Entity<Vendor>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();

                // ✅ إضافة علاقة Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ إضافة Index على CompanyId
                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_Vendor_Company");
            });

            // Customer Configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();

                // ✅ إضافة علاقة Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ إضافة Index على CompanyId
                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_Customer_Company");
            });

            // Voucher Configuration
            builder.Entity<Voucher>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.WalletType).HasConversion<string>();
                entity.Property(e => e.CounterpartyType).HasConversion<string>();

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

                entity.HasIndex(e => e.Date).HasDatabaseName("IX_Voucher_Date");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Voucher_Status");
                entity.HasIndex(e => e.Type).HasDatabaseName("IX_Voucher_Type");
                entity.HasIndex(e => new { e.WalletType, e.WalletId }).HasDatabaseName("IX_Voucher_Wallet");
                entity.HasIndex(e => new { e.CounterpartyType, e.CounterpartyId }).HasDatabaseName("IX_Voucher_Counterparty");
            });

            // VoucherTax Configuration
            builder.Entity<VoucherTax>(entity =>
            {
                entity.Property(e => e.Direction).HasConversion<string>();
                entity.Property(e => e.IsWithholding).HasDefaultValue(false);
            });

            // LedgerEntry Configuration
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

            // Currency Configuration
            builder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // VoucherAttachment Configuration
            builder.Entity<VoucherAttachment>(entity =>
            {
                entity.HasOne(e => e.UploadedByUser)
                      .WithMany(e => e.UploadedAttachments)
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // RecurringSchedule Configuration
            builder.Entity<RecurringSchedule>(entity =>
            {
                entity.Property(e => e.Frequency).HasConversion<string>();
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.CreatedSchedules)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog Configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany(e => e.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.VoucherId).HasDatabaseName("IX_AuditLog_Voucher");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AuditLog_Timestamp");
            });

            // ============================================
            // INVENTORY MODULE CONFIGURATIONS
            // ============================================
            builder.ApplyConfiguration(new WarehouseStockConfiguration());

            // Warehouse Configuration
            builder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("Warehouses");

                // Unique constraint: Company + Name
                entity.HasIndex(e => new { e.CompanyId, e.Name })
                      .IsUnique()
                      .HasDatabaseName("IX_Warehouse_Company_Name");

                // Convert enum to string
                entity.Property(e => e.Status)
                      .HasConversion<string>();

                // Default values
                entity.Property(e => e.IsPrimary)
                      .HasDefaultValue(false);

                entity.Property(e => e.Status)
                      .HasDefaultValue(WarehouseStatus.Active);

                // Company relationship
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_Warehouse_Status");

                entity.HasIndex(e => e.IsPrimary)
                      .HasDatabaseName("IX_Warehouse_IsPrimary");

                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_Warehouse_Company");
            });

            // Category Configuration
            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");


                // Self-referencing relationship for hierarchy
                entity.HasOne(e => e.ParentCategory)
                      .WithMany(e => e.SubCategories)
                      .HasForeignKey(e => e.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Attachments relationship
                entity.HasMany(e => e.Attachments)
                      .WithOne(e => e.Category)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(e => e.ParentCategoryId)
                      .HasDatabaseName("IX_Category_Parent");

            });

            // Category Attachment Configuration
            builder.Entity<CategoryAttachment>(entity =>
            {
                entity.ToTable("CategoryAttachments");

                // User relationship
                entity.HasOne(e => e.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.CategoryId)
                      .HasDatabaseName("IX_CategoryAttachment_Category");

                entity.HasIndex(e => e.UploadedBy)
                      .HasDatabaseName("IX_CategoryAttachment_UploadedBy");
            });

            // Brand Configuration
            builder.Entity<Brand>(entity =>
            {
                entity.ToTable("Brands");

   

    

                entity.HasIndex(e => e.Name)
                      .HasDatabaseName("IX_Brand_Name");
            });
            // UnitTemplate Configuration
            builder.Entity<UnitTemplate>(entity =>
            {
                entity.ToTable("UnitTemplates");

                // ✅ إضافة علاقة Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Convert enum to string
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(UnitTemplateStatus.Active);

                // Unit conversions relationship
                entity.HasMany(e => e.UnitConversions)
                      .WithOne(e => e.UnitTemplate)
                      .HasForeignKey(e => e.UnitTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_UnitTemplate_Status");

                // ✅ إضافة Index على CompanyId
                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_UnitTemplate_Company");
            });
            // UnitConversion Configuration
            builder.Entity<UnitConversion>(entity =>
            {
                entity.ToTable("UnitConversions");

                // Unique constraint: UnitTemplateId + UnitName
                entity.HasIndex(e => new { e.UnitTemplateId, e.UnitName })
                      .IsUnique()
                      .HasDatabaseName("IX_UnitConversion_Template_Name");

                // Unique constraint: UnitTemplateId + ShortName
                entity.HasIndex(e => new { e.UnitTemplateId, e.ShortName })
                      .IsUnique()
                      .HasDatabaseName("IX_UnitConversion_Template_ShortName");

                // Unique constraint: UnitTemplateId + Factor
                entity.HasIndex(e => new { e.UnitTemplateId, e.Factor })
                      .IsUnique()
                      .HasDatabaseName("IX_UnitConversion_Template_Factor");

                // Precision for decimal
                entity.Property(e => e.Factor)
                      .HasPrecision(18, 6);

                // Indexes
                entity.HasIndex(e => e.DisplayOrder)
                      .HasDatabaseName("IX_UnitConversion_DisplayOrder");
            });

            // BarcodeSettings Configuration
            builder.Entity<BarcodeSettings>(entity =>
            {
                entity.ToTable("BarcodeSettings");

                // ✅ إضافة علاقة Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Precision for decimals
                entity.Property(e => e.WeightUnitDivider)
                      .HasPrecision(18, 6);

                entity.Property(e => e.CurrencyDivider)
                      .HasPrecision(18, 6);

                // Default value
                entity.Property(e => e.EnableWeightEmbedded)
                      .HasDefaultValue(false);

                entity.Property(e => e.IsDefault)
                      .HasDefaultValue(true);

                // ✅ إضافة Index على CompanyId
                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_BarcodeSettings_Company");

                entity.HasIndex(e => e.BarcodeType)
                      .HasDatabaseName("IX_BarcodeSettings_Type");
            });
            // CustomField Configuration
            builder.Entity<CustomField>(entity =>
            {
                entity.ToTable("CustomFields");

 

                // Convert enums to string
                entity.Property(e => e.FieldType)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(CustomFieldStatus.Active);

                // Default values
                entity.Property(e => e.IsRequired)
                      .HasDefaultValue(false);

                entity.Property(e => e.DisplayOrder)
                      .HasDefaultValue(0);

                // Indexes
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_CustomField_Status");

                entity.HasIndex(e => e.FieldType)
                      .HasDatabaseName("IX_CustomField_Type");

                entity.HasIndex(e => e.DisplayOrder)
                      .HasDatabaseName("IX_CustomField_DisplayOrder");


                     
            });
            builder.Entity<TaxComponent>(entity =>
            {
                entity.ToTable("TaxComponents");

    

                // Convert enums to string
                entity.Property(e => e.RateType)
                      .HasConversion<string>();

                entity.Property(e => e.IncludedType)
                      .HasConversion<string>();

                entity.Property(e => e.AppliesOn)
                      .HasConversion<string>()
                      .HasDefaultValue(TaxAppliesOn.Both);

                // Decimal precision
                entity.Property(e => e.RateValue)
                      .HasPrecision(10, 4);


                // Relationships
                entity.HasMany(e => e.TaxProfileComponents)
                      .WithOne(e => e.TaxComponent)
                      .HasForeignKey(e => e.TaxComponentId)
                      .OnDelete(DeleteBehavior.Restrict);




                entity.HasIndex(e => e.AppliesOn)
                      .HasDatabaseName("IX_TaxComponent_AppliesOn");
            });

            // TaxProfile Configuration
            builder.Entity<TaxProfile>(entity =>
            {
                entity.ToTable("TaxProfiles");



                // Default values


                // Relationships
                entity.HasMany(e => e.TaxProfileComponents)
                      .WithOne(e => e.TaxProfile)
                      .HasForeignKey(e => e.TaxProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

       


            });

            // TaxProfileComponent Configuration (Many-to-Many)
            builder.Entity<TaxProfileComponent>(entity =>
            {
                entity.ToTable("TaxProfileComponents");

                // Composite Primary Key
                entity.HasKey(e => new { e.TaxProfileId, e.TaxComponentId });

                // Default value
                entity.Property(e => e.Priority)
                      .HasDefaultValue(1);
                // Multi-tenancy support
                entity.Property(e => e.IsDeleted)
                      .HasDefaultValue(false);

       

                // Indexes
                entity.HasIndex(e => e.Priority)
                      .HasDatabaseName("IX_TaxProfileComponent_Priority");
            });
            builder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                // Unique constraint: CompanyId + Name
                entity.HasIndex(e => new { e.CompanyId, e.Name })
                      .IsUnique()
                      .HasDatabaseName("IX_Supplier_Company_Name");

                // Company relationship
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);



                // Convert enum to string
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(SupplierStatus.Active);

                // Relationships
                entity.HasMany(e => e.Products)
                      .WithOne(e => e.Supplier)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Services)
                      .WithOne(e => e.Supplier)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

 

                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_Supplier_Status");

                entity.HasIndex(e => e.Email)
                      .HasDatabaseName("IX_Supplier_Email");
            });

            // ============================================
            // PRODUCTS CONFIGURATION
            // ============================================
            // Product Configuration
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");


                // Convert enums to string
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(ProductStatus.Active);

                entity.Property(e => e.DiscountType)
                      .HasConversion<string>();

                // Default values
                entity.Property(e => e.TrackStock)
                      .HasDefaultValue(true);

                // Decimal precision
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 4);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 4);
                entity.Property(e => e.MinPrice).HasPrecision(18, 4);
                entity.Property(e => e.Discount).HasPrecision(18, 4);
                entity.Property(e => e.ProfitMargin).HasPrecision(10, 4);
                entity.Property(e => e.InitialStock).HasPrecision(18, 3);
                entity.Property(e => e.LowStockThreshold).HasPrecision(18, 3);

                // Relationships
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ NEW: Warehouse Relationship
                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                      .WithMany()
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supplier)
                      .WithMany(e => e.Products)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ProductTaxProfiles)
                      .WithOne(e => e.Product)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ItemGroupItems)
                      .WithOne(e => e.Product)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.CustomFieldValues)
                      .WithOne(e => e.Product)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Performance indexes
                entity.HasIndex(e => e.CompanyId).HasDatabaseName("IX_Product_Company");
                entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_Product_Warehouse");  // ✅ NEW
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Product_Status");
                entity.HasIndex(e => e.TrackStock).HasDatabaseName("IX_Product_TrackStock");
                entity.HasIndex(e => e.LowStockThreshold).HasDatabaseName("IX_Product_LowStock");
                entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_Product_Category");
                entity.HasIndex(e => e.BrandId).HasDatabaseName("IX_Product_Brand");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_Product_Supplier");
            });

            // ============================================
            // SERVICES CONFIGURATION
            // ============================================
            builder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");

                entity.HasIndex(e => new { e.CompanyId, e.Code })
                      .IsUnique()
                      .HasFilter("[Code] IS NOT NULL")
                      .HasDatabaseName("IX_Service_Company_Code");

                // Company relationship
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ NEW: Warehouse Relationship
                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

    

                // Convert enums to string
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(ServiceStatus.Active);

                entity.Property(e => e.DiscountType)
                      .HasConversion<string>();

                // Decimal precision
                entity.Property(e => e.PurchasePrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.UnitPrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.MinPrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.Discount)
                      .HasPrecision(18, 4);

                entity.Property(e => e.ProfitMargin)
                      .HasPrecision(10, 4);

                // Relationships
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supplier)
                      .WithMany(e => e.Services)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ServiceTaxProfiles)
                      .WithOne(e => e.Service)
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.CompanyId).HasDatabaseName("IX_Service_Company");
                entity.HasIndex(e => e.WarehouseId).HasDatabaseName("IX_Service_Warehouse");  // ✅ NEW
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Service_Status");
                entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_Service_Category");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_Service_Supplier");
            });
            // ============================================
            // PRODUCT TAX PROFILES (Many-to-Many)
            // ============================================
            builder.Entity<ProductTaxProfile>(entity =>
            {
                entity.ToTable("ProductTaxProfiles");

                // Composite Primary Key
                entity.HasKey(e => new { e.ProductId, e.TaxProfileId });

                // Default value
                entity.Property(e => e.IsPrimary)
                      .HasDefaultValue(false);

                // Multi-tenancy support
                entity.Property(e => e.IsDeleted)
                      .HasDefaultValue(false);


                // Only one primary tax profile per product
                entity.HasIndex(e => new { e.ProductId, e.IsPrimary })
                      .IsUnique()
                      .HasFilter("[IsPrimary] = 1")
                      .HasDatabaseName("IX_ProductTaxProfile_Primary");

                // Relationships
                entity.HasOne(e => e.TaxProfile)
                      .WithMany()
                      .HasForeignKey(e => e.TaxProfileId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // ============================================
            // SERVICE TAX PROFILES (Many-to-Many)
            // ============================================
            builder.Entity<ServiceTaxProfile>(entity =>
            {
                entity.ToTable("ServiceTaxProfiles");

                // Composite Primary Key
                entity.HasKey(e => new { e.ServiceId, e.TaxProfileId });

                // Default value
                entity.Property(e => e.IsPrimary)
                      .HasDefaultValue(false);

                // Multi-tenancy support
                entity.Property(e => e.IsDeleted)
                      .HasDefaultValue(false);


                // Only one primary tax profile per service
                entity.HasIndex(e => new { e.ServiceId, e.IsPrimary })
                      .IsUnique()
                      .HasFilter("[IsPrimary] = 1")
                      .HasDatabaseName("IX_ServiceTaxProfile_Primary");

                // Relationships
                entity.HasOne(e => e.TaxProfile)
                      .WithMany()
                      .HasForeignKey(e => e.TaxProfileId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // ITEM GROUPS CONFIGURATION
            // ============================================
            builder.Entity<ItemGroup>(entity =>
            {
                entity.ToTable("ItemGroups");

                // Unique constraint


                // Relationships
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                      .WithMany()
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Items)
                      .WithOne(e => e.Group)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);

  

                entity.HasIndex(e => e.CategoryId)
                      .HasDatabaseName("IX_ItemGroup_Category");
            });

            // ============================================
            // ITEM GROUP ITEMS CONFIGURATION
            // ============================================
            builder.Entity<ItemGroupItem>(entity =>
            {
                entity.ToTable("ItemGroupItems");

                // Unique constraint: GroupId + ProductId
                entity.HasIndex(e => new { e.GroupId, e.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_ItemGroupItem_Group_Product");

                // Decimal precision
                entity.Property(e => e.PurchasePrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.SellingPrice)
                      .HasPrecision(18, 4);

                // Indexes
                entity.HasIndex(e => e.SKU)
                      .HasDatabaseName("IX_ItemGroupItem_SKU");

                entity.HasIndex(e => e.Barcode)
                      .HasDatabaseName("IX_ItemGroupItem_Barcode");
            });

            // ============================================
            // PRODUCT CUSTOM FIELD VALUES CONFIGURATION
            // ============================================
            builder.Entity<ProductCustomFieldValue>(entity =>
            {
                entity.ToTable("ProductCustomFieldValues");

                // Unique constraint: ProductId + FieldId
                entity.HasIndex(e => new { e.ProductId, e.FieldId })
                      .IsUnique()
                      .HasDatabaseName("IX_ProductCustomFieldValue_Product_Field");

                // Relationships
                entity.HasOne(e => e.CustomField)
                      .WithMany()
                      .HasForeignKey(e => e.FieldId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.FieldId)
                      .HasDatabaseName("IX_ProductCustomFieldValue_Field");
            });
            builder.Entity<StockTransaction>(entity =>
            {
                entity.ToTable("StockTransactions");

                // Convert enum to string
                entity.Property(e => e.TransactionType)
                      .HasConversion<string>();

                // Decimal precision
                entity.Property(e => e.Quantity)
                      .HasPrecision(18, 3);

                entity.Property(e => e.UnitCost)
                      .HasPrecision(18, 4);

                entity.Property(e => e.StockLevelAfter)
                      .HasPrecision(18, 3);

                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Performance indexes
                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_StockTransaction_Product");

                entity.HasIndex(e => e.WarehouseId)
                      .HasDatabaseName("IX_StockTransaction_Warehouse");

                entity.HasIndex(e => e.TransactionType)
                      .HasDatabaseName("IX_StockTransaction_Type");

                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("IX_StockTransaction_Date");

                entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId })
                      .HasDatabaseName("IX_StockTransaction_Reference");
            });

            // ============================================
            // PRODUCT STATS CONFIGURATION
            // ============================================
            builder.Entity<ProductStats>(entity =>
            {
                entity.ToTable("ProductStats");

                // One-to-One relationship with Product
                entity.HasKey(e => e.ProductId);

                entity.HasOne(e => e.Product)
                      .WithOne()
                      .HasForeignKey<ProductStats>(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Decimal precision
                entity.Property(e => e.TotalSold)
                      .HasPrecision(18, 3);

                entity.Property(e => e.SoldLast28Days)
                      .HasPrecision(18, 3);

                entity.Property(e => e.SoldLast7Days)
                      .HasPrecision(18, 3);

                entity.Property(e => e.OnHandStock)
                      .HasPrecision(18, 3);

                entity.Property(e => e.AvgUnitCost)
                      .HasPrecision(18, 4);

                // Default values
                entity.Property(e => e.TotalSold)
                      .HasDefaultValue(0);

                entity.Property(e => e.SoldLast28Days)
                      .HasDefaultValue(0);

                entity.Property(e => e.SoldLast7Days)
                      .HasDefaultValue(0);

                entity.Property(e => e.OnHandStock)
                      .HasDefaultValue(0);

                entity.Property(e => e.AvgUnitCost)
                      .HasDefaultValue(0);

                entity.Property(e => e.LastUpdated)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // ============================================
            // REQUISITIONS CONFIGURATION
            // ============================================
            // ProductStats Configuration
            builder.Entity<ProductStats>(entity =>
            {
                entity.ToTable("ProductStats");

                // Primary Key من BaseEntity (Id)
                entity.HasKey(e => e.Id);

                // Unique index على ProductId
                entity.HasIndex(e => e.ProductId)
                      .IsUnique()
                      .HasDatabaseName("IX_ProductStats_Product");

                // One-to-One relationship with Product
                entity.HasOne(e => e.Product)
                      .WithOne()
                      .HasForeignKey<ProductStats>(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Company relationship
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Decimal precision
                entity.Property(e => e.TotalSold).HasPrecision(18, 3).HasDefaultValue(0);
                entity.Property(e => e.SoldLast28Days).HasPrecision(18, 3).HasDefaultValue(0);
                entity.Property(e => e.SoldLast7Days).HasPrecision(18, 3).HasDefaultValue(0);
                entity.Property(e => e.OnHandStock).HasPrecision(18, 3).HasDefaultValue(0);
                entity.Property(e => e.AvgUnitCost).HasPrecision(18, 4).HasDefaultValue(0);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.CompanyId).HasDatabaseName("IX_ProductStats_Company");
            });

            // ============================================
            // REQUISITIONS CONFIGURATION
            // ============================================
            builder.Entity<Requisition>(entity =>
            {
                entity.ToTable("Requisitions");

                entity.HasMany(e => e.Attachments)
                      .WithOne(e => e.Requisition)
                      .HasForeignKey(e => e.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Convert enums to string
                entity.Property(e => e.Type)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(RequisitionStatus.Draft);

                // Relationships
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supplier)
                      .WithMany()
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentRequisition)
                      .WithMany(e => e.ChildRequisitions)
                      .HasForeignKey(e => e.ParentRequisitionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ User relationships - صححناهم
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ كان غلط: CreatedByUser مكررة، المفروض SubmittedByUser
                entity.HasOne(e => e.SubmittedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.SubmittedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ConfirmedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ConfirmedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReversedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReversedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Child collections
                entity.HasMany(e => e.Items)
                      .WithOne(e => e.Requisition)
                      .HasForeignKey(e => e.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ApprovalLogs)
                      .WithOne(e => e.Requisition)
                      .HasForeignKey(e => e.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.CompanyId)
                      .HasDatabaseName("IX_Requisition_Company");

                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_Requisition_Status");

                entity.HasIndex(e => e.Date)
                      .HasDatabaseName("IX_Requisition_Date");

                entity.HasIndex(e => e.WarehouseId)
                      .HasDatabaseName("IX_Requisition_Warehouse");

                entity.HasIndex(e => e.Type)
                      .HasDatabaseName("IX_Requisition_Type");
            });
            // ============================================
            // REQUISITION ATTACHMENT CONFIGURATION
            // ============================================
            builder.Entity<RequisitionAttachment>(entity =>
            {
                entity.ToTable("RequisitionAttachments");

                // Relationships
                entity.HasOne(e => e.Requisition)
                      .WithMany(e => e.Attachments)
                      .HasForeignKey(e => e.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Default value
                entity.Property(e => e.UploadedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.RequisitionId)
                      .HasDatabaseName("IX_RequisitionAttachment_Requisition");

                entity.HasIndex(e => e.UploadedBy)
                      .HasDatabaseName("IX_RequisitionAttachment_UploadedBy");
            });
            // ============================================
            // REQUISITION ITEMS CONFIGURATION
            // ============================================

            builder.Entity<RequisitionItem>(entity =>
            {
                entity.ToTable("RequisitionItems");

                // Decimal precision
                entity.Property(e => e.UnitPrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.Quantity)
                      .HasPrecision(18, 3);

                entity.Property(e => e.StockOnHand)
                      .HasPrecision(18, 3);

                entity.Property(e => e.NewStockOnHand)
                      .HasPrecision(18, 3);

                entity.Property(e => e.LineTotal)
                      .HasPrecision(18, 4);


                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.RequisitionId)
                      .HasDatabaseName("IX_RequisitionItem_Requisition");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_RequisitionItem_Product");
            });

            // ============================================
            // REQUISITION APPROVAL LOG CONFIGURATION
            // ============================================
            builder.Entity<RequisitionApprovalLog>(entity =>
            {
                entity.ToTable("RequisitionApprovalLogs");

                // Convert enum to string
                entity.Property(e => e.Action)
                      .HasConversion<string>();

                // Default value
                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.RequisitionId)
                      .HasDatabaseName("IX_RequisitionApprovalLog_Requisition");

                entity.HasIndex(e => e.Timestamp)
                      .HasDatabaseName("IX_RequisitionApprovalLog_Timestamp");
            });

            // ============================================
            // STOCKTAKING HEADER CONFIGURATION
            // ============================================
            builder.Entity<StocktakingHeader>(entity =>
            {
                entity.ToTable("StocktakingHeaders");



                // Convert enum to string
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(StocktakingStatus.Draft);

                // Default values
                entity.Property(e => e.UpdateSystem)
                      .HasDefaultValue(true);

                // Relationships
                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PostedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.PostedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Child collections
                entity.HasMany(e => e.Lines)
                      .WithOne(e => e.Stocktaking)
                      .HasForeignKey(e => e.StocktakingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Attachments)
                      .WithOne(e => e.Stocktaking)
                      .HasForeignKey(e => e.StocktakingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Snapshots)
                      .WithOne(e => e.Stocktaking)
                      .HasForeignKey(e => e.StocktakingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_Stocktaking_Status");

                entity.HasIndex(e => e.DateTime)
                      .HasDatabaseName("IX_Stocktaking_DateTime");

                entity.HasIndex(e => e.WarehouseId)
                      .HasDatabaseName("IX_Stocktaking_Warehouse");
            });

            // ============================================
            // STOCKTAKING LINE CONFIGURATION
            // ============================================
            builder.Entity<StocktakingLine>(entity =>
            {
                entity.ToTable("StocktakingLines");

                // Decimal precision
                entity.Property(e => e.PhysicalQty)
                      .HasPrecision(18, 3);

                entity.Property(e => e.SystemQtySnapshot)
                      .HasPrecision(18, 3);

                entity.Property(e => e.SystemQtyAtPost)
                      .HasPrecision(18, 3);

                entity.Property(e => e.VarianceQty)
                      .HasPrecision(18, 3);

                entity.Property(e => e.ValuationCost)
                      .HasPrecision(18, 4);

                entity.Property(e => e.VarianceValue)
                      .HasPrecision(18, 4);

                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.StocktakingId)
                      .HasDatabaseName("IX_StocktakingLine_Stocktaking");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_StocktakingLine_Product");

                // Unique constraint: One line per product per stocktaking
                entity.HasIndex(e => new { e.StocktakingId, e.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_StocktakingLine_Stocktaking_Product");
            });

            // ============================================
            // STOCKTAKING ATTACHMENT CONFIGURATION
            // ============================================
            builder.Entity<StocktakingAttachment>(entity =>
            {
                entity.ToTable("StocktakingAttachments");

                // Default value
                entity.Property(e => e.UploadedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.StocktakingId)
                      .HasDatabaseName("IX_StocktakingAttachment_Stocktaking");

                entity.HasIndex(e => e.UploadedBy)
                      .HasDatabaseName("IX_StocktakingAttachment_UploadedBy");
            });

            // ============================================
            // STOCK SNAPSHOT CONFIGURATION
            // ============================================
            builder.Entity<StockSnapshot>(entity =>
            {
                entity.ToTable("StockSnapshots");

                // Primary Key - استخدم Id من BaseEntity
                entity.HasKey(e => e.Id);

                // Unique constraint: One snapshot per product per stocktaking
                entity.HasIndex(e => new { e.StocktakingId, e.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_StockSnapshot_Stocktaking_Product");

                // Decimal precision
                entity.Property(e => e.QtyAtStart)
                      .HasPrecision(18, 3)
                      .IsRequired();

                // BaseEntity properties configuration
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()")
                      .IsRequired();

                entity.Property(e => e.IsDeleted)
                      .HasDefaultValue(false)
                      .IsRequired();

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true)
                      .IsRequired();

                // Relationships
                entity.HasOne(e => e.Stocktaking)
                      .WithMany() // أو .WithMany(s => s.StockSnapshots) لو عندك navigation property
                      .HasForeignKey(e => e.StocktakingId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.StocktakingId)
                      .HasDatabaseName("IX_StockSnapshot_Stocktaking");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_StockSnapshot_Product");

                entity.HasIndex(e => e.IsDeleted)
                      .HasDatabaseName("IX_StockSnapshot_IsDeleted");

                // Query filter للـ soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
            builder.Entity<PriceList>(entity =>
            {
                entity.ToTable("PriceLists");



                // Convert enums to string
                entity.Property(e => e.Type)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(PriceListStatus.Active);

                // Default values
                entity.Property(e => e.IsDefault)
                      .HasDefaultValue(false);

                // Relationship with Currency (الموجودة عندك)
                entity.HasOne(e => e.Currency)
                      .WithMany()  // Currency doesn't have PriceLists navigation
                      .HasForeignKey(e => e.CurrencyCode)
                      .OnDelete(DeleteBehavior.Restrict);

                // Child collections
                entity.HasMany(e => e.Items)
                      .WithOne(e => e.PriceList)
                      .HasForeignKey(e => e.PriceListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Rules)
                      .WithOne(e => e.PriceList)
                      .HasForeignKey(e => e.PriceListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.BulkDiscounts)
                      .WithOne(e => e.PriceList)
                      .HasForeignKey(e => e.PriceListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Assignments)
                      .WithOne(e => e.PriceList)
                      .HasForeignKey(e => e.PriceListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Type)
                      .HasDatabaseName("IX_PriceList_Type");

                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_PriceList_Status");

                entity.HasIndex(e => new { e.ValidFrom, e.ValidTo })
                      .HasDatabaseName("IX_PriceList_ValidityDates");

                entity.HasIndex(e => e.CurrencyCode)
                      .HasDatabaseName("IX_PriceList_Currency");

  
            });

            // ============================================
            // PRICE LIST ITEMS CONFIGURATION
            // ============================================
            builder.Entity<PriceListItem>(entity =>
            {
                entity.ToTable("PriceListItems");

                // Decimal precision
                entity.Property(e => e.BasePrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.ListPrice)
                      .HasPrecision(18, 4);

                entity.Property(e => e.DiscountValue)
                      .HasPrecision(18, 4);

                entity.Property(e => e.FinalPrice)
                      .HasPrecision(18, 4);

                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Service)
                      .WithMany()
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TaxProfile)
                      .WithMany()
                      .HasForeignKey(e => e.TaxProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Check constraint: Either ProductId or ServiceId must be set, not both
                entity.HasCheckConstraint(
                    "CK_PriceListItem_ProductOrService",
                    "([ProductId] IS NOT NULL AND [ServiceId] IS NULL) OR ([ProductId] IS NULL AND [ServiceId] IS NOT NULL)"
                );

                // Indexes
                entity.HasIndex(e => e.PriceListId)
                      .HasDatabaseName("IX_PriceListItem_PriceList");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_PriceListItem_Product");

                entity.HasIndex(e => e.ServiceId)
                      .HasDatabaseName("IX_PriceListItem_Service");

                entity.HasIndex(e => new { e.ValidFrom, e.ValidTo })
                      .HasDatabaseName("IX_PriceListItem_ValidityDates");

                // Unique constraint: One product/service per price list
                entity.HasIndex(e => new { e.PriceListId, e.ProductId })
                      .IsUnique()
                      .HasFilter("[ProductId] IS NOT NULL")
                      .HasDatabaseName("IX_PriceListItem_PriceList_Product");

                entity.HasIndex(e => new { e.PriceListId, e.ServiceId })
                      .IsUnique()
                      .HasFilter("[ServiceId] IS NOT NULL")
                      .HasDatabaseName("IX_PriceListItem_PriceList_Service");
            });

            // ============================================
            // PRICE LIST RULES CONFIGURATION
            // ============================================
            builder.Entity<PriceListRule>(entity =>
            {
                entity.ToTable("PriceListRules");

                // Convert enum to string
                entity.Property(e => e.RuleType)
                      .HasConversion<string>();

                // Decimal precision
                entity.Property(e => e.Value)
                      .HasPrecision(18, 4);

                // Default value
                entity.Property(e => e.Priority)
                      .HasDefaultValue(1);

                // Indexes
                entity.HasIndex(e => e.PriceListId)
                      .HasDatabaseName("IX_PriceListRule_PriceList");

                entity.HasIndex(e => e.Priority)
                      .HasDatabaseName("IX_PriceListRule_Priority");

                entity.HasIndex(e => new { e.StartDate, e.EndDate })
                      .HasDatabaseName("IX_PriceListRule_Dates");
            });

            // ============================================
            // BULK DISCOUNTS CONFIGURATION
            // ============================================
            builder.Entity<BulkDiscount>(entity =>
            {
                entity.ToTable("BulkDiscounts");

                // Convert enum to string
                entity.Property(e => e.DiscountType)
                      .HasConversion<string>();

                // Decimal precision
                entity.Property(e => e.MinQty)
                      .HasPrecision(18, 3);

                entity.Property(e => e.MaxQty)
                      .HasPrecision(18, 3);

                entity.Property(e => e.DiscountValue)
                      .HasPrecision(18, 4);

                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.PriceListId)
                      .HasDatabaseName("IX_BulkDiscount_PriceList");

                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_BulkDiscount_Product");

                entity.HasIndex(e => new { e.ProductId, e.PriceListId, e.MinQty })
                      .HasDatabaseName("IX_BulkDiscount_Product_PriceList_MinQty");

                // Check constraint: MinQty must be less than MaxQty
                entity.HasCheckConstraint(
                    "CK_BulkDiscount_QtyRange",
                    "[MaxQty] IS NULL OR [MinQty] < [MaxQty]"
                );
            });

            // ============================================
            // PRICE LIST ASSIGNMENTS CONFIGURATION
            // ============================================
            builder.Entity<PriceListAssignment>(entity =>
            {
                entity.ToTable("PriceListAssignments");

                // Convert enum to string
                entity.Property(e => e.EntityType)
                      .HasConversion<string>();

                // Unique constraint: One price list per entity
                entity.HasIndex(e => new { e.EntityType, e.EntityId, e.PriceListId })
                      .IsUnique()
                      .HasDatabaseName("IX_PriceListAssignment_Entity_PriceList");

                // Indexes
                entity.HasIndex(e => e.PriceListId)
                      .HasDatabaseName("IX_PriceListAssignment_PriceList");

                entity.HasIndex(e => new { e.EntityType, e.EntityId })
                      .HasDatabaseName("IX_PriceListAssignment_Entity");
            });

            // ============================================
            // PRICE CALCULATION LOG CONFIGURATION
            // ============================================
            builder.Entity<PriceCalculationLog>(entity =>
            {
                entity.ToTable("PriceCalculationLogs");

                // Decimal precision
                entity.Property(e => e.ValueBefore)
                      .HasPrecision(18, 4);

                entity.Property(e => e.ValueAfter)
                      .HasPrecision(18, 4);

                // Default value
                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_PriceCalcLog_Product");

                entity.HasIndex(e => e.Timestamp)
                      .HasDatabaseName("IX_PriceCalcLog_Timestamp");

                entity.HasIndex(e => new { e.TransactionType, e.TransactionId })
                      .HasDatabaseName("IX_PriceCalcLog_Transaction");
            });

            builder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("ActivityLogs");

                // Required fields
                entity.Property(e => e.ProductId)
                    .IsRequired();

                entity.Property(e => e.ActionType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.BeforeValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AfterValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // User relationship
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.ProductId)
                    .HasDatabaseName("IX_ActivityLog_Product");

                entity.HasIndex(e => e.ActionType)
                    .HasDatabaseName("IX_ActivityLog_ActionType");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_ActivityLog_User");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_ActivityLog_CreatedAt");

                entity.HasIndex(e => new { e.ProductId, e.CreatedAt })
                    .HasDatabaseName("IX_ActivityLog_Product_Date");

                entity.HasIndex(e => new { e.ActionType, e.CreatedAt })
                    .HasDatabaseName("IX_ActivityLog_ActionType_Date");
            });

            // ============================================
            // PRODUCT TIMELINE CONFIGURATION
            // ============================================
            builder.Entity<ProductTimeline>(entity =>
            {
                entity.ToTable("ProductTimelines");

                // Required fields
                entity.Property(e => e.ProductId)
                    .IsRequired();

                entity.Property(e => e.ActionType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ItemReference)
                    .HasMaxLength(200);

                entity.Property(e => e.StockBalance)
                    .HasPrecision(18, 3);

                entity.Property(e => e.AveragePrice)
                    .HasPrecision(18, 4);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                // Relationships
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // User relationship
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.ProductId)
                    .HasDatabaseName("IX_ProductTimeline_Product");

                entity.HasIndex(e => e.ActionType)
                    .HasDatabaseName("IX_ProductTimeline_ActionType");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_ProductTimeline_User");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_ProductTimeline_CreatedAt");

                entity.HasIndex(e => new { e.ProductId, e.CreatedAt })
                    .HasDatabaseName("IX_ProductTimeline_Product_Date");

                entity.HasIndex(e => new { e.ProductId, e.ActionType })
                    .HasDatabaseName("IX_ProductTimeline_Product_ActionType");
            });


            // Apply Global Query Filters for Multi-tenancy
            ApplyGlobalFilters(builder);
        }

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
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantId();
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SetTenantId();
            SetAuditFields();
            return base.SaveChanges();
        }

        private void SetTenantId()
        {
            var currentTenantId = _tenantService?.GetCurrentTenantId();
            if (!string.IsNullOrEmpty(currentTenantId) && Guid.TryParse(currentTenantId, out var tenantGuid))
            {
                var entries = ChangeTracker.Entries<ITenantEntity>()
                    .Where(e => e.State == EntityState.Added);

                foreach (var entry in entries)
                {
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = tenantGuid;
                    }
                }
            }
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedById).IsModified = false;
                }
            }
        }
    }
}