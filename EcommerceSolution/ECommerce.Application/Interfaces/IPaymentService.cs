using Ecommerce.Models.DTOs.Payment;

namespace ECommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<CheckoutResponse> CreateCheckoutPreferenceAsync(CheckoutCreateRequest request);
    Task ProcessPaymentNotificationAsync(string topic, string id); // Para webhooks/IPN
}