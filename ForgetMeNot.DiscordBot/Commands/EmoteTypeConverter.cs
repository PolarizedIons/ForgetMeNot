using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace ForgetMeNot.DiscordBot.Commands
{
    public class EmoteTypeConverter<T> : TypeConverter<T> where T : IEmote
    {
        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var input = option.Value as string;
            if (Emote.TryParse(input, out var emote))
            {
                return Task.FromResult(TypeConverterResult.FromSuccess(emote));
            }

            if (NeoSmart.Unicode.Emoji.IsEmoji(input))
            {
                return Task.FromResult(TypeConverterResult.FromSuccess(new Emoji(input)));
            }

            // Doesn't seem to throw anything/tell me not success...
            // return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.Unsuccessful, "That is not an emote."));
            return Task.FromResult(TypeConverterResult.FromSuccess(null));
        }
    }
}
