using System.Net.Http;
using System.Net.Http.Json;

namespace TelegramBot;

public class TinkoffLinkGenerator
{
    public async Task<string> GenerateLink()
    {
        using HttpClient client = new();
        const string url = "https://securepay.tinkoff.ru/v2/Init";
        var testRequestData = new 
        {
            TerminalKey = "TinkoffBankTest",
            Amount = 140000,
            OrderId = "21090",
            Description = "Подарочная карта на 1000 рублей",
            Token = "68711168852240a2f34b6a8b19d2cfbd296c7d2a6dff8b23eda6278985959346",
            DATA = new 
            {
                Phone ="+71234567890",
                Email = "a@test.com"
            },
            Receipt = new 
            {
                Email = "a@test.ru",
                Phone = "+79031234567",
                Taxation = "osn",
                Items = new List<object>()
                {
                    new 
                    {
                        Name = "Наименование товара 1",
                        Price = 10000,
                        Quantity = 1,
                        Amount = 10000,
                        Tax = "vat10",
                        Ean13 = "303130323930303030630333435"
                    },
                    new 
                    {
                        Name = "Наименование товара 2",
                        Price = 20000,
                        Quantity = 2,
                        Amount = 40000,
                        Tax = "vat20"
                    },
                    new 
                    {
                        Name = "Наименование товара 3",
                        Price = 30000,
                        Quantity = 3,
                        Amount = 90000,
                        Tax = "vat10"
                    }
                }
            }
        };
        JsonContent content = JsonContent.Create(testRequestData);
        HttpResponseMessage response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}
