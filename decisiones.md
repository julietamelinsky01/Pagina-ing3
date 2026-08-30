# Decisiones técnicas

Este archivo reúne las decisiones tomadas en los trabajos prácticos de Ingeniería de Software 3.

## TP1 — Git colaborativo

### 1. Por qué Git no pudo resolver el conflicto solo

Git no pudo resolver el conflicto automáticamente porque las dos ramas habían modificado la misma línea del README de maneras diferentes: la rama A cambió el título a "versión A" y la rama B a "versión B".

Git detectó que existían dos cambios incompatibles sobre la misma línea, pero no podía determinar cuál de las dos versiones era la correcta. Por ese motivo fue necesario resolver el conflicto manualmente, eligiendo el contenido que debía quedar.

El conflicto se podría haber evitado si las ramas hubieran modificado partes distintas del archivo o si se hubiera integrado una de las ramas antes de realizar el cambio conflictivo en la otra.

### 2. Problemas encontrados y cómo los solucioné

- **"Require approvals" activado por defecto:** al configurar la protección de `main`, GitHub solicitaba una aprobación para poder mergear. Como el TP era individual, no podía aprobar mi propio Pull Request. Se resolvió desactivando ese requisito y manteniendo la obligación de ingresar los cambios mediante Pull Request.

- **Nombres automáticos de ramas:** al crear algunas ramas desde la interfaz web de GitHub se generaron nombres automáticos, en lugar de la convención `feature/...` sugerida. Verifiqué que esto no afectaba el funcionamiento del flujo de trabajo.

- **Terminal que parecía trabada:** al pegar varios comandos juntos, en algunos casos la terminal quedaba esperando. Lo solucioné cancelando con `Ctrl+C` y ejecutando los comandos individualmente.

- **Conflicto intencional entre ramas:** dos ramas modificaron la misma línea del README. GitHub detectó el conflicto y bloqueó el merge hasta que fue resuelto manualmente.

### 3. Declaración de uso de IA

Utilicé Claude (Anthropic) como asistente durante el desarrollo del TP1 para guiarme paso a paso en tareas como la configuración de la protección de rama, la creación de Pull Requests, la generación del conflicto, su resolución y la creación del tag y la release.

Las indicaciones fueron verificadas durante el trabajo práctico ejecutando los comandos y comprobando sus resultados en Git y GitHub. Las evidencias del push rechazado, el conflicto, los marcadores de conflicto y la release publicada quedaron registradas en `evidencias.md`.

## TP2 — Contenedores

### Qué app y por qué

La app es la misma descripta arriba (Las Melis: .NET 8 + React/Vite + PostgreSQL), elegida contra los
criterios del §3.3 de la guía: buildea y corre local sin magia (probada antes de comprometerse),
tiene superficie para llegar a los 8+4 tests del TP5 (ver "Reglas de negocio" arriba), es un CRUD +
calendario acotado (3 pantallas principales) y es un dominio que entiendo lo suficiente como para
modificarlo en vivo en el Integrador. Vive en este mismo repo (el del TP1, con sus protecciones), no
en uno nuevo.

### Decisiones de contenerización

**Imágenes base — multi-stage en los dos servicios:**

- Backend: build con `mcr.microsoft.com/dotnet/sdk:8.0` (1.25 GB, tiene compilador y herramientas) →
  runtime final con `mcr.microsoft.com/dotnet/aspnet:8.0` (350 MB, solo el runtime de ASP.NET). La
  imagen final (`364 MB`) no lleva el SDK: menos peso, menos superficie de ataque.
- Frontend: build con `node:22-alpine` (228 MB, para correr `npm ci` + `npm run build`) → runtime
  final con `nginx:alpine` (92.7 MB) sirviendo solo el `dist/` estático. La imagen final pesa
  `93.5 MB` — no lleva Node ni `node_modules` a producción, solo HTML/JS/CSS compilado.

Comparación real de tamaños y la corrida completa en [evidencias.md](evidencias.md).

**Frontend: URL absoluta + CORS, no proxy relativo en nginx.** El TP2 (§2.6) da dos caminos para que
el frontend contenerizado hable con el backend: (a) rutas relativas `/api` con un proxy en nginx, o
(b) URL absoluta al puerto publicado del backend + CORS. Ya existía código con el camino (b) desde
antes de dockerizar (`VITE_API_URL` en `frontend/src/api/client.js`, consumida por Axios, más
`AddCors`/`UseCors` en `Program.cs`), así que se mantuvo esa decisión en vez de reescribirla al
camino (a):

- `VITE_API_URL` se pasa como build arg al `frontend/Dockerfile` (Vite resuelve las env vars en
  **build time**, no en runtime) apuntando a `http://localhost:8080/api`, el puerto que
  `docker-compose.yml` publica del backend.
- El backend habilita CORS para `http://localhost:3000` (variable `Frontend__Origin`), el puerto
  publicado del frontend.
- `frontend/nginx.conf` no tiene bloque `location /api/`: no hace falta, todas las llamadas salen
  directo del browser al backend. Sí tiene el `try_files` para el fallback de `react-router`
  (`BrowserRouter`), necesario sea cual sea el camino elegido para las rutas del cliente.

El costo del camino (b), tal como advierte la guía, es que la URL del backend queda fija en la imagen
del frontend: cambiar de entorno implica rebuildear con otro `VITE_API_URL`. Se acepta ese costo
porque es consistente con lo que ya tenía la app.

**Qué persiste y qué no.** El único estado real del sistema es la base de datos, montada en el
volumen nombrado `db_data:/var/lib/postgresql/data` — sobrevive a `docker compose down` y a que se
recree el contenedor de `db`. Todo lo demás es descartable: la capa de escritura de los contenedores
de `backend` y `frontend` (logs, claves de Data Protection de ASP.NET, cachés), y por supuesto las
etapas de build (SDK, `node_modules`, código fuente copiado) que ni siquiera llegan a la imagen
final. `docker compose down -v` borra el volumen a propósito, para poder probar ese límite.

**Secretos.** `DB_PASSWORD` y `JWT_KEY` viven en `.env` (raíz, gitignored), con `.env.example`
commiteado como plantilla. `docker-compose.yml` los referencia como `${DB_PASSWORD}`/`${JWT_KEY}` —
nunca están hardcodeados en el YAML ni en los Dockerfiles.

### Problemas encontrados y cómo se resolvieron

- **Puerto del backend en contenedor vs. desarrollo local.** En desarrollo el backend corre en
  `:5091`/`:5080` (`launchSettings.json`); la imagen `aspnet:8.0` escucha por default en `:8080`. Se
  resolvió dejando que el contenedor use el default de la imagen (`EXPOSE 8080`, sin pisar
  `ASPNETCORE_URLS`) y publicando `8080:8080` en el compose — no hubo que tocar código, solo ser
  consistente entre `Dockerfile`, `docker-compose.yml` y el build arg `VITE_API_URL` del frontend.
- **Prueba de persistencia con falsos positivos.** La primera vez que probé `docker compose down -v`,
  los `TiposTurno` seguían apareciendo — parecía que el volumen no se había borrado. La causa real es
  que esos datos están cargados vía `HasData` en la migración (ver "Autenticación" arriba: mismo
  mecanismo que el usuario admin), así que reaparecen en cualquier base nueva. Se corrigió la prueba
  creando un `Empleado` real por API antes de cada corrida: ese sí desaparece con `-v` y confirma que
  el volumen es lo que sostiene el estado, no la migración.
- **`ConnectionStrings__DefaultConnection` con host equivocado.** Al correr el backend suelto (sin
  compose, `docker run` directo) contra el Postgres del host, `Host=localhost` del `appsettings.json`
  apunta al contenedor mismo, no a la máquina. Se resolvió pasando la connection string completa por
  variable de entorno en el `docker run` (en macOS/Windows con `host.docker.internal`; en Linux hace
  falta además `--add-host=host.docker.internal:host-gateway`). Dentro del compose no aplica: ahí el
  host es `db`, el nombre del servicio.

### Uso de IA

Se usó Claude Code (Anthropic) como asistente para este TP2: generar la primera versión de los
Dockerfiles multi-stage, `docker-compose.yml`, `docker-compose.registry.yml` y `nginx.conf`, y para
redactar este apartado y `evidencias.md`. La verificación no fue "correr y confiar": se levantó el
stack completo con `docker compose up -d --build`, se probó login + JWT + CORS end-to-end contra la
API real, y se corrió la prueba de persistencia dos veces (`down` sin `-v` y con `-v`) creando un
empleado real para descartar falsos positivos por datos de seed — todo documentado con salidas reales
en `evidencias.md`, no simuladas. Lo que no fue asistido por IA es todo lo anterior a este TP: el
dominio, las reglas de negocio, la elección de stack y la app en sí.

