FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/AutoResearchClaw.Api/AutoResearchClaw.Api.csproj ./AutoResearchClaw.Api/
COPY src/AutoResearchClaw.Application/AutoResearchClaw.Application.csproj ./AutoResearchClaw.Application/
COPY src/AutoResearchClaw.Domain/AutoResearchClaw.Domain.csproj ./AutoResearchClaw.Domain/
COPY src/AutoResearchClaw.Infrastructure/AutoResearchClaw.Infrastructure.csproj ./AutoResearchClaw.Infrastructure/

RUN dotnet restore AutoResearchClaw.Api/AutoResearchClaw.Api.csproj

COPY src/ .
RUN dotnet build AutoResearchClaw.Api/AutoResearchClaw.Api.csproj -c Release -o /app/build
RUN dotnet publish AutoResearchClaw.Api/AutoResearchClaw.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AutoResearchClaw.Api.dll"]