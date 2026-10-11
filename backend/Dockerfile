# Compilación aislada de la API.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LenguajesFormalesAPI.csproj", "./"]
RUN dotnet restore "LenguajesFormalesAPI.csproj"

COPY . .
RUN dotnet publish "LenguajesFormalesAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final mínima, sin SDK ni código fuente.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN mkdir -p /app/uploads && chown -R app:app /app
COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "LenguajesFormalesAPI.dll"]
