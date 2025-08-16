using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.WebApp.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", 
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "URL de Retorno")]
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Lista de provedores de autenticação externa (Google, Facebook, etc.)
    /// </summary>
    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>
    /// Indica se existem provedores de login externo configurados
    /// </summary>
    public bool HasExternalLogins => ExternalLogins?.Any() ?? false;
}