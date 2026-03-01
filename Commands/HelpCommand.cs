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
        ";

        if (channelCache.TryGetValue(channelId, out var channel))
        {
            await channel.SendMessageAsync($"{memberPing}\n{helpMessage}");
        }
    }
}