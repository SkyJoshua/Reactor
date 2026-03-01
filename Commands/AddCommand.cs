using Reactor.Services;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Commands
{
    public static class AddCommand
    {
        public static async Task Execute(
            Dictionary<long, Channel> channelCache,
            long channelId,
            long messageId,
            string emoji,
            long roleId,
            ValourClient client,
            Planet planet)
        {
            //Check if the current channel is in the cache (should never happen but you never know!)
            if (!channelCache.TryGetValue(channelId, out var channel))
            {
                Console.WriteLine($"Channel {channelId} not found in cache.");
                return;
            }

            //Check if the message id is a valid reaction message
            if (!ReactionRoleService.Messages.TryGetValue(messageId, out var reactionMsg))
            {
                await channel.SendMessageAsync($"Message ID {messageId} is not tracked as a reaction message.");
                return;
            }

            //Fetch recent messages
            var recentMessages = await channel.GetLastMessagesAsync(50);

            //Try and find the message inside those recent messages
            var message = recentMessages.FirstOrDefault(m => m.Id == messageId);
            if (message == null)
            {
                await channel.SendMessageAsync("Could not find the message in the last 50 messages.");
                return;
            }

            // Add the emoji to the message
            await message.AddReactionAsync(emoji);

            //Add reaction-role mapping to DB and Cache
            await ReactionRoleService.AddReactionAsync(messageId, emoji, roleId);

            await channel.SendMessageAsync($"Added reaction {emoji} -> role {roleId} for message {messageId}");
        }
    }
}