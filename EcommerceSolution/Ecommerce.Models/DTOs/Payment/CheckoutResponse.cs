namespace Ecommerce.Models.DTOs.Payment;

public class CheckoutResponse
{
    public string? CheckoutUrl { get; set; } // URL para redirecionar o usuário para o Mercado Pago
    public string? PaymentId { get; set; } // ID da preferência de pagamento no Mercado Pago
    public string? Message { get; set; }
    public bool Success { get; set; }
}