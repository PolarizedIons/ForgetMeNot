using System.Threading.Tasks;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace ForgetMeNot.Core.Services
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

        public async Task<GuildSettings> SetSaveReaction(ulong guildId, string emote)
        {
            var guildSettings = await GetGuildSettings(guildId);
            guildSettings.SaveReaction = emote;
            await _db.SaveChangesAsync();

            return guildSettings;
        }

        public async Task<bool> IsUsingLocalQuotes(ulong guildId)
        {
            var guildSettings = await GetGuildSettings(guildId);
            return guildSettings.LocalQuotes == true;
        }

        public async Task<GuildSettings> SetUsingLocalQuotes(ulong guildId, bool local)
        {
            var guildSettings = await GetGuildSettings(guildId);
            guildSettings.LocalQuotes = local;
            await _db.SaveChangesAsync();

            return guildSettings;
        }
    }
}
