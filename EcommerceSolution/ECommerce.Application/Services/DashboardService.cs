using ECommerce.Models.DTOs.Product; 
using ECommerce.Models.DTOs.Review;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var sales = await GetSalesMetricsAsync();
        var stock = await GetStockMetricsAsync();
        var deliveries = await GetDeliveryMetricsAsync();
        var customerSatisfaction = await GetCustomerSatisfactionMetricsAsync();
        var topRatedProducts = await GetTopRatedProductsAsync(5);
        var bestSellingProducts = await GetBestSellingProductsAsync(5);
        return new DashboardSummaryDto
        {
            Sales = sales,
            Stock = stock,
            Deliveries = deliveries,
            CustomerSatisfaction = customerSatisfaction,
            TopRatedProducts = topRatedProducts,
            BestSellingProducts = bestSellingProducts
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
            .Where(p => p.Stock > 0 && p.Stock < 10)
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
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
            .Take(5)
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
    private async Task<List<ProductDto>> GetTopRatedProductsAsync(int count)
    {
        return await _context.Reviews
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(count)
            .Join(_context.Products,
                reviewStats => reviewStats.ProductId,
                product => product.Id,
                (reviewStats, product) => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                })
            .ToListAsync();
    }
    private async Task<List<ProductDto>> GetBestSellingProductsAsync(int count)
    {
        return await _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantitySold = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(count)
            .Join(_context.Products,
                salesStats => salesStats.ProductId,
                product => product.Id,
                (salesStats, product) => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                })
            .ToListAsync();
    }
    public async Task<byte[]> GenerateSalesPdfReportAsync()
    {
        var salesData = await GetSalesMetricsAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));
                
                page.Header()
                    .PaddingBottom(10)
                    .Text("Relatório de Vendas do E-Commerce")
                    .SemiBold().FontSize(18).AlignCenter();


                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Text($"Relatório Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(10).AlignRight();
                        column.Item().Column(subColumn =>
                        {
                            subColumn.Spacing(5);
                            subColumn.Item().Text("Resumo Geral").SemiBold().FontSize(14);
                            subColumn.Item().Text($"Total de Vendas: {salesData.TotalSales:C}");
                            subColumn.Item().Text($"Total de Pedidos: {salesData.TotalOrders}");
                            subColumn.Item().Text($"Valor Médio por Pedido: {salesData.AverageOrderValue:C}");
                        });
                        
                        column.Item().Column(subColumn =>
                        {
                            subColumn.Spacing(5);
                            subColumn.Item().Text("Vendas por Mês").SemiBold().FontSize(14);
                            subColumn.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Mês").SemiBold();
                                    header.Cell().BorderBottom(1).Padding(5).Text("Total de Vendas").SemiBold();
                                });

                                foreach (var entry in salesData.SalesByMonth)
                                {
                                    table.Cell().BorderBottom(0.5f).Padding(5).Text(entry.Key);
                                    table.Cell().BorderBottom(0.5f).Padding(5).Text(entry.Value.ToString("C"));
                                }
                            });
                        });
                    });
                
                page.Footer()
                    .AlignRight()
                    .Text(x => 
                    {
                        x.Span("Página ").FontSize(10);
                        x.CurrentPageNumber().FontSize(10);
                        x.Span(" de ").FontSize(10);
                        x.TotalPages().FontSize(10);
                    });
            });
        });
        
        using (var stream = new MemoryStream())
        {
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }

    public async Task<byte[]> GenerateSalesExcelReportAsync()
    {
        var salesData = await GetSalesMetricsAsync();

        using (var workbook = new ClosedXML.Excel.XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Relatório de Vendas");

            // Título
            worksheet.Cell("A1").Value = "Relatório de Vendas do E-Commerce";
            worksheet.Range("A1:B1").Merge();
            worksheet.Cell("A1").Style.Font.SetBold();
            worksheet.Cell("A1").Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

            // Resumo Geral
            worksheet.Cell("A3").Value = "Resumo Geral";
            worksheet.Cell("A3").Style.Font.SetBold();
            worksheet.Cell("A4").Value = "Total de Vendas:";
            worksheet.Cell("B4").Value = salesData.TotalSales;
            worksheet.Cell("A5").Value = "Total de Pedidos:";
            worksheet.Cell("B5").Value = salesData.TotalOrders;
            worksheet.Cell("A6").Value = "Valor Médio por Pedido:";
            worksheet.Cell("B6").Value = salesData.AverageOrderValue;

            // Vendas por Mês
            worksheet.Cell("A8").Value = "Vendas por Mês";
            worksheet.Cell("A8").Style.Font.SetBold();

            worksheet.Cell("A9").Value = "Mês";
            worksheet.Cell("B9").Value = "Total de Vendas";
            worksheet.Range("A9:B9").Style.Font.SetBold();

            int row = 10;
            foreach (var entry in salesData.SalesByMonth)
            {
                worksheet.Cell($"A{row}").Value = entry.Key;
                worksheet.Cell($"B{row}").Value = entry.Value;
                row++;
            }

            // Formatação de Colunas
            worksheet.Column("B").Style.NumberFormat.Format = "R$ #,##0.00"; // Formato monetário
            worksheet.Columns().AdjustToContents(); // Ajusta a largura das colunas

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}