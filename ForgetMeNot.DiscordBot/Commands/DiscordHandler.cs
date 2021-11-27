using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace ForgetMeNot.DiscordBot.Commands
{
    public class DiscordHandler : ISingletonDiService
    {
        private readonly Random _random = new();
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interaction;
        private readonly ulong _debugGuild;
        private readonly IServiceProvider _services;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly QuoteService _quoteService;

        public DiscordHandler(
            DiscordSocketClient discord,
            InteractionService interaction,
            IConfiguration config,
            IServiceProvider services,
            IServiceScopeFactory scopeFactory,
            QuoteService quoteService
        )
        {
            _discord = discord;
            _interaction = interaction;
            _services = services;
            _debugGuild = ulong.Parse(config["Bot:DebugGuild"]);
            _scopeFactory = scopeFactory;
            _quoteService = quoteService;
        }

        public async Task InitializeAsync()
        {
            _interaction.AddGenericTypeConverter<IEmote>(typeof(EmoteTypeConverter<>));
            await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _discord.Ready += DiscordOnReady;
            _discord.ReactionAdded += DiscordOnReactionAdded;
            _discord.SlashCommandExecuted += ExecuteInteraction;
            _discord.MessageCommandExecuted += ExecuteInteraction;
            _interaction.SlashCommandExecuted += InteractionCommandExecuted;
            _interaction.ContextCommandExecuted += InteractionCommandExecuted;
        }

        private async Task ExecuteInteraction(SocketInteraction interaction)
        {
            await interaction.DeferAsync();
            var ctx = new SocketInteractionContext(_discord, interaction);
            await WithMessageContext(interaction, () => _interaction.ExecuteCommandAsync(ctx, _services));
        }

        private async Task InteractionCommandExecuted(IApplicationCommandInfo command, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                await context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.Embed = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithTitle("An error has occured")
                        .WithDescription(result.ErrorReason)
                        .Build();
                });
            }
        }

        private async Task DiscordOnReady()
        {
            if (Debugger.IsAttached)
            {
                Log.Debug("Registering commands to debug guild!");
                await _interaction.RegisterCommandsToGuildAsync(_debugGuild);
            }
            else
            {
                Log.Debug("Registering commands...");
                await _interaction.RegisterCommandsGloballyAsync();
            }

            Log.Information("Discord is Ready!");
            await _discord.SetStatusAsync(UserStatus.Online);
        }

        private async Task DiscordOnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            if (!reaction.User.IsSpecified || !(reaction.User.Value is IGuildUser user))
            {
                return;
            }

            if (!user.GuildPermissions.Has(GuildPermission.ManageMessages))
            {
                return;
            }

            await WithMessageContext(message, async () =>
            {
                await HandleReactionAsync(message, reaction);
            });
        }

        private async Task HandleReactionAsync(IUserMessage msg, IReaction reaction)
        {
            Log.Debug("Got a reaction! {Emote}", reaction.Emote);

            if (msg.Author.IsBot || msg.Channel is not IGuildChannel channel)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var guildSettingsService = scope.ServiceProvider.GetRequiredService<GuildSettingsService>();
            if (!await guildSettingsService.IsSaveReaction(channel.GuildId, reaction.Emote))
            {
                return;
            }

            var response = await _quoteService.RememberQuote(msg);
            var jumpUrl = $"https://discord.com/channels/{(channel).GuildId}/{channel.Id}/{msg.Id}";
            switch (response)
            {
                case HandlerResponseCode.QuoteAlreadySaved:
                    await msg.Channel.SendMessageAsync(
                        embed: new EmbedBuilder()
                            .WithTitle("I already know that quote!")
                            .WithDescription($"[Jump to Message]({jumpUrl})")
                            .WithColor(Color.Orange)
                            .Build(),
                        allowedMentions: AllowedMentions.None
                    );
                    break;
                case HandlerResponseCode.Success:
                {
                    var easterEgg = _random.Next(100) == 0;
                    var text = easterEgg ? $"{_discord.CurrentUser.Username} will remember this...." : "Quote saved!";
                    await msg.Channel.SendMessageAsync(
                        embed: new EmbedBuilder()
                            .WithTitle(text)
                            .WithDescription($"[Jump to Message]({jumpUrl})")
                            .WithColor(Color.Blue)
                            .Build(),
                        allowedMentions: AllowedMentions.None
                    );
                    break;
                }
                default:
                    await msg.Channel.SendMessageAsync(
                        embed: new EmbedBuilder()
                            .WithTitle("Sorry, something went wrong!")
                            .WithColor(Color.Red)
                            .Build(),
                        allowedMentions: AllowedMentions.None
                    );
                    break;
            }
        }

        private static async Task WithMessageContext(IMessage message, Func<Task> action)
        {
            using (LogContext.PushProperty("Author", message.Author))
            {
                using (LogContext.PushProperty("AuthorId", message.Author.Id.ToString()))
                {
                    using (LogContext.PushProperty("Message", message.Content))
                    {
                        using (LogContext.PushProperty("MessageId", message.Id.ToString()))
                        {
                            using (LogContext.PushProperty("Channel", message.Channel.Name))
                            {
                                using (LogContext.PushProperty("ChannelId", message.Channel.Id.ToString()))
                                {
                                    using (LogContext.PushProperty("Guild", (message.Channel as IGuildChannel)?.Guild.Name))
                                    {
                                        using (LogContext.PushProperty("GuildId", (message.Channel as IGuildChannel)?.Guild.Id.ToString()))
                                        {
                                            await action.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task WithMessageContext(SocketInteraction slashCommand, Func<Task> action)
        {
            using (LogContext.PushProperty("Author", slashCommand.User.ToString()))
            {
                using (LogContext.PushProperty("AuthorId", slashCommand.User.Id.ToString()))
                {
                    using (LogContext.PushProperty("Channel", slashCommand.Channel.Name))
                    {
                        using (LogContext.PushProperty("ChannelId", slashCommand.Channel.Id.ToString()))
                        {
                            using (LogContext.PushProperty("Guild", (slashCommand.Channel as IGuildChannel)?.Guild.Name))
                            {
                                using (LogContext.PushProperty("GuildId", (slashCommand.Channel as IGuildChannel)?.Guild.Id.ToString()))
                                {
                                    await action.Invoke();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
