using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using ForgetMeNot.DiscordBot.Services;

namespace ForgetMeNot.DiscordBot.Commands
{
    [Group("settings", "Settings for ForgetMeNotBot")]
    public class SettingsCommands : InteractionModuleBase
    {
        private readonly GuildSettingsService _guildSettingsService;

        public SettingsCommands(GuildSettingsService guildSettingsService)
        {
            _guildSettingsService = guildSettingsService;
        }

        [SlashCommand("set-emote", "Set the reaction emote")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task SetEmote(IEmote emote)
        {
            if (emote == null)
            {
                await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.AllowedMentions = AllowedMentions.None;
                    properties.Embed = new EmbedBuilder()
                        .WithTitle("Remember emote not changed")
                        .WithDescription("That's not an emote!")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build();
                });
                
                return;
            }
            
            var guild = ((IGuildChannel)Context.Channel).Guild;
            await _guildSettingsService.SetSaveReaction(guild.Id, emote);
            await Context.Interaction.ModifyOriginalResponseAsync(properties =>
            {
                properties.AllowedMentions = AllowedMentions.None;
                properties.Embed = new EmbedBuilder()
                    .WithTitle("Remember emote changed")
                    .WithDescription($"Got it! {emote} is your new remember-reaction")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .Build();
            });
        }

        [SlashCommand("local-quotes", "Set quotes to be local to channels within the guild")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task LocalQuotes()
        {
            var guild = ((IGuildChannel)Context.Channel).Guild;
            await _guildSettingsService.SetUsingLocalQuotes(guild.Id, true);

            await Context.Interaction.ModifyOriginalResponseAsync(properties =>
            {
                properties.AllowedMentions = AllowedMentions.None;
                properties.Embed = new EmbedBuilder()
                    .WithTitle("Quotes are local")
                    .WithDescription($"Got it! Your quotes are now channel-based")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .Build();
            });
        }

        [SlashCommand("global-quotes", "Set quotes to be global across the entire guild")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task GlobalQuotes()
        {
            var guild = ((IGuildChannel)Context.Channel).Guild;
            await _guildSettingsService.SetUsingLocalQuotes(guild.Id, false);

            await Context.Interaction.ModifyOriginalResponseAsync(properties =>
            {
                properties.AllowedMentions = AllowedMentions.None;
                properties.Embed = new EmbedBuilder()
                    .WithTitle("Quotes are global")
                    .WithDescription($"Got it! Your quotes are now server-based")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .Build();
            });
        }
    }
}
