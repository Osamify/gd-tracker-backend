# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy ALL the csproj files first (this makes building much faster)
COPY GdTrackerApi/*.csproj ./GdTrackerApi/
COPY GdTracker.Core/*.csproj ./GdTracker.Core/
COPY GdTracker.Infra/*.csproj ./GdTracker.Infra/

# 2. Restore dependencies for the whole solution
RUN dotnet restore "GdTrackerApi/GdTrackerApi.csproj"

# 3. Copy everything else
COPY . .

# 4. Build and publish
WORKDIR /src/GdTrackerApi
RUN dotnet publish "GdTrackerApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Use the ASP.NET runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 6. Render assigns a random PORT.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GdTrackerApi.dll"]