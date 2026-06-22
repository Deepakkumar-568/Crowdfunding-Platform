FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy csproj files and restore dependencies
COPY Crowdfunding.Domain/Crowdfunding.Domain.csproj Crowdfunding.Domain/
COPY Crowdfunding.Application/Crowdfunding.Application.csproj Crowdfunding.Application/
COPY Crowdfunding.Infrastructure/Crowdfunding.Infrastructure.csproj Crowdfunding.Infrastructure/
COPY Crowdfunding.WebApi/Crowdfunding.WebApi.csproj Crowdfunding.WebApi/
COPY Crowdfunding.Backend.slnx ./

RUN dotnet restore

# Copy all source files and publish release
COPY . ./
RUN dotnet publish Crowdfunding.WebApi/Crowdfunding.WebApi.csproj -c Release -o out

# Build runtime container
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "Crowdfunding.WebApi.dll"]
