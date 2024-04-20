using System.Net.Http;
using Newtonsoft.Json;

namespace TelegramBot;

public static class TinkoffLinkGenerator
{
    public static async Task<Response> GenerateLink()
    {
        using HttpClient http = new();
        mapiClient client = new(http);
        Init_FULL init = new()
        {
            TerminalKey = "TinkoffBankTest",
            Amount = 140000,
            OrderId = "2109023781937921",
            Description = "Подарочная карта на 1000 рублей",
            Token = "68711168852240a2f34b6a8b19d2cfbd296c7d2a6dff8b23eda6278985959346"
        };
        return await client.InitAsync(init);
    }
}
