FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and all project files first (for layer caching)
COPY Ecommerce.Automation.sln .
COPY Ecommerce.Domain/Ecommerce.Domain.csproj Ecommerce.Domain/
COPY Ecommerce.Application/Ecommerce.Application.csproj Ecommerce.Application/
COPY Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj Ecommerce.Infrastructure/
COPY Ecommerce.Persistence/Ecommerce.Persistence.csproj Ecommerce.Persistence/
COPY Ecommerce.API/Ecommerce.API.csproj Ecommerce.API/

# Restore all packages
RUN dotnet restore Ecommerce.Automation.sln

# Copy all source code
COPY . .

# Publish API project
RUN dotnet publish Ecommerce.API/Ecommerce.API.csproj -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]
