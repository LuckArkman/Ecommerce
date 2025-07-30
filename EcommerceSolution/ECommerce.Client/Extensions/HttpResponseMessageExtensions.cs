using System.Net.Http;
using System.Threading.Tasks;

namespace ECommerce.Client.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> ReadStringContentAsync(this HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }
    }
}