FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy csproj files and restore dependencies
COPY Antigravity.Domain/Antigravity.Domain.csproj Antigravity.Domain/
COPY Antigravity.Application/Antigravity.Application.csproj Antigravity.Application/
COPY Antigravity.Infrastructure/Antigravity.Infrastructure.csproj Antigravity.Infrastructure/
COPY Antigravity.WebApi/Antigravity.WebApi.csproj Antigravity.WebApi/
COPY Antigravity.Backend.slnx ./

RUN dotnet restore

# Copy all source files and publish release
COPY . ./
RUN dotnet publish Antigravity.WebApi/Antigravity.WebApi.csproj -c Release -o out

# Build runtime container
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "Antigravity.WebApi.dll"]
