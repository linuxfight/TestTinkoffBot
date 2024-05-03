using TelegramBot.Data;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Repositories;

public class UserRepository(ApplicationDbContext context) : Repository<User>(context);