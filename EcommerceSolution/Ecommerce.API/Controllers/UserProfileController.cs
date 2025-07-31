// ECommerce.Api/Controllers/UserProfileController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using ECommerce.Models.DTOs.User;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public UserProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userProfile = await _userService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound("Perfil do usuário não encontrado.");
        }
        return Ok(userProfile);
    }
}