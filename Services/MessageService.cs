using Reactor.Commands;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Services
{
    public static class MessageService
    {
        public static async Task HandleMessageAsync(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            Message message,
            string prefix)
        {
            //Bot cant reply to its self hahahahahaha loser!
            if (message.AuthorUserId == client.Me.Id) return;

            string content = message.Content ?? "";
            if (string.IsNullOrWhiteSpace(content)) return;
            if (!content.StartsWith(prefix)) return;

            long channelId = message.ChannelId;

            var member = await message.FetchAuthorMemberAsync();
            string memberPing = member != null ? $"«@m-{member.Id}»" : "";

            string withoutPrefix = content.Substring(prefix.Length);

            var parts = withoutPrefix.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string command = parts[0].ToLower();
            string[] args = parts[1..];

            //Commands.. duh..
            switch (command)
            {
                case "help":
                    await HelpCommand.Execute(channelCache, channelId, prefix, memberPing);
                    break;
                
                case "source":
                    await SourceCommand.Execute(channelCache, channelId, memberPing);
                    break;
                
                case "create":
                    if (parts.Length < 2)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Usage: {prefix}create <default message text>");
                        return;
                    }

                    if (message.PlanetId == null)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Could not detect planet ID for this message. Please contact me if you are seeing this.");
                        return;
                    }

                    var messageText = string.Join(' ', parts[1..]);
                    await CreateCommand.Execute(channelCache, channelId, messageText, message.PlanetId.Value);
                    break;

                case "add":
                    if (parts.Length < 4)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Usage: {prefix}add <messageId> <emoji> <roleId>");
                        return;
                    }
                    
                    if (!long.TryParse(parts[1], out var msgId))
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Invalid message ID.");
                        return;
                    }

                    var emoji = parts[2];

                    if (!long.TryParse(parts[3], out var roleId))
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Invalid role ID.");
                        return;
                    }

                    await AddCommand.Execute(channelCache, channelId, msgId, emoji, roleId, client, message.Planet);
                    break;
            }
        }   
    }
}