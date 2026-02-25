using System;
using System.Collections.Generic;
using BE.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookingItems> BookingItems { get; set; }

    public virtual DbSet<Bookings> Bookings { get; set; }

    public virtual DbSet<CartItems> CartItems { get; set; }

    public virtual DbSet<Carts> Carts { get; set; }

    public virtual DbSet<Categories> Categories { get; set; }

    public virtual DbSet<FeedbackResponses> FeedbackResponses { get; set; }

    public virtual DbSet<Payments> Payments { get; set; }

    public virtual DbSet<ProductImages> ProductImages { get; set; }

    public virtual DbSet<ProductRatingSummary> ProductRatingSummary { get; set; }

    public virtual DbSet<ProductReviews> ProductReviews { get; set; }

    public virtual DbSet<ProductVariants> ProductVariants { get; set; }

    public virtual DbSet<Products> Products { get; set; }

    public virtual DbSet<ProviderBranches> ProviderBranches { get; set; }

    public virtual DbSet<ProviderEarnings> ProviderEarnings { get; set; }

    public virtual DbSet<ProviderFeedbacks> ProviderFeedbacks { get; set; }

    public virtual DbSet<ProviderReports> ProviderReports { get; set; }

    public virtual DbSet<ProviderSubscriptions> ProviderSubscriptions { get; set; }

    public virtual DbSet<Providers> Providers { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<SubscriptionPlans> SubscriptionPlans { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public DbSet<MediaFiles> MediaFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingItems>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__booking___3213E83FCEB19883");

            entity.ToTable("booking_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CommissionAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("commission_amount");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductVariantId).HasColumnName("product_variant_id");
            entity.Property(e => e.ProviderBranchId).HasColumnName("provider_branch_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingItems)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__booking_i__booki__02084FDA");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.BookingItems)
                .HasForeignKey(d => d.ProductVariantId)
                .HasConstraintName("FK__booking_i__produ__02FC7413");

            entity.HasOne(d => d.ProviderBranch).WithMany(p => p.BookingItems)
                .HasForeignKey(d => d.ProviderBranchId)
                .HasConstraintName("FK__booking_i__provi__03F0984C");
        });

        modelBuilder.Entity<Bookings>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookings__3213E83FEF6DB01B");

            entity.ToTable("bookings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_price");

            // Delivery info columns (added via ALTER TABLE)
            entity.Property(e => e.RecipientName)
                .HasMaxLength(255)
                .HasColumnName("RecipientName");
            entity.Property(e => e.RecipientPhone)
                .HasMaxLength(20)
                .HasColumnName("RecipientPhone");
            entity.Property(e => e.RecipientAddress)
                .HasMaxLength(500)
                .HasColumnName("RecipientAddress");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__bookings__custom__04E4BC85");
        });

        modelBuilder.Entity<CartItems>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cart_ite__3213E83F94BDA66F");

            entity.ToTable("cart_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ProductVariantId).HasColumnName("product_variant_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__cart_item__cart___05D8E0BE");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductVariantId)
                .HasConstraintName("FK__cart_item__produ__06CD04F7");
        });

        modelBuilder.Entity<Carts>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__carts__3213E83F631D80E2");

            entity.ToTable("carts");

            entity.HasIndex(e => e.CustomerId, "UQ__carts__CD65CB8439BF51C5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");

            entity.HasOne(d => d.Customer).WithOne(p => p.Carts)
                .HasForeignKey<Carts>(d => d.CustomerId)
                .HasConstraintName("FK__carts__customer___07C12930");
        });

        modelBuilder.Entity<Categories>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83FA26A8655");

            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.Property(e => e.ProviderId)
                  .HasColumnName("provider_id");


            entity.HasOne(e => e.Provider)
                  .WithMany()
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_categories_provider");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__categorie__paren__08B54D69");
        });

        modelBuilder.Entity<FeedbackResponses>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__feedback__3213E83F143F8CF3");

            entity.ToTable("feedback_responses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionTaken)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("action_taken");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.ResponseContent)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("response_content");

            entity.HasOne(d => d.Admin).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_fr_admin");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.FeedbackId)
                .HasConstraintName("FK_fr_feedback");
        });

        modelBuilder.Entity<Payments>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83FEB57F3F3");

            entity.ToTable("payments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__payments__bookin__0B91BA14");
        });

        modelBuilder.Entity<ProductImages>(entity =>
{
    entity.HasKey(e => e.Id).HasName("PK__product___3213E83F68A92C8A");

    entity.ToTable("product_images");

    entity.Property(e => e.Id).HasColumnName("id");

    // CHANGED: image_url -> image_file_id
    entity.Property(e => e.ImageFileId)
        .HasColumnName("image_file_id");

    entity.Property(e => e.ProductId).HasColumnName("product_id");

    entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
        .HasForeignKey(d => d.ProductId)
        .HasConstraintName("FK__product_i__produ__0C85DE4D");

    // NEW: FK sang media_files
    entity.HasOne(d => d.ImageFile)
        .WithMany()
        .HasForeignKey(d => d.ImageFileId)
        .HasConstraintName("fk_product_images_file");
});


        modelBuilder.Entity<ProductRatingSummary>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__product___47027DF552127FDE");

            entity.ToTable("product_rating_summary");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.AverageRating)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("average_rating");
            entity.Property(e => e.TotalReviews).HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductRatingSummary)
                .HasForeignKey<ProductRatingSummary>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_prs_product");
        });

        modelBuilder.Entity<ProductReviews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__product___3213E83F2BD88A6B");

            entity.ToTable("product_reviews");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingItemId).HasColumnName("booking_item_id");
            entity.Property(e => e.Comment)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.BookingItem).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.BookingItemId)
                .HasConstraintName("FK_pr_booking_item");

            entity.HasOne(d => d.Customer).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_pr_customer");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_pr_product");
        });

        modelBuilder.Entity<ProductVariants>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__product___3213E83F8CDE9303");

            entity.ToTable("product_variants");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColorCode)
                .HasMaxLength(10)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("color_code");
            entity.Property(e => e.ColorName)
                .HasMaxLength(50)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("color_name");
            entity.Property(e => e.DepositAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("deposit_amount");
            entity.Property(e => e.PricePerDay)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price_per_day");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SizeLabel)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("size_label");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__product_v__produ__114A936A");
        });

        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3213E83F6D170459");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("name");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("product_type");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__products__catego__123EB7A3");

            entity.HasOne(d => d.Provider).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__products__provid__1332DBDC");
        });

        modelBuilder.Entity<ProviderBranches>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83FEFA518FC");

            entity.ToTable("provider_branches");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("address");
            entity.Property(e => e.BranchName)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("branch_name");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("email");
            entity.Property(e => e.IsMainBranch).HasColumnName("is_main_branch");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("phone");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderBranches)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__provider___provi__14270015");
        });

        modelBuilder.Entity<ProviderEarnings>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83F11F44D44");

            entity.ToTable("provider_earnings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingItemId).HasColumnName("booking_item_id");
            entity.Property(e => e.Commission)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("commission");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GrossAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("gross_amount");
            entity.Property(e => e.NetAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("net_amount");
            entity.Property(e => e.ProviderBranchId).HasColumnName("provider_branch_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");

            entity.HasOne(d => d.BookingItem).WithMany(p => p.ProviderEarnings)
                .HasForeignKey(d => d.BookingItemId)
                .HasConstraintName("FK_pe_booking_item");

            entity.HasOne(d => d.ProviderBranch).WithMany(p => p.ProviderEarnings)
                .HasForeignKey(d => d.ProviderBranchId)
                .HasConstraintName("FK_pe_branch");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderEarnings)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK_pe_provider");
        });

        modelBuilder.Entity<ProviderFeedbacks>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83F1E8C2247");

            entity.ToTable("provider_feedbacks");

            entity.Property(e => e.Id).HasColumnName("id");
            // CHANGED: attachment_url -> attachment_file_id
            entity.Property(e => e.AttachmentFileId)
                .HasColumnName("attachment_file_id");

            // NEW: FK sang media_files
            entity.HasOne(d => d.AttachmentFile)
                .WithMany()
                .HasForeignKey(d => d.AttachmentFileId)
                .HasConstraintName("fk_provider_feedbacks_attachment_file");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Content)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.FeedbackType)
                .HasMaxLength(50)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("feedback_type");
            entity.Property(e => e.ProviderBranchId).HasColumnName("provider_branch_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.ProviderFeedbacks)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_pf_booking");

            entity.HasOne(d => d.Customer).WithMany(p => p.ProviderFeedbacks)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_pf_customer");

            entity.HasOne(d => d.ProviderBranch).WithMany(p => p.ProviderFeedbacks)
                .HasForeignKey(d => d.ProviderBranchId)
                .HasConstraintName("FK_pf_branch");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderFeedbacks)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK_pf_provider");
        });

        modelBuilder.Entity<ProviderReports>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83F433D4AAB");

            entity.ToTable("provider_reports");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Month)
                .HasMaxLength(7)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("month");
            entity.Property(e => e.ProviderBranchId).HasColumnName("provider_branch_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.RepeatCustomers).HasColumnName("repeat_customers");
            entity.Property(e => e.TotalBookings).HasColumnName("total_bookings");
            entity.Property(e => e.TotalRevenue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_revenue");

            entity.HasOne(d => d.ProviderBranch).WithMany(p => p.ProviderReports)
                .HasForeignKey(d => d.ProviderBranchId)
                .HasConstraintName("FK_pr_branch");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderReports)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK_pr_provider");
        });

        modelBuilder.Entity<ProviderSubscriptions>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83FD1803EB5");

            entity.ToTable("provider_subscriptions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");

            entity.HasOne(d => d.Plan).WithMany(p => p.ProviderSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__provider___plan___1DB06A4F");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderSubscriptions)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__provider___provi__1EA48E88");
        });

        modelBuilder.Entity<Providers>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__provider__3213E83FD98F40EA");

            entity.ToTable("providers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrandName)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("brand_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("description");
            // CHANGED: logo_url -> logo_file_id
            entity.Property(e => e.LogoFileId)
                .HasColumnName("logo_file_id");

            // NEW: FK sang media_files
            entity.HasOne(d => d.LogoFile)
                .WithMany()
                .HasForeignKey(d => d.LogoFileId)
                .HasConstraintName("fk_providers_logo_file");

            entity.Property(e => e.ProviderType)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("provider_type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Verified)
                .HasDefaultValue(false)
                .HasColumnName("verified");
            entity.Property(e => e.LastNotificationSeenAt)
      .HasColumnName("last_notification_seen_at");

            entity.HasOne(d => d.User).WithMany(p => p.Providers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__providers__user___1F98B2C1");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83FE560DCDF");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B197BD1B86").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<SubscriptionPlans>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83F3E319DDC");

            entity.ToTable("subscription_plans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommissionRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("commission_rate");
            entity.Property(e => e.Description)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("description");
            entity.Property(e => e.MaxProducts).HasColumnName("max_products");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("name");
            entity.Property(e => e.PriceMonthly)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price_monthly");
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F1F4F60F0");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E61647FE709C5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            // CHANGED: avatar_url -> avatar_file_id
            entity.Property(e => e.AvatarFileId)
                .HasColumnName("avatar_file_id");

            // NEW: FK sang media_files
            entity.HasOne(d => d.AvatarFile)
                .WithMany()
                .HasForeignKey(d => d.AvatarFileId)
                .HasConstraintName("fk_users_avatar_file");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .UseCollation("Vietnamese_100_CI_AI")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasMany(d => d.Role).WithMany(p => p.User)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    r => r.HasOne<Roles>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__role___208CD6FA"),
                    l => l.HasOne<Users>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__user___2180FB33"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__user_rol__6EDEA1538E4027A3");
                        j.ToTable("user_roles", tb => tb.HasTrigger("trg_create_cart_after_register"));
                        j.IndexerProperty<long>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });
        modelBuilder.Entity<MediaFiles>(entity =>
{
    entity.HasKey(e => e.Id).HasName("PK__media_files__id");

    entity.ToTable("media_files");

    entity.Property(e => e.Id).HasColumnName("id");

    entity.Property(e => e.FileName)
        .HasMaxLength(255)
        .UseCollation("Vietnamese_100_CI_AI")
        .HasColumnName("file_name");

    entity.Property(e => e.MimeType)
        .HasMaxLength(100)
        .UseCollation("Vietnamese_100_CI_AI")
        .HasColumnName("mime_type");

    entity.Property(e => e.FileSize)
        .HasColumnName("file_size");

    entity.Property(e => e.Data)
        .HasColumnName("data");

    entity.Property(e => e.CreatedAt)
        .HasColumnName("created_at");
});


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
