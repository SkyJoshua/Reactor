using Reactor.Services;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Commands
{
    public static class DeleteCommand
    {
        public static async Task Execute(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            long channelId,
            long messageId)
        {
            //Check if channel in cache
            if (!channelCache.TryGetValue(channelId, out var channel))
            {
                Console.WriteLine($"Channel {channelId} not found in cache.");
                return;
            }

            //Check if message is actually a reaction message
            if (!ReactionRoleService.Messages.TryGetValue(messageId, out var reactionMsg))
            {
                await channel.SendMessageAsync($"Message ID {messageId} is not tracked as a reaction message.");
                return;
            }

            //Delete the actual message
            var recentMessages = await channel.GetLastMessagesAsync(50);
            var message = recentMessages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                await message.DeleteAsync();
            } else
            {
                Console.WriteLine($"Message {messageId} not found in recent messages, skipping deletion of message.");
            }

            //Remove from cache and database
            await ReactionRoleService.RemoveMessageAsync(messageId);
            ReactionRoleService.ResetSubscription(messageId);

            await channel.SendMessageAsync($"Deleted reaction message {messageId} and all its role mappings.");
        }
    }
}