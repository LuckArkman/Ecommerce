using ECommerce.Models.DTOs.User;

namespace ECommerce.WebApp.Models;

public class UserProfileViewModel
{
    public UserProfileDto UserProfile { get; set; } = new();
    public UpdateUserProfileRequest UpdateRequest { get; set; } = new();
    public bool IsEditMode { get; set; }
}