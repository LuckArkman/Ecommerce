using ECommerce.Models.DTOs.Dashboard;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}