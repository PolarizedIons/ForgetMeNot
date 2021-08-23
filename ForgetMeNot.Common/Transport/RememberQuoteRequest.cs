namespace ForgetMeNot.Common.Transport
{
    public class RememberQuoteRequest
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong AuthorId { get; set; }
        public string Message { get; set; }
    }
}
