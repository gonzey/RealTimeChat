# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RealTimeChat.csproj", "./"]
RUN dotnet restore "RealTimeChat.csproj"
COPY . .
RUN dotnet build "RealTimeChat.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RealTimeChat.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install the remote debugger
RUN apt-get update \
    && apt-get install -y unzip curl \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealTimeChat.dll"]
