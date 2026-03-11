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
        public static async Task LoadAllAsync(ValourClient client)
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

        public static async Task RemoveReactionAsync(long messageId, string emoji)
        {
            if (!Messages.TryGetValue(messageId, out var msg))
                return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM ReactionRoles WHERE ReactionMessageId = @msgId AND Emoji = @emoji";
            cmd.Parameters.AddWithValue("@msgId", msg.Id);
            cmd.Parameters.AddWithValue("@emoji", emoji);
            await cmd.ExecuteNonQueryAsync();

            msg.Reactions.Remove(emoji);
            Console.WriteLine($"Removed reaction {emoji} from message {messageId}.");
        }

        private static readonly HashSet<long> _subscribedMessages = new();

        public static void SubscribeToMessageReactions(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            Message syncedMessage)
        {
            if (!_subscribedMessages.Add(syncedMessage.Id))
            {
                Console.WriteLine($"Already subscribed to message {syncedMessage.Id}, skipping.");
                return;
            }

            Action<MessageReaction> addhandler = (reaction) =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"Reaction added: {reaction.Emoji} by {reaction.AuthorUserId}");
                        await HandleReactionAddedAsync(client, channelCache, syncedMessage, reaction);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in addhandler: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                });
            };

            Action<MessageReaction> removehandler = (reaction) =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"Reaction removed: {reaction.Emoji} by {reaction.AuthorUserId}");
                        await HandleReactionRemovedAsync(client, channelCache, syncedMessage, reaction);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in removehandler: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                });
            };

            syncedMessage.ReactionAdded += addhandler;
            syncedMessage.ReactionRemoved += removehandler;
        }

        public static void ResetSubscription(long messageId)
        {
            _subscribedMessages.Remove(messageId);
        }

        public static async Task HandleReactionAddedAsync(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            Message message,
            MessageReaction reaction)
        {
            if (!Messages.TryGetValue(message.Id, out var cachedMsg))
                return;

            if (!channelCache.TryGetValue(cachedMsg.ChannelId, out var channel))
                return;

            if (!cachedMsg.Reactions.TryGetValue(reaction.Emoji, out var roleId))
                return;

            var role = channel.Planet.Roles.FirstOrDefault(r => r.Id == roleId);
            string roleName = role != null ? role.Name : $"Role {roleId}";

            var member = await channel.Planet.FetchMemberByUserAsync(reaction.AuthorUserId);
            if (member == null) return;

            // Check if member already has the role
            if (member.Roles.Any(r => r.Id == roleId))
            {
                Console.WriteLine($"User {reaction.AuthorUserId} already has role {roleId}, skipping.");
                return;
            }

            await member.AddRoleAsync(roleId);

            var confirm = await channel.SendMessageAsync($"«@m-{member.Id}» has been added to the role {roleName}");
            if (confirm.Success && confirm.Data != null)
            {
                await Task.Delay(cachedMsg.DeleteDelaySeconds * 1000);
                if (client.Cache.Messages.TryGet(confirm.Data.Id, out var cachedConfirm))
                {
                    await cachedConfirm.DeleteAsync();
                } else
                {
                    Console.WriteLine($"Could not find confirmation message {confirm.Data.Id} in cache.");
                }
            }
        }

        public static async Task HandleReactionRemovedAsync(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            Message message,
            MessageReaction reaction)
        {
            if (!Messages.TryGetValue(message.Id, out var cachedMsg))
                return;

            if (!channelCache.TryGetValue(cachedMsg.ChannelId, out var channel))
                return;

            if (!cachedMsg.Reactions.TryGetValue(reaction.Emoji, out var roleId))
                return;

            var role = channel.Planet.Roles.FirstOrDefault(r => r.Id == roleId);
            string roleName = role != null ? role.Name : $"Role {roleId}";

            var member = await channel.Planet.FetchMemberByUserAsync(reaction.AuthorUserId);
            if (member == null) return;

            // Check if member actually has the role before removing
            if (!member.Roles.Any(r => r.Id == roleId))
            {
                Console.WriteLine($"User {reaction.AuthorUserId} does not have role {roleId}, skipping.");
                return;
            }

            await member.RemoveRoleAsync(roleId);

            var confirm = await channel.SendMessageAsync($"«@m-{member.Id}» has been removed from the role {roleName}");
            if (confirm.Success && confirm.Data != null)
            {
                await Task.Delay(cachedMsg.DeleteDelaySeconds * 1000);
                if (client.Cache.Messages.TryGet(confirm.Data.Id, out var cachedConfirm))
                {
                    await cachedConfirm.DeleteAsync();
                } else
                {
                    Console.WriteLine($"Could not find confirmation message {confirm.Data.Id} in cache.");
                }
            }
        }

        public static async Task RemoveMessageAsync(long messageId)
        {
            if (!Messages.TryGetValue(messageId, out var msg)) return;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM ReactionRoles WHERE ReactionMessageId = @id;
                DELETE FROM ReactionMessages WHERE MessageId = @messageId;
            ";
            cmd.Parameters.AddWithValue("@id", msg.Id);
            cmd.Parameters.AddWithValue("@messageId", messageId);
            await cmd.ExecuteNonQueryAsync();

            Messages.Remove(messageId);
            Console.WriteLine($"Removed stale reaction message {messageId} from DB and memory.");
        }
    }
}