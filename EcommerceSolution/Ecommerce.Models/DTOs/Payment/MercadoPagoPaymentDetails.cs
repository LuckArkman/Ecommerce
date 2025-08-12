using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Payment;

public class MercadoPagoPaymentDetails
{
    [JsonProperty("id")]
    public long Id { get; set; } // ID do Pagamento no MP

    [JsonProperty("status")]
    public string? Status { get; set; } // approved, rejected, pending

    [JsonProperty("status_detail")]
    public string? StatusDetail { get; set; } // Informação mais detalhada do status

    [JsonProperty("external_reference")]
    public string? ExternalReference { get; set; } // Referência externa (seu OrderId, se você o passou)

    [JsonProperty("transaction_amount")]
    public decimal TransactionAmount { get; set; }

    [JsonProperty("currency_id")]
    public string? CurrencyId { get; set; }

    [JsonProperty("payment_type_id")]
    public string? PaymentTypeId { get; set; } // credit_card, ticket, bank_transfer, etc.

    [JsonProperty("date_created")]
    public DateTime DateCreated { get; set; }

    [JsonProperty("date_approved")]
    public DateTime? DateApproved { get; set; }

    [JsonProperty("merchant_order_id")]
    public long? MerchantOrderId { get; set; } // ID da ordem de compra

    [JsonProperty("order")]
    public MercadoPagoOrderDetails? Order { get; set; } // Detalhes da ordem de pagamento

    // Adicione outras propriedades que você precisa da resposta da API de Pagamentos
}
