namespace TelegramBot.Classes;

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
            Token = "68711168852240a2f34b6a8b19d2cfbd296c7d2a6dff8b23eda6278985959346",
            Receipt = new()
            {
                Email = "a@test.ru",
                Phone = "+79031234567",
                Items = new List<Items_FFD_105>()
                {
                    new()
                    {
                        Name = "Наименование товара 1",
                        Price = 10000,
                        Quantity = 1,
                        Amount = 10000,
                        Tax = Items_FFD_105Tax.Vat10,
                        Ean13 = "303130323930303030630333435"
                    },
                    new()
                    {
                        Name = "Наименование товара 2",
                        Price = 20000,
                        Quantity = 2,
                        Amount = 40000,
                        Tax = Items_FFD_105Tax.Vat20
                    },
                    new()
                    {
                        Name = "Наименование товара 3",
                        Price = 30000,
                        Quantity = 3,
                        Amount = 90000,
                        Tax = Items_FFD_105Tax.Vat10
                    }
                }
            }
        };
        return await client.InitAsync(init);
    }
}
