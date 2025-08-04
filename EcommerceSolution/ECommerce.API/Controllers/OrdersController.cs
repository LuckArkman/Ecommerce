// ECommerce.Api/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.DTOs.Order;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Todos os endpoints de pedido exigem autenticação
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetUserOrder), new { orderId = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar pedido.", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(GetUserId());
        return Ok(orders);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetUserOrder(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.UserId != GetUserId()) // Garante que o usuário só veja seus próprios pedidos
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")] // Apenas admin pode ver todos os pedidos
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("{orderId}/status")]
    [Authorize(Roles = "Admin")] // Apenas admin pode atualizar status
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}