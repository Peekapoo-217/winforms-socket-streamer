FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["NetworkCore/NetworkCore.csproj", "NetworkCore/"]
COPY ["SharpView.Relay/SharpView.Relay.csproj", "SharpView.Relay/"]
RUN dotnet restore "SharpView.Relay/SharpView.Relay.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SharpView.Relay"
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 9000
ENTRYPOINT ["dotnet", "SharpView.Relay.dll"]
