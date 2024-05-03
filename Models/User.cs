namespace TelegramBot.Models;

public class User
{
    public long Id { get; set; }
    public string State { get; set; } = "default";
    public List<Payment> Payments { get; set; } = new();
}