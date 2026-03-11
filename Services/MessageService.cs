using Reactor.Commands;
using Valour.Sdk.Client;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

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

            bool hasPermission = await HasPermissionAsync(member);

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

                    if (!hasPermission)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} You need Manage Roles or Full Control to use this command.");
                        return;
                    }

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
                    await CreateCommand.Execute(client, channelCache, channelId, messageText, message.PlanetId.Value);
                    break;

                case "delete":

                    if (!hasPermission)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} You need Manage Roles or Full Control to use this command.");
                        return;
                    }

                    if (parts.Length < 2)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Usage: {prefix}delete <messageId>");
                        return;
                    }

                    if (!long.TryParse(parts[1], out var deleteMsgId))
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Invalid message ID.");
                        return;
                    }

                    await DeleteCommand.Execute(client, channelCache, channelId, deleteMsgId);
                    break;

                case "add":

                    if (!hasPermission)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} You need Manage Roles or Full Control to use this command.");
                        return;
                    }

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

                    await AddCommand.Execute(client, channelCache, channelId, msgId, emoji, roleId);
                    break;
                
                case "remove":

                    if (!hasPermission)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} You need Manage Roles or Full Control to use this command.");
                        return;
                    }

                    if (parts.Length < 3)
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Usage: {prefix}remove <messageId> <emoji>");
                        return;
                    }

                    if (!long.TryParse(parts[1], out var removeMsgId))
                    {
                        await channelCache[channelId].SendMessageAsync($"{memberPing} Invalid message ID.");
                        return;
                    }

                    var removeEmoji = parts[2];
                    await RemoveCommand.Execute(client, channelCache, channelId, removeMsgId, removeEmoji);
                    break;
            }
        }   

        private static async Task<bool> HasPermissionAsync(PlanetMember member)
        {
            if (member == null) return false;
            
            return member.HasPermission(PlanetPermissions.FullControl) ||
                member.HasPermission(PlanetPermissions.ManageRoles);
        }
    }
}