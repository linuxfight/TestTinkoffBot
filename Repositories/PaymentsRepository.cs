using TelegramBot.Data;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Repositories;

public class PaymentsRepository(ApplicationDbContext context) : Repository<Payment>(context);