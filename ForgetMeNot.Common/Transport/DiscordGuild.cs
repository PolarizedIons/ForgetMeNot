using System.Collections.Generic;

namespace ForgetMeNot.Common.Transport
{
    public class DiscordGuild
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public IEnumerable<DiscordChannel> Channels { get; set; }
    }
}
