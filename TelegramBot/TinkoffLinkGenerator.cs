using System.Net.Http;
using Newtonsoft.Json;

namespace TelegramBot;

public class TinkoffLinkGenerator
{
    public async Task<string> GenerateLink()
    {
        using HttpClient http = new();
        var client = new mapiClient(http);
        var init = new Init_FULL()
        {
            TerminalKey = "TinkoffBankTest",
            Amount = 140000,
            OrderId = "2109023781937921",
            Description = "Подарочная карта на 1000 рублей",
            Token = "68711168852240a2f34b6a8b19d2cfbd296c7d2a6dff8b23eda6278985959346"
        };
        Response response = await client.InitAsync(init);
        return response.PaymentURL.ToString();
    }
}
