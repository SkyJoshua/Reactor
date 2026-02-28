using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using Reactor.Services;

namespace Reactor
{
    public class Reactor
    {
        private readonly ValourClient _client;
        private readonly Dictionary<long, Channel> _channelCache = new();
        private readonly HashSet<long> _initializedPlanets = new();
        private readonly string _prefix = "r.";

        public Reactor()
        {
            _client = new ValourClient("https://api.valour.gg/");
            _client.SetupHttpClient();
        }

        public async Task StartAsync(string token)
        {
            await BotService.InitializeBotAsync(
                token,
                _client,
                _channelCache,
                _initializedPlanets,
                _prefix
            );
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();

            var token = Environment.GetEnvironmentVariable("TOKEN");

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKEN not set.");
                return;
            }

            var bot = new Reactor();
            await bot.StartAsync(token);

            await Task.Delay(Timeout.Infinite);
        }
    }
}