using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ForgetMeNot.Extentions;
using ForgetMeNot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace ForgetMeNot.Commands
{
    public class DiscordHandler : ISingletonDiService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly string _botPrefix;
        private readonly IServiceProvider _services;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly QuoteService _quoteService;

        public DiscordHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfiguration config,
            IServiceProvider services,
            IServiceScopeFactory scopeFactory,
            QuoteService quoteService
        )
        {
            _discord = discord;
            _commands = commands;
            _services = services;
            _botPrefix = config["Bot:Prefix"];
            _scopeFactory = scopeFactory;
            _quoteService = quoteService;
        }

        public async Task InitializeAsync()
        {
            _commands.AddTypeReader(typeof(IEmote), new EmoteTypeReader());

            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services
            );

            _discord.Ready += DiscordOnReady;
            _discord.MessageReceived += DiscordOnMessageReceived;
            _discord.ReactionAdded += DiscordOnReactionAdded;
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private Task DiscordOnReady()
        {
            Log.Information("Discord is Ready!");
            return Task.CompletedTask;
        }

        private async Task DiscordOnMessageReceived(SocketMessage message)
        {
            await WithMessageContext(message, async () =>
            {
                await HandleCommandAsync(message);
            });
        }

        private async Task DiscordOnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            if (!(message.Author is IGuildUser user))
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

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }

            var argPos = 0;
            if (!msg.HasStringPrefix(_botPrefix, ref argPos))
            {
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = new CommandContext(_discord, msg);
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: scope.ServiceProvider
                );
            }
        }

        private async Task HandleReactionAsync(IUserMessage msg, SocketReaction reaction)
        {
            Log.Debug("Got a reaction! {Emote}", reaction.Emote);

            if (msg.Author.IsBot)
            {
                return;
            }

            if (!(msg.Channel is IGuildChannel channel))
            {
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var guildSettingsService = scope.ServiceProvider.GetRequiredService<GuildSettingsService>();
                if (!await guildSettingsService.IsSaveReaction(channel.GuildId, reaction.Emote))
                {
                    return;
                }

                await _quoteService.RememberQuote(msg);
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We don't care about unknown commands
            if (result.Error == CommandError.UnknownCommand)
            {
                return;
            }

            if (!result.IsSuccess && !string.IsNullOrEmpty(result.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                Log.Error("Error {ErrorType}: '{CommandName}', {ExceptionMessage}", result.Error, command.Value?.Name, result.ErrorReason);
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
    }
}
