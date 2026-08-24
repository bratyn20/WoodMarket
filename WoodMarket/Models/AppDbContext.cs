using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WoodMarket.Models
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<Specification> Specifications { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountUsage> DiscountUsages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<CustomOrder> CustomOrders { get; set; }
        public DbSet<CustomOrderAttachment> CustomOrderAttachments { get; set; }
        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальный индекс для корзины (один пользователь - один товар с одним вариантом)
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(c => new { c.UserId, c.ProductId, c.VariantName })
                      .IsUnique()
                      .HasDatabaseName("IX_CartItem_User_Product_Variant");

                // Если вариант не указан, считаем его пустой строкой
                entity.Property(c => c.VariantName)
                      .HasDefaultValue(string.Empty);

                entity.Property(c => c.AddedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).UseIdentityColumn();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            //User - IdentityRole
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "USER" }
            );

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("IX_Product_Sku");
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.OldPrice).HasPrecision(18, 2);
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);
                entity.Property(p => p.AverageRating).HasDefaultValue(0.0);
                entity.Property(p => p.ReviewCount).HasDefaultValue(0);
            });

            // Category - иерархия
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.ChildCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.Property(o => o.Subtotal).HasPrecision(18, 2);
                entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
                entity.Property(o => o.DiscountAmount).HasPrecision(18, 2);
                entity.Property(o => o.Total).HasPrecision(18, 2);
                entity.Property(o => o.OrderDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(o => o.UnitPrice).HasPrecision(18, 2);
                entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
            });

            // Discount
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.HasIndex(d => d.Code).IsUnique();
                entity.Property(d => d.Amount).HasPrecision(18, 2);
                entity.Property(d => d.MinOrderAmount).HasPrecision(18, 2);
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // WishlistItem
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
                entity.Property(w => w.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // BlogPost
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasIndex(b => b.Slug).IsUnique();
                entity.Property(b => b.PublishedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // CustomOrder
            modelBuilder.Entity<CustomOrder>(entity =>
            {
                entity.Property(c => c.EstimatedPrice).HasPrecision(18, 2);
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(co => co.User)
                    .WithMany(u => u.CustomOrders) // ✅ Указываем первую коллекцию
                    .HasForeignKey(co => co.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь 2: User -> AssignedCustomOrders (менеджер)
                entity.HasOne(co => co.AssignedTo)
                    .WithMany(u => u.AssignedCustomOrders) // ✅ Указываем вторую коллекцию
                    .HasForeignKey(co => co.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NewsletterSubscription
            modelBuilder.Entity<NewsletterSubscription>(entity =>
            {
                entity.HasIndex(n => n.Email).IsUnique();
                entity.Property(n => n.SubscribedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.Property(p => p.AdditionalPrice).HasPrecision(18, 2);
            });

        }

    }
}
