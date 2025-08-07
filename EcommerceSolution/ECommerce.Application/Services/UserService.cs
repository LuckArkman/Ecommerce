using ECommerce.Models.DTOs.User;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using ApplicationUser = ECommerce.Domain.Entities.ApplicationUser;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
            // Mapear outras propriedades se existirem
        };
    }
}