using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Belfast.Services;
using Discord.Commands;

namespace Belfast
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += Log;
                services.GetRequiredService<CommandService>().Log += Log;
                
                await client.LoginAsync(TokenType.Bot, services.GetService<Configuration>().Token);
                await client.StartAsync();

                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            // TODO: Load config in memory from file directly. 
            DotNetEnv.Env.TraversePath().Load();
            Configuration conf = new Configuration
            {
                Prefix = Environment.GetEnvironmentVariable("Prefix"),
                Token = Environment.GetEnvironmentVariable("DiscordToken")
            };

            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<Configuration>(conf)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }
    }
} 