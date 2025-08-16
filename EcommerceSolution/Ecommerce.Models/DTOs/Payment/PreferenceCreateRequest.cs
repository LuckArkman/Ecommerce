namespace Ecommerce.Models.DTOs.Payment;

public class PreferenceCreateRequest
{
     //public List<MercadoPago.Client.Preference.PreferenceItemRequest> Items { get; set; }
     //public PreferencePayerRequest Payer { get; set; }
     //public PreferenceBackUrlsRequest BackUrls { get; set; }
     public string AutoReturn { get; set; }
     public string NotificationUrl { get; set; }
     // Outras propriedades como ExternalReference, StatementDescriptor, etc.
}