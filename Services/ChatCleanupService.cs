using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;

namespace DripCube.Services
{
    public class ChatCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


                    var cutoff = DateTime.UtcNow.AddMinutes(-15);


                    var activeChats = await context.ChatSessions
                        .Include(c => c.Messages)
                        .Where(c => c.Status != ChatStatus.Closed)
                        .ToListAsync();

                    foreach (var chat in activeChats)
                    {

                        var lastMsg = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();


                        if (lastMsg == null && chat.CreatedAt < cutoff)
                        {
                            context.ChatSessions.Remove(chat);
                            continue;
                        }


                        if (lastMsg != null && lastMsg.SentAt < cutoff)
                        {
                            if (lastMsg.Sender == SenderRole.Manager)
                            {

                                context.ChatSessions.Remove(chat);
                            }
                            else if (lastMsg.Sender == SenderRole.User)
                            {
                                chat.ManagerId = null;
                                chat.Status = ChatStatus.New;
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                }


                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}