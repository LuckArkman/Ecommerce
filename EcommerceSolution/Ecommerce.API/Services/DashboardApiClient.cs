using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Client.Services
{
    public class DashboardApiClient
    {
        private readonly HttpClient _httpClient;

        public DashboardApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummary()
        {
            return await _httpClient.GetFromJsonAsync<DashboardSummaryDto>("api/dashboard/summary");
        }
    }
}