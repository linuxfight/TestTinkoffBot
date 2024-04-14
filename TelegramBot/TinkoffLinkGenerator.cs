using System.Net.Http;
using Newtonsoft.Json;

namespace TelegramBot;

public class TinkoffLinkGenerator
{
    public async Task<OrderResult> GenerateLink()
    {
        using HttpClient client = new();
        const string url = "https://securepay.tinkoff.ru/v2/Init";
        HttpResponseMessage response = await client.PostAsync(url, null);
        string content = await response.Content.ReadAsStringAsync();
        OrderResult result = JsonConvert.DeserializeObject<OrderResult>(content);
        return result;
    }
}
