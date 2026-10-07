# ---- Etapa 1: build (imagen con SDK) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Se copian primero la solución y los .csproj (de la app y de los tests) para que el
# restore quede en una capa cacheable: solo se rehace si cambian las dependencias.
# El proyecto de tests tiene que estar acá, o la etapa `test` no lo encuentra.
COPY LasMelis.sln .
COPY LasMelis.Api/LasMelis.Api.csproj LasMelis.Api/
COPY LasMelis.Tests/LasMelis.Tests.csproj LasMelis.Tests/
RUN dotnet restore LasMelis.sln

COPY . .
RUN dotnet publish LasMelis.Api/LasMelis.Api.csproj -c Release -o /app/publish

# ---- Etapa 2: tests (parte del build, con el SDK adentro) ----
# Los tests corren en `docker run`, no al construir: el pipeline los ejecuta y lee /out.
#   /p:Threshold=65 + ThresholdType=line%2cbranch -> si la cobertura (línea O rama) baja de 65, sale con error.
#   /p:Exclude -> qué NO entra en la cuenta: el arranque (Program), el DbContext (Data),
#                 las clases de datos (Models) y las migraciones (código generado por EF).
#   La coma va escrita %2c porque MSBuild parte los valores por coma.
FROM build AS test
ENTRYPOINT ["dotnet", "test", "LasMelis.sln", \
            "/p:CollectCoverage=true", \
            "/p:Threshold=65", \
            "/p:ThresholdType=line%2cbranch", \
            "/p:Exclude=[LasMelis.Api]Program*%2c[LasMelis.Api]LasMelis.Api.Data.*%2c[LasMelis.Api]LasMelis.Api.Models.*%2c[LasMelis.Api]LasMelis.Api.Migrations.*", \
            "--logger", "trx;LogFileName=tests.trx", \
            "--results-directory", "/out"]

# ---- Etapa 3: runtime (imagen mínima, sin SDK) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LasMelis.Api.dll"]
