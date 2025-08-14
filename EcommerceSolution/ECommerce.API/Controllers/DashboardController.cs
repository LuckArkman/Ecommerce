// ECommerce.Api/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs.Dashboard;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
//[ApiController]
//[Authorize(Roles = "Admin")] // Apenas administradores podem acessar o dashboard
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync();
        return Ok(summary);
    }
    
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        try
        {
            var pdfBytes = await _dashboardService.GenerateSalesPdfReportAsync();
            return File(pdfBytes, "application/pdf", "RelatorioVendas.pdf");
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, "Funcionalidade de exportação de PDF não implementada.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar PDF: {ex.Message}");
        }
    }

    // GET /api/Dashboard/export/excel
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel()
    {
        try
        {
            var excelBytes = await _dashboardService.GenerateSalesExcelReportAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RelatorioVendas.xlsx");
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, "Funcionalidade de exportação de Excel não implementada.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar Excel: {ex.Message}");
        }
    }
}