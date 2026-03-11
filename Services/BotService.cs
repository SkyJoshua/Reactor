using System.ComponentModel;
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
            await ReactionRoleService.LoadAllAsync(client);
            Console.WriteLine($"Loaded {ReactionRoleService.Messages.Count} reaction messages into memory.");

            //Initialize the Planets
            await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);
            client.PlanetService.JoinedPlanetsUpdated += async () =>
            {
                await PlanetService.InitializePlanetsAsync(client, channelCache, initializedPlanets);
            };

            //Fucking pain in my ass is what this is, i dont even wanna comment on it
            foreach (var reactionMessage in ReactionRoleService.Messages.Values.ToList())
            {
                try
                {
                    if(!channelCache.TryGetValue(reactionMessage.ChannelId, out var channel))
                    {
                        Console.WriteLine($"Channel {reactionMessage.ChannelId} not found, pruning message {reactionMessage.MessageId}.");
                        await ReactionRoleService.RemoveMessageAsync(reactionMessage.MessageId);
                        continue;
                    }

                    var messages = await channel.GetMessagesAsync(reactionMessage.MessageId + 1, 50);
                    Console.WriteLine($"Fetched {messages?.Count ?? 0} messages from channel {reactionMessage.ChannelId}");
                    var match = messages?.FirstOrDefault(m => m.Id == reactionMessage.MessageId);

                    if (match == null)
                    {
                        Console.WriteLine($"Message {reactionMessage.MessageId} not found, pruning.");
                        await ReactionRoleService.RemoveMessageAsync(reactionMessage.MessageId);
                        continue;
                    }

                    ReactionRoleService.SubscribeToMessageReactions(client, channelCache, match);
                    Console.WriteLine($"Subscribed to reactions for message {reactionMessage.MessageId}");
                } catch (Exception ex)
                {
                Console.WriteLine($"Error setting up message {reactionMessage.MessageId}: {ex.Message}, pruning.");
                await ReactionRoleService.RemoveMessageAsync(reactionMessage.MessageId);
                }
            }

            client.MessageService.MessageReceived += async (message) =>
            {
                await MessageService.HandleMessageAsync(client, channelCache, message, prefix);
            };

            client.MessageService.MessageDeleted += async (message) =>
            {
                if (ReactionRoleService.Messages.ContainsKey(message.Id))
                {
                    await ReactionRoleService.RemoveMessageAsync(message.Id);
                    ReactionRoleService.ResetSubscription(message.Id);
                    Console.WriteLine($"Reaction message {message.Id} was deleted, removed from DB and cache.");
                }
            };

            

            //Bot is active and ready
            Console.WriteLine("Bot ready and listening...");
        }
    }
}