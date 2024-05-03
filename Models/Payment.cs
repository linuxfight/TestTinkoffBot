using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBot.Models;

public class Payment
{
    [Key]
    public long Id { get; set; }
    public string Description { get; set; } = "Не указано";
    public double Price { get; set; } = 1400;
    public string UserEmail { get; set; } = "a@test.ru";
    public string UserPhone { get; set; } = "+79123456789";
    public long CreatorId { get; set; }
    [ForeignKey("CreatorId")]
    public required User Creator { get; set; }
}