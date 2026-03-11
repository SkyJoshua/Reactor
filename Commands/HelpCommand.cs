using Valour.Sdk.Models;

namespace Reactor.Commands;

public static class HelpCommand
{
    public static async Task Execute(Dictionary<long, Channel> channelCache, long channelId, String prefix, string memberPing)
    {
        string helpMessage = $@"**Reactor Commands**:
        - `{prefix}help` - Shows this list.
        - `{prefix}source` - Shows my source code!
        - `{prefix}create` - Creates the Reaction Message.
        - `{prefix}delete` - Deletes a Reaction Message.
        - `{prefix}add` - Adds a Reaction Role to a Valid Message.
        - `{prefix}remove` - Removes a Reaction Role from a Valid Message.
        ";

        if (channelCache.TryGetValue(channelId, out var channel))
        {
            await channel.SendMessageAsync($"{memberPing}\n{helpMessage}");
        }
    }
}