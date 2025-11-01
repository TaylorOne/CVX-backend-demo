using Microsoft.EntityFrameworkCore;

namespace CVX.Services
{
    public class TokenReleaseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TokenReleaseBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var tokenReleaseService = scope.ServiceProvider.GetRequiredService<ITokenReleaseService>();

                    var collaboratives = await dbContext.Collaboratives.ToListAsync(stoppingToken);

                    foreach (var collab in collaboratives)
                    {
                        if (tokenReleaseService.ReleaseTokens(collab))
                        {
                            dbContext.Collaboratives.Update(collab);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                // Wait for 24 hours
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}

