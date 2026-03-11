using Valour.Sdk.Client;
using Valour.Sdk.ModelLogic;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace Reactor.Services
{
    public static class PlanetService
    {
        public static async Task InitializePlanetsAsync(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            HashSet<long> initializedPlanets)
        {
            foreach (var planet in client.PlanetService.JoinedPlanets)
            {
                if (initializedPlanets.Contains(planet.Id))
                    continue;

                Console.WriteLine($"Initializing Planet: {planet.Name}");

                await planet.EnsureReadyAsync();
                await planet.FetchInitialDataAsync();
                

                foreach (var channel in planet.Channels)
                {
                    channelCache[channel.Id] = channel;

                    if (channel.ChannelType == ChannelTypeEnum.PlanetChat)
                    {
                        await channel.OpenWithResult("Reactor");
                        Console.WriteLine($"Realtime opened for: {planet.Name} -> {channel.Name}");
                    }
                }

                Action<IModelEvent<Channel>> channelChangedHandler = (evt) =>
                {
                    _ = Task.Run(async () =>
                    {
                        foreach (var channel in planet.Channels)
                        {
                            if (channelCache.ContainsKey(channel.Id))
                                continue;

                            channelCache[channel.Id] = channel;

                            if (channel.ChannelType == ChannelTypeEnum.PlanetChat)
                            {
                                await channel.OpenWithResult("Reactor");
                                Console.WriteLine($"New channel detected: {planet.Name} -> {channel.Name}");
                            }
                        }
                    });
                };

                planet.Channels.Changed += channelChangedHandler;

                initializedPlanets.Add(planet.Id);
            }
        }
    }
}