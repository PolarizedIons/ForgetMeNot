namespace ForgetMeNot.Common.Database.Models
{
    public class Quote : DbEntity
    {
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public ulong AuthorId { get; set; }
        public string Message { get; set; } = null!;
    }
}
