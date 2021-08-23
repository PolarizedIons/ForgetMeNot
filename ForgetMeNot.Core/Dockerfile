FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ForgetMeNot.Core/ForgetMeNot.Core.csproj", "ForgetMeNot.Core/"]
RUN dotnet restore "ForgetMeNot.Core/ForgetMeNot.Core.csproj"
COPY . .
WORKDIR "/src/ForgetMeNot.Core"
RUN dotnet build "ForgetMeNot.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForgetMeNot.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForgetMeNot.Core.dll"]
