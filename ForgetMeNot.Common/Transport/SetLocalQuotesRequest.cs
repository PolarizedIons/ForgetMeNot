namespace ForgetMeNot.Common.Transport
{
    public class SetLocalQuotesRequest
    {
        public ulong GuildId { get; set; }
        public bool UsingLocal { get; set; }
    }
}
