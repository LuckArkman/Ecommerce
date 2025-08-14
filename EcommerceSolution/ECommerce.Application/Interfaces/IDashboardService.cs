using System.Threading.Tasks;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        // Task<IEnumerable<ProductDto>> GetTopRatedProductsAsync(int count);
        // Task<IEnumerable<ProductDto>> GetBestSellingProductsAsync(int count);
        // Task<byte[]> GenerateSalesPdfReportAsync(); // Para exportação PDF
        // Task<byte[]> GenerateSalesExcelReportAsync(); // Para exportação Excel
        Task<byte[]> GenerateSalesPdfReportAsync();
        Task<byte[]> GenerateSalesExcelReportAsync();
    }
}