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

            switch (command)
            {
                case "help":
                    await HelpCommand.Execute(channelCache, channelId, prefix, memberPing);
                    break;
                
                case "source":
                    await SourceCommand.Execute(channelCache, channelId, memberPing);
                    break;
            }
        }   
    }
}