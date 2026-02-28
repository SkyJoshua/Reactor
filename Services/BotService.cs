using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Services
{
    public static class BotService
    {
        public static async Task InitializeBotAsync(
            string token,
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            HashSet<long> initializedPlanets,
            string prefix)
        {
            //Check token is valid
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKEN not set.");
                return;
            }

            //Login to the bot
            var loginResult = await client.InitializeUser(token);
            if (!loginResult.Success)
            {
                Console.WriteLine($"Login failed: {loginResult.Message}");
                return;
            }
            Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

            //Initialize the Database
            await DatabaseService.InitializeAsync();
            await ReactionRoleService.LoadAllAsync();
            Console.WriteLine($"Loaded {ReactionRoleService.Messages.Count} reaction messages into memory.");

            //Initialize the Planets
            await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);
            client.PlanetService.JoinedPlanetsUpdated += async () =>
            {
                await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);
            };

            //Initialize the Messages
            client.MessageService.MessageReceived += async (msg) => await MessageService.HandleMessageAsync(client, channelCache, msg, prefix);

            //Bot is active and ready
            Console.WriteLine("Bot ready and listening...");
        }
    }
}