FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=1.0.0
WORKDIR /webapi
COPY ["webapi.csproj", "."]
RUN dotnet restore "webapi.csproj"
COPY . .
RUN sed -i "s/VERSION_TO_REPLACE/$APP_VERSION/g" appsettings.json
WORKDIR "/webapi"
RUN dotnet build "webapi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "webapi.dll"]
