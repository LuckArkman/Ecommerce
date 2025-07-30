using ECommerce.Models.DTOs.User;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);
    }
}