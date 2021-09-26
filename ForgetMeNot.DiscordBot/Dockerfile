FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ForgetMeNot.Common/ForgetMeNot.Common.csproj", "ForgetMeNot.Common/"]
COPY ["ForgetMeNot.DiscordBot/ForgetMeNot.DiscordBot.csproj", "ForgetMeNot.DiscordBot/"]
RUN dotnet restore "ForgetMeNot.DiscordBot/ForgetMeNot.DiscordBot.csproj"
COPY . .
WORKDIR "/src/ForgetMeNot.DiscordBot"
RUN dotnet build "ForgetMeNot.DiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForgetMeNot.DiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForgetMeNot.DiscordBot.dll"]
