using ECommerce.Models.DTOs.User;

namespace ECommerce.Client.Services
{
    public class UserProfileApiClient
    {
        private readonly HttpClient _httpClient;

        public UserProfileApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserProfileDto> GetUserProfile()
        {
            return await _httpClient.GetFromJsonAsync<UserProfileDto>("api/userprofile");
        }
    }
}