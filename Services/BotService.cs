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
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKEN not set.");
                return;
            }

            var loginResult = await client.InitializeUser(token);
            if (!loginResult.Success)
            {
                Console.WriteLine($"Login failed: {loginResult.Message}");
                return;
            }

            Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

            await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);

            client.PlanetService.JoinedPlanetsUpdated += async () =>
            {
                await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);
            };

            client.MessageService.MessageReceived += async (msg) => await MessageService.HandleMessageAsync(client, channelCache, msg, prefix);

            Console.WriteLine("Bot ready and listening...");
        }
    }
}