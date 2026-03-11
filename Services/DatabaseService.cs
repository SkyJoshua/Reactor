using Microsoft.Data.Sqlite;

namespace Reactor.Services
{
    public static class DatabaseService
    {
        private static string _connectionString = "Data Source=reactor.db";

        public static async Task InitializeAsync()
        {
            //Connection frfr
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            //ReactionMessages Table
            var cmd1 = connection.CreateCommand();
            cmd1.CommandText =
                "CREATE TABLE IF NOT EXISTS ReactionMessages (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "PlanetId INTEGER NOT NULL, " +
                "ChannelId INTEGER NOT NULL, " +
                "MessageId INTEGER NOT NULL UNIQUE, " +
                "DeleteDelaySeconds INTEGER NOT NULL DEFAULT 5" +
                ")";
            await cmd1.ExecuteNonQueryAsync();

            //ReactionRoles table
            var cmd2 = connection.CreateCommand();
            cmd2.CommandText =
                "CREATE TABLE IF NOT EXISTS ReactionRoles (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "ReactionMessageId INTEGER NOT NULL, " +
                "Emoji TEXT NOT NULL, " +
                "RoleId INTEGER NOT NULL, " +
                "FOREIGN KEY (ReactionMessageId) REFERENCES ReactionMessages(Id) ON DELETE CASCADE" +
                ")";
            await cmd2.ExecuteNonQueryAsync();
        }
    }
}