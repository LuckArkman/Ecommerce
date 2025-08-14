using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// For DateTime

namespace ECommerce.Infrastructure.Data
{
    public static class InfrastructureDbContextInitializer
    {
        public static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Criar Role "Admin" se não existir
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Criar usuário Admin se não existir
            var adminUser = await userManager.FindByEmailAsync("admin@ecommerce.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@ecommerce.com",
                    Email = "admin@ecommerce.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    Console.WriteLine("Erro ao criar usuário admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // ***** CHAMADA AO MÉTODO SEEDINITIALDATA AQUI *****
            await SeedInitialData(context, userManager);
        }

        public static async Task SeedInitialData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // ***** DECLARAR VARIÁVEIS AQUI FORA DOS IFs *****
            ApplicationUser? testUser = null;
            Product? productLaptop = null;
            Product? productSofa = null;
            Product? productBook = null;

            // Adicionar Categorias
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics", Description = "Electronic devices" },
                    new Category { Name = "Books", Description = "Various books" },
                    new Category { Name = "Furniture", Description = "Home furniture" }
                );
                await context.SaveChangesAsync();
            }

            // Adicionar Usuário de Teste PRIMEIRO, antes de Produtos/Pedidos/Reviews
            testUser = await userManager.FindByEmailAsync("testuser@example.com");
            if (testUser == null)
            {
                testUser = new ApplicationUser { UserName = "testuser@example.com", Email = "testuser@example.com", EmailConfirmed = true, FirstName = "Test", LastName = "User" };
                var userCreationResult = await userManager.CreateAsync(testUser, "TestUser123!");
                if (!userCreationResult.Succeeded)
                {
                    Console.WriteLine("Erro ao criar usuário de teste: " + string.Join(", ", userCreationResult.Errors.Select(e => e.Description)));
                    return; // Retorna se não conseguir criar o usuário principal de teste
                }
            }


            // Adicionar Produtos
            if (!context.Products.Any())
            {
                var catElectronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
                var catFurniture = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Furniture");
                var catBooks = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Books");

                context.Products.AddRange(
                    new Product { Name = "Laptop X1", Description = "Powerful laptop", Price = 1200.00m, Stock = 10, ImageUrl = "/images/product_images/laptop.jpg", CategoryId = catElectronics?.Id ?? 1, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
                    new Product { Name = "Smartphone Pro", Description = "Latest model", Price = 800.00m, Stock = 2, ImageUrl = "/images/product_images/smartphone.jpg", CategoryId = catElectronics?.Id ?? 1, CreatedAt = DateTime.UtcNow.AddMonths(-3) },
                    new Product { Name = "Comfortable Sofa", Description = "Luxury living room sofa", Price = 1500.00m, Stock = 1, ImageUrl = "/images/product_images/sofa.jpg", CategoryId = catFurniture?.Id ?? 3, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                    new Product { Name = "Coffee Table", Description = "Modern coffee table", Price = 300.00m, Stock = 0, ImageUrl = "/images/product_images/table.jpg", CategoryId = catFurniture?.Id ?? 3, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
                    new Product { Name = "Book 'The Future'", Description = "Sci-fi novel", Price = 25.00m, Stock = 100, ImageUrl = "/images/product_images/book.jpg", CategoryId = catBooks?.Id ?? 2, CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // ***** RECUPERAR OS PRODUTOS APÓS O ADD E SALVAR *****
            // (Para garantir que IDs estão preenchidos para OrderItems/Reviews)
            productLaptop = await context.Products.FirstOrDefaultAsync(p => p.Name == "Laptop X1");
            productSofa = await context.Products.FirstOrDefaultAsync(p => p.Name == "Comfortable Sofa");
            productBook = await context.Products.FirstOrDefaultAsync(p => p.Name == "Book 'The Future'");


            // Adicionar Pedidos
            if (!context.Orders.Any())
            {
                if (productLaptop != null && productSofa != null && productBook != null && testUser != null)
                {
                    var order1 = new Order
                    {
                        UserId = testUser.Id,
                        OrderDate = DateTime.UtcNow.AddMonths(-4), TotalAmount = productLaptop.Price,
                        ShippingAddress = "123 Main St", Status = "Delivered", TrackingNumber = "TRK123"
                    };
                    order1.OrderItems = new List<OrderItem> { new OrderItem { ProductId = productLaptop.Id, Quantity = 1, Price = productLaptop.Price } };
                    context.Orders.Add(order1);

                    var order2 = new Order
                    {
                        UserId = testUser.Id,
                        OrderDate = DateTime.UtcNow.AddMonths(-2).AddDays(10), TotalAmount = productSofa.Price,
                        ShippingAddress = "456 Oak Ave", Status = "Shipped", TrackingNumber = "TRK456"
                    };
                    order2.OrderItems = new List<OrderItem> { new OrderItem { ProductId = productSofa.Id, Quantity = 1, Price = productSofa.Price } };
                    context.Orders.Add(order2);

                    var order3 = new Order
                    {
                        UserId = testUser.Id,
                        OrderDate = DateTime.UtcNow.AddMonths(-1).AddDays(5), TotalAmount = productBook.Price * 2,
                        ShippingAddress = "789 Pine Ln", Status = "Pending", TrackingNumber = null
                    };
                    order3.OrderItems = new List<OrderItem> { new OrderItem { ProductId = productBook.Id, Quantity = 2, Price = productBook.Price } };
                    context.Orders.Add(order3);

                    // Pedidos para o gráfico de vendas mensais
                    context.Orders.AddRange(
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-6), TotalAmount = 100m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-5), TotalAmount = 150m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-4), TotalAmount = 200m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-3), TotalAmount = 250m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-2), TotalAmount = 300m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow.AddMonths(-1), TotalAmount = 350m, ShippingAddress = "Addr", Status = "Delivered" },
                        new Order { UserId = testUser.Id, OrderDate = DateTime.UtcNow, TotalAmount = 400m, ShippingAddress = "Addr", Status = "Delivered" }
                    );

                    await context.SaveChangesAsync();
                }
            }

            // Adicionar Avaliações
            if (!context.Reviews.Any())
            {
                if (productBook != null && testUser != null)
                {
                    context.Reviews.AddRange(
                        new Review { ProductId = productBook.Id, UserId = testUser.Id, Rating = 5, Comment = "Excelente produto!", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                        new Review { ProductId = productBook.Id, UserId = testUser.Id, Rating = 4, Comment = "Muito bom, atendeu às expectativas.", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                        new Review { ProductId = productLaptop.Id, UserId = testUser.Id, Rating = 2, Comment = "Demorou a entregar e veio com um arranhão.", CreatedAt = DateTime.UtcNow.AddDays(-2) }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}