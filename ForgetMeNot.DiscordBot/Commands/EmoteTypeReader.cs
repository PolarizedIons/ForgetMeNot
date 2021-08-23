using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ForgetMeNot.DiscordBot.Commands
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Emote.TryParse(input, out var emote))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emote));
            }

            if (NeoSmart.Unicode.Emoji.IsEmoji(input))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji(input)));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "That is not an emote."));
        }
    }
}
