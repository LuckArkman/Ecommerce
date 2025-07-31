using ECommerce.Models.DTOs.Product;
using ECommerce.Models.DTOs.Review;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var sales = await GetSalesMetricsAsync();
        var stock = await GetStockMetricsAsync();
        var deliveries = await GetDeliveryMetricsAsync();
        var customerSatisfaction = await GetCustomerSatisfactionMetricsAsync();

        return new DashboardSummaryDto
        {
            Sales = sales,
            Stock = stock,
            Deliveries = deliveries,
            CustomerSatisfaction = customerSatisfaction
        };
    }

    private async Task<SalesMetricDto> GetSalesMetricsAsync()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalSales = await _context.Orders.SumAsync(o => o.TotalAmount);
        var salesByMonth = await _context.Orders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new {
                Month = g.Key.Month,
                Year = g.Key.Year,
                Total = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToDictionaryAsync(
                x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), // Ex: "Jan 2023"
                x => x.Total
            );

        return new SalesMetricDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0 ? (int)(totalSales / totalOrders) : 0,
            SalesByMonth = salesByMonth
        };
    }

    private async Task<StockMetricDto> GetStockMetricsAsync()
    {
        var totalProducts = await _context.Products.CountAsync();
        var outOfStockProducts = await _context.Products.CountAsync(p => p.Stock <= 0);
        var lowStockProducts = await _context.Products
            .Where(p => p.Stock > 0 && p.Stock < 10) // Defina seu limite de estoque baixo
            .Select(p => new ProductDto // Mapeie para DTO
            {
                Id = p.Id,
                Name = p.Name,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
                // Outras propriedades se necessário
            })
            .ToListAsync();

        return new StockMetricDto
        {
            TotalProducts = totalProducts,
            OutOfStockProducts = outOfStockProducts,
            LowStockProducts = lowStockProducts
        };
    }

    private async Task<DeliveryMetricDto> GetDeliveryMetricsAsync()
    {
        var orders = await _context.Orders.ToListAsync();
        return new DeliveryMetricDto
        {
            PendingDeliveries = orders.Count(o => o.Status == "Pending" || o.Status == "Processing"),
            ShippedDeliveries = orders.Count(o => o.Status == "Shipped"),
            DeliveredDeliveries = orders.Count(o => o.Status == "Delivered"),
            CancelledDeliveries = orders.Count(o => o.Status == "Cancelled")
        };
    }

    private async Task<CustomerSatisfactionMetricDto> GetCustomerSatisfactionMetricsAsync()
    {
        var reviews = await _context.Reviews.ToListAsync();
        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0 ? reviews.Average(r => r.Rating) : 0;

        var recentReviews = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5) // Últimas 5 reviews
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserName = r.User.UserName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new CustomerSatisfactionMetricDto
        {
            AverageRating = Math.Round(averageRating, 2),
            TotalReviews = totalReviews,
            PositiveReviews = reviews.Count(r => r.Rating >= 4),
            NegativeReviews = reviews.Count(r => r.Rating <= 2),
            NeutralReviews = reviews.Count(r => r.Rating == 3),
            RecentReviews = recentReviews
        };
    }
}