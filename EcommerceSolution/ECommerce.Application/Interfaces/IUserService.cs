using ECommerce.Models.DTOs.User;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        // Task UpdateUserProfileAsync(string userId, UserProfileDto userProfileDto);
    }
}