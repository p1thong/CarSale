using System;
using System.Collections.Generic;
using ASM1.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Data;

public partial class CarSalesDbContext : DbContext
{
    public CarSalesDbContext() { }

    public CarSalesDbContext(DbContextOptions<CarSalesDbContext> options)
        : base(options) { }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Dealer> Dealers { get; set; }

    public virtual DbSet<DealerContract> DealerContracts { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Quotation> Quotations { get; set; }

    public virtual DbSet<SalesContract> SalesContracts { get; set; }

    public virtual DbSet<TestDrive> TestDrives { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    public virtual DbSet<VehicleVariant> VehicleVariants { get; set; }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //         =>
    //         optionsBuilder.UseSqlServer(
    //             "Server=(local);Uid=sa;Pwd=12345;Database=CarSalesDB;TrustServerCertificate=True"
    //         );

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__B611CB7D0049E38E");

            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).ValueGeneratedOnAdd().HasColumnName("customerId");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity
                .Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullName");
            entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("phone");

            entity
                .HasOne(d => d.Dealer)
                .WithMany(p => p.Customers)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Customer__dealer__6FE99F9F");
        });

        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.HasKey(e => e.DealerId).HasName("PK__Dealer__5A9E9D961C30970D");

            entity.ToTable("Dealer");

            entity.Property(e => e.DealerId).ValueGeneratedOnAdd().HasColumnName("dealerId");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity
                .Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullName");
            entity
                .Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("phone");
            entity.Property(e => e.TransactionId).HasColumnName("transactionId");
        });

        modelBuilder.Entity<DealerContract>(entity =>
        {
            entity.HasKey(e => e.DealerContractId).HasName("PK__DealerCo__5D9704E720573FF3");

            entity.ToTable("DealerContract");

            entity
                .Property(e => e.DealerContractId)
                .ValueGeneratedOnAdd()
                .HasColumnName("dealerContractId");
            entity
                .Property(e => e.CreditLimit)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("creditLimit");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturerId");
            entity.Property(e => e.SignedDate).HasColumnName("signedDate");
            entity
                .Property(e => e.TargetSales)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("targetSales");

            entity
                .HasOne(d => d.Dealer)
                .WithMany(p => p.DealerContracts)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DealerCon__deale__6C190EBB");

            entity
                .HasOne(d => d.Manufacturer)
                .WithMany(p => p.DealerContracts)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DealerCon__manuf__6D0D32F4");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__2613FD24ACBA58F5");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn()
                .HasColumnName("feedbackId");
            entity
                .Property(e => e.Content)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("createdAt");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.FeedbackDate).HasColumnType("datetime").HasColumnName("feedbackDate");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity
                .HasOne(d => d.Customer)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__custom__02FC7413");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufacturerId).HasName("PK__Manufact__02B55389ED519028");

            entity.ToTable("Manufacturer");

            entity
                .Property(e => e.ManufacturerId)
                .ValueGeneratedOnAdd()
                .HasColumnName("manufacturerId");
            entity
                .Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("address");
            entity
                .Property(e => e.Country)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("country");
            entity.Property(e => e.Name).HasMaxLength(100).IsUnicode(false).HasColumnName("name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__0809335DA5E72A85");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).ValueGeneratedOnAdd().HasColumnName("orderId");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.OrderDate).HasColumnName("orderDate");
            entity
                .Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.VariantId).HasColumnName("variantId");

            entity
                .HasOne(d => d.Customer)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__customerI__797309D9");

            entity
                .HasOne(d => d.Dealer)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__dealerId__787EE5A0");

            entity
                .HasOne(d => d.Variant)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__variantId__7A672E12");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__A0D9EFC6E641B2C2");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).ValueGeneratedOnAdd().HasColumnName("paymentId");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)").HasColumnName("amount");
            entity
                .Property(e => e.Method)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentDate");

            entity
                .HasOne(d => d.Order)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__orderId__7D439ABD");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__99EB696E2E1B6388");

            entity.ToTable("Promotion");

            entity.Property(e => e.PromotionId).ValueGeneratedOnAdd().HasColumnName("promotionId");
            entity
                .Property(e => e.DiscountAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("discountAmount");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity
                .Property(e => e.PromotionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("promotionCode");
            entity.Property(e => e.ValidUntil).HasColumnName("validUntil");

            entity
                .HasOne(d => d.Order)
                .WithMany(p => p.Promotions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Promotion__order__00200768");
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.HasKey(e => e.QuotationId).HasName("PK__Quotatio__7536E352113ABA0A");

            entity.ToTable("Quotation");

            entity.Property(e => e.QuotationId).ValueGeneratedOnAdd().HasColumnName("quotationId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("createdAt");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)").HasColumnName("price");
            entity
                .Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.VariantId).HasColumnName("variantId");

            entity
                .HasOne(d => d.Customer)
                .WithMany(p => p.Quotations)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotation__custo__0C85DE4D");

            entity
                .HasOne(d => d.Dealer)
                .WithMany(p => p.Quotations)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotation__deale__0E6E26BF");

            entity
                .HasOne(d => d.Variant)
                .WithMany(p => p.Quotations)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quotation__varia__0D7A0286");
        });

        modelBuilder.Entity<SalesContract>(entity =>
        {
            entity.HasKey(e => e.SaleContractId).HasName("PK__SalesCon__BBA78B0BB1B2B884");

            entity.ToTable("SalesContract");

            entity
                .Property(e => e.SaleContractId)
                .ValueGeneratedOnAdd()
                .HasColumnName("saleContractId");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.SignedDate).HasColumnName("signedDate");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)").HasColumnName("totalAmount");

            entity
                .HasOne(d => d.Order)
                .WithMany(p => p.SalesContracts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesCont__order__05D8E0BE");
        });

        modelBuilder.Entity<TestDrive>(entity =>
        {
            entity.HasKey(e => e.TestDriveId).HasName("PK__TestDriv__1BEFF08411737214");

            entity.ToTable("TestDrive");

            entity.Property(e => e.TestDriveId).ValueGeneratedOnAdd().HasColumnName("testDriveId");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.ScheduledDate).HasColumnName("scheduledDate");
            entity
                .Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.VariantId).HasColumnName("variantId");

            entity
                .HasOne(d => d.Customer)
                .WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestDrive__custo__08B54D69");

            entity
                .HasOne(d => d.Variant)
                .WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestDrive__varia__09A971A2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__CB9A1CFF85902E89");

            entity.ToTable("User");

            entity.Property(e => e.UserId).ValueGeneratedOnAdd().HasColumnName("userId");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity
                .Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullName");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturerId");
            entity
                .Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false).HasColumnName("phone");
            entity.Property(e => e.Role).HasMaxLength(50).IsUnicode(false).HasColumnName("role");

            entity
                .HasOne(d => d.Dealer)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.DealerId)
                .HasConstraintName("FK__User__dealerId__68487DD7");

            entity
                .HasOne(d => d.Manufacturer)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK__User__manufactur__693CA210");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.VehicleModelId).HasName("PK__VehicleM__DF4B1849AF5DCC0A");

            entity.ToTable("VehicleModel");

            entity
                .Property(e => e.VehicleModelId)
                .ValueGeneratedOnAdd()
                .HasColumnName("vehicleModelId");
            entity
                .Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturerId");
            entity.Property(e => e.Name).HasMaxLength(100).IsUnicode(false).HasColumnName("name");
            entity
                .Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("imageUrl");

            entity
                .HasOne(d => d.Manufacturer)
                .WithMany(p => p.VehicleModels)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VehicleMo__manuf__72C60C4A");
        });

        modelBuilder.Entity<VehicleVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__VehicleV__69E44B2D7D537419");

            entity.ToTable("VehicleVariant");

            entity.Property(e => e.VariantId).ValueGeneratedOnAdd().HasColumnName("variantId");
            entity.Property(e => e.Color).HasMaxLength(50).IsUnicode(false).HasColumnName("color");
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)").HasColumnName("price");
            entity.Property(e => e.ProductYear).HasColumnName("productYear");
            entity.Property(e => e.VehicleModelId).HasColumnName("vehicleModelId");
            entity
                .Property(e => e.Version)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("version");
            entity.Property(e => e.Quantity).HasColumnName("Quantity");

            entity
                .HasOne(d => d.VehicleModel)
                .WithMany(p => p.VehicleVariants)
                .HasForeignKey(d => d.VehicleModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VehicleVa__vehic__75A278F5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
