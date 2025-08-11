namespace Ecommerce.Models.DTOs.Payment;

public class PaymentNotification
{
    public string? Id { get; set; } // ID da notificação ou pagamento
    public string? Topic { get; set; } // Tipo de notificação (payment, merchant_order)
}