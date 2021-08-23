namespace ForgetMeNot.Common.Transport
{
    public class SearchQuoteRequest
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong? UserId { get; set; }
        public string? SearchTerm { get; set; }
    }
}
