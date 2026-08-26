# ---- Etapa 1: build (imagen con SDK) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Se copia el .csproj antes que el resto del código para que el restore
# quede en una capa cacheable: solo se rehace si cambian las dependencias.
COPY LasMelis.Api/LasMelis.Api.csproj LasMelis.Api/
RUN dotnet restore LasMelis.Api/LasMelis.Api.csproj

COPY . .
RUN dotnet publish LasMelis.Api/LasMelis.Api.csproj -c Release -o /app/publish

# ---- Etapa 2: runtime (imagen mínima, sin SDK) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LasMelis.Api.dll"]
