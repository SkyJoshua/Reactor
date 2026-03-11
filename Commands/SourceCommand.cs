using Valour.Sdk.Models;

namespace Reactor.Commands;

public static class SourceCommand
{
    public static async Task Execute(Dictionary<long, Channel> channelCache, long channelId, string memberPing)
    {
        if (channelCache.TryGetValue(channelId, out var channel))
        {
            await channel.SendMessageAsync($"{memberPing} You can see my source code here: https://github.com/SkyJoshua/Reactor");
        }
    }
}