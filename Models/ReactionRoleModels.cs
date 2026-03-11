namespace Reactor.Models
{
    public class ReactionMessage
    {
        public long Id { get; set; }
        public long PlanetId { get; set; }
        public long ChannelId { get; set; }
        public long MessageId { get; set; }
        public int DeleteDelaySeconds { get; set; } = 5;

        public Dictionary<string, long> Reactions { get; set; } = new();
    }
}