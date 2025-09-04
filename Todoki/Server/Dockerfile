# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as separate layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose default ASP.NET port
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Start the application
ENTRYPOINT ["dotnet", "Todoki.Server.dll"]
