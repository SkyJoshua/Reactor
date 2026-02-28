using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using Valour.Shared.Models;
using Reactor.Commands;

namespace Reactor
{
    public class Reactor
    {
        private ValourClient _client;
        private Dictionary<long, Channel> _channelCache = new();
        private HashSet<long> _initializedPlanets = new();
        private string _prefix = "r.";

        public Reactor(string token)
        {
            Env.Load();
            _client = new ValourClient("https://api.valour.gg/");
            _client.SetupHttpClient();
            InitializeBotAsync(token).GetAwaiter().GetResult();
        }

        //Initialize the bot.
        private async Task InitializeBotAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKEN not set.");
                return;
            }

            var loginResult = await _client.InitializeUser(token);
            if (!loginResult.Success)
            {
                Console.WriteLine($"Login failed: {loginResult.Message}");
                return;
            }

            Console.WriteLine($"Logged in as {_client.Me.Name} (ID: {_client.Me.Id})");

            await InitializePlanetsAsync();

            _client.PlanetService.JoinedPlanetsUpdated += async () =>
            {
                await InitializePlanetsAsync();
            };

            _client.MessageService.MessageReceived += async (msg) => await HandleMessageAsync(msg);

            Console.WriteLine("Bot ready and listening...");
        }

        //Initalize the planets.
        private async Task InitializePlanetsAsync()
        {
            foreach (var planet in _client.PlanetService.JoinedPlanets)
            {
                if (_initializedPlanets.Contains(planet.Id))
                    continue;

                Console.WriteLine($"Initializing Planet: {planet.Name}");

                await planet.EnsureReadyAsync();
                await planet.FetchInitialDataAsync();

                foreach (var channel in planet.Channels)
                {
                    _channelCache[channel.Id] = channel;

                    if (channel.ChannelType == ChannelTypeEnum.PlanetChat)
                    {
                        await channel.OpenWithResult("Reactor");
                        Console.WriteLine($"Realtime opened for: {planet.Name} -> {channel.Name}");
                    }
                }

                _initializedPlanets.Add(planet.Id);
            }
        }

        //Message handler.
        private async Task HandleMessageAsync(Message message)
        {
            if (message.AuthorUserId == _client.Me.Id) return;

            string content = message.Content ?? "";
            if (string.IsNullOrWhiteSpace(content)) return;
            if (!content.StartsWith(_prefix)) return;

            long channelId = message.ChannelId;

            var member = await message.FetchAuthorMemberAsync();
            string memberPing = member != null ? $"«@m-{member.Id}»" : "";

            string withoutPrefix = content.Substring(_prefix.Length);

            var parts = withoutPrefix.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string command = parts[0].ToLower();
            string[] args = parts[1..];

            switch (command)
            {
                case "help":
                    await HelpComamnd.Execute(_channelCache, channelId, _prefix, memberPing);
                    break;
                
                case "source":
                    await SourceComamnd.Execute(_channelCache, channelId, memberPing);
                    break;
            }
        }

    }


    //Because it required a main or something idk I hate C# :)
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

            var bot = new Reactor(token);

            await Task.Delay(Timeout.Infinite);
        }
    }
}