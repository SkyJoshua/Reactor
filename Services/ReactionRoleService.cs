using Microsoft.Data.Sqlite;
using Reactor.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Services
{
    public static class ReactionRoleService
    {
        private static readonly string _connectionString = "Data source=reactor.db";

        //Memory Cache
        public static Dictionary<long, ReactionMessage> Messages { get; private set; } = new(); 

        //Load all messages and reaction role mappings
        public static async Task LoadAllAsync()
        {
            Messages.Clear();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            //Load messages
            var cmdMsg = connection.CreateCommand();
            cmdMsg.CommandText = "SELECT Id, PlanetId, ChannelId, MessageId, DeleteDelaySeconds FROM ReactionMessages";
            using var readerMsg = await cmdMsg.ExecuteReaderAsync();
            var tempMessages = new Dictionary<long, ReactionMessage>();

            while (await readerMsg.ReadAsync())
            {
                var msg = new ReactionMessage
                {
                    Id = readerMsg.GetInt64(0),
                    PlanetId = readerMsg.GetInt64(1),
                    ChannelId = readerMsg.GetInt64(2),
                    MessageId = readerMsg.GetInt64(3),
                    DeleteDelaySeconds = readerMsg.GetInt32(4),
                    Reactions = new Dictionary<string, long>()
                };
                tempMessages[msg.Id] = msg;
            }

            //Load reaction role mappings
            var cmdRoles = connection.CreateCommand();
            cmdRoles.CommandText = "SELECT ReactionMessageId, Emoji, RoleId FROM ReactionRoles";
            using var readerRoles = await cmdRoles.ExecuteReaderAsync();
            while (await readerRoles.ReadAsync())
            {
                var msgId = readerRoles.GetInt64(0);
                var emoji = readerRoles.GetString(1);
                var roleId = readerRoles.GetInt64(2);

                if (tempMessages.ContainsKey(msgId))
                {
                    tempMessages[msgId].Reactions[emoji] = roleId;
                }
            }

            //Build lookup by MessageId
            Messages = tempMessages.Values.ToDictionary(m => m.MessageId, m => m);
        }

        public static async Task AddReactionAsync(long messageId, string emoji, long roleId)
        {
            if (!Messages.TryGetValue(messageId, out var msg))
                return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO ReactionRoles (ReactionMessageId, Emoji, RoleId) VALUES (@msgId, @emoji, @roleId)";
            cmd.Parameters.AddWithValue("@msgId", msg.Id);
            cmd.Parameters.AddWithValue("@emoji", emoji);
            cmd.Parameters.AddWithValue("@roleId", roleId);

            await cmd.ExecuteNonQueryAsync();

            //Update Cache
            msg.Reactions[emoji] = roleId;
        }

        public static async Task HandleReactionAddedAsync(
            Dictionary<long, Channel> channelCache,
            Message message)
        {
            if (!Messages.TryGetValue(message.Id, out var cachedMsg))
                return;

            if (!channelCache.TryGetValue(cachedMsg.ChannelId, out var channel))
                return;
            
            foreach (var kvp in message.Reactions)
            {
                string emoji = kvp.Emoji;
                if (!cachedMsg.Reactions.TryGetValue(emoji, out var roleId))
                    continue;

                //Fetch role name
                var role = channel.Planet.Roles.FirstOrDefault(r => r.Id == roleId);
                string roleName = role != null ? role.Name : $"Role {roleId}";

                //Fetch member
                var member = await channel.Planet.FetchMemberAsync(kvp.AuthorUserId);
                if (member == null) return;

                //Apply role to user
                await member.AddRoleAsync(roleId);

                //Confirmation
                var confirm = await channel.SendMessageAsync($"«@m-{member.Id}» has been given the role {roleName}");
                await Task.Delay(cachedMsg.DeleteDelaySeconds * 1000);
                await confirm.Data.DeleteAsync();
            }
        }

        // public static async Task HandleReactionAddedAsync(
        //     ValourClient client,
        //     Dictionary<long, Channel> channelCache,
        //     MessageReaction reaction)
        // {
        //     if (!Messages.TryGetValue(reaction.MessageId, out var msg))
        //         return;

        //     if (!msg.Reactions.TryGetValue(reaction.Emoji, out var roleId))
        //         return;
            
        //     if (!channelCache.TryGetValue(msg.ChannelId, out var channel))
        //         return;

        //     var role = channel.Planet.Roles.FirstOrDefault(r => r.Id == roleId);
        //     string roleName = role != null ? role.Name : $"Role {roleId}";

        //     //Fetch the member
        //     var member = await channel.Planet.FetchMemberAsync(reaction.AuthorUserId);
        //     if (member == null) return;

        //     //Add role
        //     await member.AddRoleAsync(roleId);

        //     //Confirmation
        //     var confirm = await channel.SendMessageAsync($"«@m-{member.Id}» has been given the role {roleName}");
        //     await Task.Delay(msg.DeleteDelaySeconds * 1000);
        //     await confirm.Data.DeleteAsync();
        // }

        // public static async Task HandleReactionRemovedAsync(
        //     ValourClient client,
        //     Dictionary<long, Channel> channelCache,
        //     MessageReaction reaction)
        // {
        //     if (!Messages.TryGetValue(reaction.MessageId, out var msg))
        //         return;

        //     if (!msg.Reactions.TryGetValue(reaction.Emoji, out var roleId))
        //         return;
            
        //     if (!channelCache.TryGetValue(msg.ChannelId, out var channel))
        //         return;

        //     var role = channel.Planet.Roles.FirstOrDefault(r => r.Id == roleId);
        //     string roleName = role != null ? role.Name : $"role {roleId}";

        //     var member = await channel.Planet.FetchMemberAsync(reaction.AuthorUserId);
        //     if (member == null) return;

        //     await member.RemoveRoleAsync(roleId);

        //     var confirm = await channel.SendMessageAsync($"«@m-{member.Id}» has been removed from the role {roleName}");
        //     await Task.Delay(msg.DeleteDelaySeconds * 1000);
        //     await confirm.Data.DeleteAsync();
        // }
    }
}