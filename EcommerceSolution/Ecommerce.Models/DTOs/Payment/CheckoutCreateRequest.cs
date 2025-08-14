using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs.Payment;

public class CheckoutCreateRequest
{
    [Required]
    public int OrderId { get; set; } // O ID do pedido que está sendo pago
    [Required]
    public string OrderDescription { get; set; } = string.Empty; // Descrição do que está sendo pago
    [Required]
    public decimal TotalAmount { get; set; }
    [Required]
    public string PayerEmail { get; set; } = string.Empty; // Email do comprador
    public string? SuccessUrl { get; set; } // URL para onde o MP redireciona após pagamento bem-sucedido
    public string? FailureUrl { get; set; } // URL para onde o MP redireciona após pagamento falho
    public string? PendingUrl { get; set; } // URL para onde o MP redireciona após pagamento pendente
}