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
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealTimeChat.dll"]