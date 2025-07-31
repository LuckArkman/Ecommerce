using System.Threading.Tasks;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        // Task<SalesMetricDto> GetSalesMetricsAsync(); etc.
    }
}