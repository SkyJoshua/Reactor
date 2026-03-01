using Microsoft.Data.Sqlite;
using Reactor.Services;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace Reactor.Commands
{
    public static class CreateCommand
    {
        //Sends a new Reaction Role Message and Stores it
        public static async Task Execute(
            ValourClient client,
            Dictionary<long, Channel> channelCache,
            long channelId,
            string content,
            long planetId,
            int deleteDelaySeconds = 5)
        {
            if (!channelCache.TryGetValue(channelId, out var channel))
            {
                Console.WriteLine($"Channel {channelId} not found in cache.");
                return;
            }

            //Send the Message
            var result = await channel.SendMessageAsync(content);
            if (!result.Success || result.Data == null)
            {
                Console.WriteLine("Failed to send message.");
                return;
            }

            var sentMessage = result.Data;
            await channel.SendMessageAsync($"This Reaction Message has the ID of: {sentMessage.Id}");

            //Insert into DB
            using var connection = new SqliteConnection("Data Source=reactor.db");
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ReactionMessages (PlanetId, ChannelId, MessageId, DeleteDelaySeconds)
                VALUES (@planetId, @channelId, @messageId, @delay);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@planetId", planetId);
            cmd.Parameters.AddWithValue("@channelId", channelId);
            cmd.Parameters.AddWithValue("@messageId", sentMessage.Id);
            cmd.Parameters.AddWithValue("@delay", deleteDelaySeconds);

            var insertedId = (long)await cmd.ExecuteScalarAsync();

            //Add to memory
            ReactionRoleService.Messages[sentMessage.Id] = new Models.ReactionMessage
            {
                Id = insertedId,
                PlanetId = planetId,
                ChannelId = channelId,
                MessageId = sentMessage.Id,
                DeleteDelaySeconds = deleteDelaySeconds,
                Reactions = new Dictionary<string, long>()
            };

            //Subscribe events
            sentMessage.ReactionAdded += async () =>
            {
                await ReactionRoleService.HandleReactionAddedAsync(channelCache, sentMessage);
            };

            Console.WriteLine($"Created reaction message {sentMessage.Id} in channel {channelId}");
        }
    }
}