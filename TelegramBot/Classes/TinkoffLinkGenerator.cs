using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace TelegramBot.Classes;

public static class TinkoffLinkGenerator
{
    private static string HashString(string str)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(str);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    
    private static string GetToken(Init_FULL init)
    {
        Dictionary<string, object> initDictionary = new Dictionary<string, object>();
        PropertyInfo[] properties = init.GetType().GetProperties();
        foreach (var property in properties)
            initDictionary[property.Name] = property.GetValue(init);
        var initList = initDictionary.ToList().Select(x => (x.Key, x.Value)).ToList();
        initList.Add(("Password", "mm0b1ner9ztu118y"));
        initList.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));
        var initString = "";
        foreach (var initItem in initList)
            initString += initItem.Value;
        return HashString(initString);
    }
    
    public static async Task<Response> GenerateLink()
    {
        using HttpClient http = new();
        mapiClient client = new(http);
        Init_FULL init = new()
        {
            TerminalKey = "1699025675147DEMO",
            Amount = 140000,
            OrderId = "2109023781937921",
            Description = "Подарочная карта на 1000 рублей",
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
        init.Token = GetToken(init);
        return await client.InitAsync(init);
    }
}
