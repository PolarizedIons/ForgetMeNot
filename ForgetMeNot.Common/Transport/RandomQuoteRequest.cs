namespace ForgetMeNot.Common.Transport
{
    public class RandomQuoteRequest
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong? UserId { get; set; }
        public string? SearchTerm { get; set; }
    }
}
