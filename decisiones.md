# Decisiones técnicas

Este proyecto es la base de la materia durante todo el semestre (TP2 en adelante), así que estas
decisiones están tomadas pensando en esa continuidad, no solo en que el CRUD funcione hoy.

## Base de datos: PostgreSQL (no SQL Server)

El spec original dejaba esto "a definir", con SQL Server como opción por defecto. Se descartó porque:

- SQL Server no corre nativamente en macOS (haría falta Docker + una imagen especial tipo
  `azure-sql-edge`), mientras que Postgres se levanta en minutos con un `docker run` simple o con
  Homebrew.
- Es el motor que mejor cumple el criterio de la cátedra "que puedan ejecutarla hoy" y "sin
  dependencias exóticas".
- EF Core lo soporta igual de bien vía `Npgsql.EntityFrameworkCore.PostgreSQL`, sin ninguna
  desventaja funcional para este proyecto.

## UI library: MUI

Se evaluó contra Bootstrap (react-bootstrap). Se eligió MUI porque sus componentes de tabla, formularios
y diálogos modales encajan directo con las pantallas de CRUD y el calendario semanal, sin tener que
armar mucho a mano.

## Autenticación: tabla `Usuario` en base de datos

Se evaluó contra una credencial fija en `appsettings.json`. Se eligió la tabla en base de datos
(username + hash BCrypt) porque:

- Es más representativo de un sistema real con JWT (el login valida contra un registro persistido,
  no una constante).
- El hash se generó una sola vez con `BCrypt.Net.BCrypt.HashPassword` y quedó fijo en el seed de la
  migración (`HasData`), para que la migración sea reproducible — no se genera un hash nuevo (con
  salt distinto) cada vez que EF recalcula el modelo.

## Connection string parametrizable por variable de entorno

`appsettings.json` solo tiene un valor de desarrollo. `Program.cs` lo lee vía
`builder.Configuration.GetConnectionString(...)`, que ASP.NET Core resuelve por la convención
`ConnectionStrings__DefaultConnection` como variable de entorno — sin tocar código ni el archivo de
configuración. Esto es intencional pensando en el TP2 (la base pasa a vivir en un contenedor, cambia
el host) y el TP6 (la misma app apunta a bases distintas para QA y producción). Lo mismo aplica a la
clave JWT (`Jwt__Key`) y al origen permitido por CORS (`Frontend__Origin`).

## Reglas de negocio agregadas más allá del CRUD

El spec funcional original (gestión de empleados y turnos) es básicamente CRUD puro. La guía de la
cátedra pide margen para llegar a 8 tests de backend y 4 de frontend en el TP5, lo que requiere unas
4-6 reglas de negocio reales (no solo altas/bajas/modificaciones). Se agregaron ahora, en el TP2/TP3,
para no llegar al TP5 sin nada que testear:

**Backend** (`Services/EmpleadoService.cs`, `Services/AsignacionTurnoService.cs`,
`Services/TipoTurnoService.cs`, `Services/TurnoHorasCalculator.cs`):

1. **DNI único por empleado** — validación + índice único en la base.
2. **No duplicar asignación** (mismo empleado + tipo de turno + fecha) — índice único + chequeo
   explícito antes del insert, con mensaje de error claro.
3. **No asignar turnos a empleados inactivos** — restricción de negocio.
4. **Cálculo de horas por turno con turnos que cruzan la medianoche** (ej. Noche 22:00–06:00 = 8hs,
   no un número negativo) — caso borde real en `TurnoHorasCalculator.CalcularHoras`.
5. **La fecha de ingreso de un empleado no puede ser futura** — validación.
6. **Dar de baja a un empleado con asignaciones futuras** se permite, pero el service cuenta cuántas
   tiene y lo informa en la respuesta (transición de estado con efecto colateral verificable).
7. **No se puede eliminar un tipo de turno con asignaciones asociadas** — restricción de integridad,
   surge naturalmente de permitir el CRUD completo de `TipoTurno`.

**Frontend** (`pages/EmpleadoForm.jsx`, `pages/AsignacionForm.jsx`, `pages/ReporteSemanal.jsx`):

1. El formulario de empleado deshabilita "Guardar" si faltan campos requeridos o el DNI no matchea
   `^\d{7,8}$` — validación antes de habilitar el submit, no solo al recibir el error del backend.
2. El formulario de nueva asignación en el calendario chequea contra las asignaciones ya cargadas de
   la semana visible y avisa/bloquea si la combinación empleado+turno+fecha ya existe, antes de
   pegarle a la API.
3. El reporte semanal recalcula el total de horas por empleado en el cliente (`useMemo` sobre las
   asignaciones cargadas) cada vez que cambia el rango de fechas seleccionado — no es un valor
   estático que devuelve el backend.

## Migraciones automáticas al arrancar

`Program.cs` corre `db.Database.Migrate()` al iniciar la aplicación, para que `dotnet run` funcione
de punta a punta sin pasos manuales adicionales la primera vez que alguien clona el repo. Es una
concesión pensada para friction-less local dev en un proyecto académico; en un pipeline de CI/CD real
(TP6) esto normalmente se separaría en un paso de deploy explícito.

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
