using ECommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Data
{
 
    // All schema constraints (lengths, column types, relationships) are configured
    // here via Fluent API 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User>      Users      { get; set; }
        public DbSet<Product>   Products   { get; set; }
        public DbSet<Order>     Orders     { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  User 
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("Customer");
            });

            //  Product 
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
                entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
                entity.Property(p => p.Stock).IsRequired();
            });

            //  Order 
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.OrderDate).IsRequired();
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();

                // One User - Many Orders 
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId);
            });

            //  OrderItem 
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.Quantity).IsRequired();
                entity.Property(oi => oi.Price).HasColumnType("decimal(18,2)").IsRequired();

                // One Order - Many OrderItems 
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One Product - Many OrderItems (restrict delete — preserve order history)
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
