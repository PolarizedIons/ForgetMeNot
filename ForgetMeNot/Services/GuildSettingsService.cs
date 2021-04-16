using System.Threading.Tasks;
using Discord;
using ForgetMeNot.Database;
using ForgetMeNot.Database.Models;
using ForgetMeNot.Extentions;
using Microsoft.EntityFrameworkCore;

namespace ForgetMeNot.Services
{
    public class GuildSettingsService : IScopedDiService
    {
        private readonly DatabaseContext _db;
        
        public GuildSettingsService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<GuildSettings> GetGuildSettings(ulong guildId)
        {
            var settings = await _db.GuildSettings
                .FirstOrDefaultAsync(x =>
                    x.GuildId == guildId &&
                    x.DeletedAt == null
                );

            if (settings == null)
            {
                settings = new GuildSettings
                {
                    GuildId = guildId,
                };

                await _db.AddAsync(settings);
                await _db.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<bool> IsSaveReaction(ulong guildId, IEmote emote)
        {
            var guildSettings = await GetGuildSettings(guildId);
            return guildSettings.SaveReaction == emote.ToString();
        }

        public async Task<GuildSettings> SetSaveReaction(ulong guildId, IEmote emote)
        {
            var guildSettings = await GetGuildSettings(guildId);
            guildSettings.SaveReaction = emote.ToString();
            await _db.SaveChangesAsync();

            return guildSettings;
        }
    }
}
