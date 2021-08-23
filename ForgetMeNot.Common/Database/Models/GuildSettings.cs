namespace ForgetMeNot.Common.Database.Models
{
    public class GuildSettings : DbEntity
    {
        public ulong GuildId { get; set; }
        public string? SaveReaction { get; set; }
        public bool? LocalQuotes { get; set; }
    }
}
