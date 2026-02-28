using Microsoft.Data.Sqlite;
using Reactor.Models;

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
    }
}