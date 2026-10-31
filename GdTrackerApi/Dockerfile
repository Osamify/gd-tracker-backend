# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["GdTrackerApi.csproj", "./"]
RUN dotnet restore "GdTrackerApi.csproj"

# Copy everything else and build the release version
COPY . .
RUN dotnet publish "GdTrackerApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET runtime image for the final image
# 👇 CHANGED THIS TO 10.0 AS WELL
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render assigns a random PORT. The C# app will read this.
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "GdTrackerApi.dll"]