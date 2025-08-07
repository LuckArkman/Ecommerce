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
}