using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Data
{
    public static class ApplicationDbContextInitializer
    {
        public static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateSope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };
                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!"); // ATENÇÃO: Mude esta senha!

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    Console.WriteLine("Erro ao criar usuário admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}