namespace ForgetMeNot.Common.Transport
{
    public class ListQuotesRequest
    {
        public int PageNumber = 1;
        public int PageSize = 20;
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
    }
}
