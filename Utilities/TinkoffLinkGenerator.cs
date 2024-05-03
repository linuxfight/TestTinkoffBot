using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace TelegramBot.Utilities;

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
        string initJson = JsonConvert.SerializeObject(init);
        Dictionary<string, object>? initDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(initJson);
        initDictionary?.Remove("Token");
        if (initDictionary != null)
        {
            var initList = initDictionary.ToList().Select(x => (x.Key, x.Value)).ToList();
            initList.Add(("Password", "mm0b1ner9ztu118y"));
            initList.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));
            var initString = "";
            foreach (var initItem in initList)
                initString += initItem.Value;
            return HashString(initString);
        }

        return "Error while hashing string";
    }
    
    public static async Task<Response> GenerateLink(double price, string description, string email, string phoneNumber)
    {
        using HttpClient http = new();
        mapiClient client = new(http);
        string orderId = Guid.NewGuid().ToString();
        Init_FULL init = new()
        {
            TerminalKey = "1699025675147DEMO",
            OrderId = orderId,
            Amount = price * 100,
            Token = "",
            Description = description,
            Receipt = new()
            {
                Email = email,
                Phone = phoneNumber,
                Items = new List<Items_FFD_105>
                {
                    new()
                    {
                        Name = "Основной заказ",
                        Price = price * 100,
                        Quantity = 1,
                        Amount = price * 100,
                        Tax = Items_FFD_105Tax.Vat0
                    }
                }
            }
        };
        init.Token = GetToken(init);
        return await client.InitAsync(init);
    }
}