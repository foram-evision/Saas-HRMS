FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["HRMS.API/HRMS.API.csproj", "HRMS.API/"]
COPY ["HRMS.Application/HRMS.Application.csproj", "HRMS.Application/"]
COPY ["HRMS.Domain/HRMS.Domain.csproj", "HRMS.Domain/"]
COPY ["HRMS.Infrastructure/HRMS.Infrastructure.csproj", "HRMS.Infrastructure/"]

RUN dotnet restore "HRMS.API/HRMS.API.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/HRMS.API"
RUN dotnet build "HRMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HRMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HRMS.API.dll"]
