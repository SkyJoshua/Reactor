using Reactor.Services;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Commands
{
    public static class RemoveCommand
    {
        public static async Task Execute(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            long channelId,
            long messageId,
            string emoji)
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

            //Check if the emoji is actually a valid reaction on the message
            if (!reactionMsg.Reactions.ContainsKey(emoji))
            {
                await channel.SendMessageAsync($"Emoji {emoji} is not mapped to any role on message {messageId}.");
                return;
            }

            //Fetch the message and remove the reaction
            var recentMessages = await channel.GetLastMessagesAsync(50);
            var message = recentMessages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                await message.RemoveReactionAsync(emoji);
            }

            await ReactionRoleService.RemoveReactionAsync(messageId, emoji);

            await channel.SendMessageAsync($"Removed reaction {emoji} from message {messageId}.");
        }
    }
}