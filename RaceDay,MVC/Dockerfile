
# Use the .NET 8 SDK to build the MVC application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container.
WORKDIR /src

# Copy the MVC project files.
COPY ["RaceDay.MVC.csproj", "./"]

# Restore the required NuGet packages.
RUN dotnet restore "RaceDay.MVC.csproj"

# Copy the remaining MVC project files.
COPY . .

# Build and publish the application.
RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the smaller ASP.NET runtime image to run the application.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Copy the published application from the build stage.
COPY --from=build /app/publish .

# The MVC application will listen on port 8080.
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
