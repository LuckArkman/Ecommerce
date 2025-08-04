// ECommerce.Api/Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using ECommerce.Models.DTOs.Cart;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Todos os endpoints de carrinho exigem autenticação
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserId()
    {
        // Obtém o ID do usuário logado a partir das claims do token JWT
        // Certifique-se de que o NameIdentifier (ou outro claim único) está sendo usado como UserId no Identity
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetUserCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetUserCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> AddOrUpdateCartItem([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        try
        {
            var updatedCartItem = await _cartService.AddOrUpdateCartItemAsync(userId, request);
            return Ok(updatedCartItem); // Retorna o item do carrinho atualizado
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveCartItem(int productId)
    {
        var userId = GetUserId();
        await _cartService.RemoveCartItemAsync(userId, productId);
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}