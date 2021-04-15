FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ForgetMeNot/ForgetMeNot.csproj", "ForgetMeNot/"]
RUN dotnet restore "ForgetMeNot/ForgetMeNot.csproj"
COPY . .
WORKDIR "/src/ForgetMeNot"
RUN dotnet build "ForgetMeNot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForgetMeNot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForgetMeNot.dll"]
