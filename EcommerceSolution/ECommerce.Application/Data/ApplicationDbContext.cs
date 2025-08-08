using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities; // Referência a ECommerce.Domain

namespace ECommerce.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed initial data for categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
                new Category { Id = 2, Name = "Books", Description = "Various books" },
                new Category { Id = 3, Name = "Clothing", Description = "Apparel and accessories" }
            );

            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1, // Valor negativo para evitar colisão
                    Name = "Laptop X1",
                    Description = "Powerful laptop",
                    Price = 1200.00m,
                    Stock = 10,
                    ImageUrl = "/images/product_images/laptop.jpg",
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },
                new Product
                {
                    Id = 2, // Valor negativo para evitar colisão
                    Name = "Smartphone Pro",
                    Description = "Latest model",
                    Price = 800.00m,
                    Stock = 2,
                    ImageUrl = "/images/product_images/smartphone.jpg",
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new Product
                {
                    Id = 3, // Valor negativo para evitar colisão
                    Name = "Comfortable Sofa",
                    Description = "Luxury living room sofa",
                    Price = 1500.00m,
                    Stock = 1,
                    ImageUrl = "/images/product_images/sofa.jpg",
                    CategoryId = 3,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                },
                new Product
                {
                    Id = 4, // Valor negativo para evitar colisão
                    Name = "Coffee Table",
                    Description = "Modern coffee table",
                    Price = 300.00m,
                    Stock = 0,
                    ImageUrl = "/images/product_images/table.jpg",
                    CategoryId = 3,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                },
                new Product
                {
                    Id = 5, // Valor negativo para evitar colisão
                    Name = "Book 'The Future'",
                    Description = "Sci-fi novel",
                    Price = 25.00m,
                    Stock = 100,
                    ImageUrl = "/images/product_images/book.jpg",
                    CategoryId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            );

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            // Exemplo de índice único para nome de categoria (opcional)
            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}