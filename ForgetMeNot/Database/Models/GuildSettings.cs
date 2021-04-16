using Discord;

namespace ForgetMeNot.Database.Models
{
    public class GuildSettings : DbEntity
    {
        public ulong GuildId { get; set; }
        public string? SaveReaction { get; set; }
    }
}
