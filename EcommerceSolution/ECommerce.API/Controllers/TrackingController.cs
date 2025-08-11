using ECommerce.Application.Interfaces;
using Ecommerce.Models.DTOs.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Rastreamento pode ser público ou exigir login, dependendo da política
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(ITrackingService trackingService, ILogger<TrackingController> logger)
    {
        _trackingService = trackingService;
        _logger = logger;
    }

    // GET /api/Tracking/{trackingNumber}
    [HttpGet("{trackingNumber}")]
    public async Task<ActionResult<TrackingResultDto>> GetTrackingStatus(string trackingNumber)
    {
        if (string.IsNullOrEmpty(trackingNumber))
        {
            return BadRequest("O código de rastreamento é obrigatório.");
        }

        try
        {
            var result = await _trackingService.TrackOrderAsync(trackingNumber);

            if (result.IsError)
            {
                return StatusCode(500, result); // Retorna o DTO de erro
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao rastrear pedido com código {trackingNumber}.");
            return StatusCode(500, new TrackingResultDto { IsError = true, ErrorMessage = "Erro interno ao rastrear pedido." });
        }
    }
}