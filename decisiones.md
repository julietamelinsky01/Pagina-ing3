# Decisiones técnicas

Este archivo reúne las decisiones tomadas en los trabajos prácticos de Ingeniería de Software 3.

## Enlaces del TP7

**Paquetes públicos en ghcr.io**, etiquetados con el commit de la release `v7.0.0` (`db433c2`); el `docker pull` anda sin credenciales:

- Backend: https://github.com/users/julietamelinsky01/packages/container/package/pagina-ing3-backend
  `docker pull --platform linux/amd64 ghcr.io/julietamelinsky01/pagina-ing3-backend:sha-db433c2caf27958503963c1251e7c1c45e350819`
- Frontend: https://github.com/users/julietamelinsky01/packages/container/package/pagina-ing3-frontend
  `docker pull --platform linux/amd64 ghcr.io/julietamelinsky01/pagina-ing3-frontend:sha-db433c2caf27958503963c1251e7c1c45e350819`

**La corrida donde la integración quedó VERDE y la e2e ROJA** (frenó la promoción):

- Commit que rompió la app (renombra el botón «Nuevo empleado» a «Agregar empleado» en `frontend/src/pages/Empleados.jsx`; no toca `e2e/`): `b3fac50` (PR #32).
- Corrida #42: https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39467270450 — `integracion` verde, `e2e` roja, `deploy-prod` sin arrancar.
- Los dos artefactos de reporte de esa corrida están al pie de la página de la corrida: `playwright-report-integracion` (verde) y `playwright-report-e2e` (rojo, con captura y traza del fallo).

**La corrida completa en verde**, hasta PROD:

- Tras el arreglo (PR #33, commit `db433c2`), corrida #44: https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39470073157 — builds, `deploy-qa`, `integracion`, `e2e` y `deploy-prod` en verde, con mi aprobación registrada.
- Primera corrida completa con las dos suites (todo verde, antes de romper nada): corrida #40, https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39323047184

**Release:** https://github.com/julietamelinsky01/Pagina-ing3/releases/tag/v7.0.0

**Entornos vivos** (los mismos del TP6): QA https://pagina-ing3-front-qa.onrender.com · PROD https://pagina-ing3-front-prod.onrender.com

Los *Events* de Render (donde se lee «Triggered via Deploy Hook» con el `sha-<commit>` de cada deploy) no se pueden enlazar: los muestro en vivo en la defensa.

Mi app tiene **dos Dockerfiles** (backend y frontend), así que son **dos paquetes** y **cuatro servicios** en Render (api y front, en QA y en PROD).

## Enlaces de este TP (TP6)

**Paquetes públicos en ghcr.io** (etiquetados con el commit del merge `be64dbb`; el `docker pull` anda sin credenciales):

- Backend: https://github.com/users/julietamelinsky01/packages/container/package/pagina-ing3-backend
  `docker pull --platform linux/amd64 ghcr.io/julietamelinsky01/pagina-ing3-backend:sha-be64dbb43863904c389dd7d0c15823269d07fd19`
- Frontend: https://github.com/users/julietamelinsky01/packages/container/package/pagina-ing3-frontend
  `docker pull --platform linux/amd64 ghcr.io/julietamelinsky01/pagina-ing3-frontend:sha-be64dbb43863904c389dd7d0c15823269d07fd19`

**La cadena del artefacto** (las dos corridas del mismo cambio):

- Corrida del PR (construye pero no publica; el paso «Entrar al registry» queda salteado): https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/38977758581
- Corrida de `main` tras el merge (publica como último paso de cada job): https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39158994571

**Entornos vivos** (Render + Neon):

| | Front | API |
|---|---|---|
| QA | https://pagina-ing3-front-qa.onrender.com | https://pagina-ing3-api-qa.onrender.com/health |
| PROD | https://pagina-ing3-front-prod.onrender.com | https://pagina-ing3-api-prod.onrender.com/health |

El plan gratis de Render duerme los servicios: el primer pedido puede tardar ~50 s (ver «La letra chica del free tier»).

**Flujo de aprobación y release:**

- Rechazo con motivo: https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39169844071 (corrida #28, `deploy-prod` rechazado).
- Aprobación (QA → «Waiting for review» → aprobado → PROD con smoke verde): https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39171298223 (corrida #30) y https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39172530014 (corrida #32).
- Release: https://github.com/julietamelinsky01/Pagina-ing3/releases/tag/v6.0.0

## TP1 — Git colaborativo

### 1. Por qué Git no pudo resolver el conflicto solo

Git no pudo resolver el conflicto automáticamente porque las dos ramas habían modificado la misma línea del archivo `conflicto-tp1.txt` de maneras diferentes: la rama A escribió "Versión A del conflicto del TP1" y la rama B escribió "Versión B del conflicto del TP1".

Git detectó que existían dos cambios incompatibles sobre la misma línea, pero no podía determinar cuál de las dos versiones era la correcta. Por ese motivo fue necesario resolver el conflicto manualmente, eligiendo el contenido que debía quedar.

El conflicto se podría haber evitado si las ramas hubieran modificado partes distintas del archivo o si se hubiera integrado una de las ramas antes de realizar el cambio conflictivo en la otra.

### 2. Problemas encontrados y cómo los solucioné

- **"Require approvals" activado por defecto:** al configurar la protección de `main`, GitHub solicitaba una aprobación para poder mergear. Como el TP era individual, no podía aprobar mi propio Pull Request. Se resolvió desactivando ese requisito y manteniendo la obligación de ingresar los cambios mediante Pull Request.

- **Nombres automáticos de ramas:** al crear algunas ramas desde la interfaz web de GitHub se generaron nombres automáticos, en lugar de la convención `feature/...` sugerida. Verifiqué que esto no afectaba el funcionamiento del flujo de trabajo.

- **Terminal que parecía trabada:** al pegar varios comandos juntos, en algunos casos la terminal quedaba esperando. Lo solucioné cancelando con `Ctrl+C` y ejecutando los comandos individualmente.

- **Conflicto intencional entre ramas:** dos ramas modificaron la misma línea del archivo `conflicto-tp1.txt`. GitHub detectó el conflicto y bloqueó el merge hasta que fue resuelto manualmente.

### 3. Declaración de uso de IA

Utilicé Claude (Anthropic) como asistente durante el desarrollo del TP1 para guiarme paso a paso en tareas como la configuración de la protección de rama, la creación de Pull Requests, la generación del conflicto, su resolución y la creación del tag y la release.

Las indicaciones fueron verificadas durante el trabajo práctico ejecutando los comandos y comprobando sus resultados en Git y GitHub. Las evidencias del push rechazado, el conflicto, los marcadores de conflicto y la release publicada quedaron registradas en `evidencias.md`.

## TP2 — Contenedores

### Decisiones de la app base

### Base de datos: PostgreSQL (no SQL Server)

El spec original dejaba esto "a definir", con SQL Server como opción por defecto. Se descartó porque:

- SQL Server no corre nativamente en macOS (haría falta Docker + una imagen especial tipo
  `azure-sql-edge`), mientras que Postgres se levanta en minutos con un `docker run` simple o con
  Homebrew.
- Es el motor que mejor cumple el criterio de la cátedra "que puedan ejecutarla hoy" y "sin
  dependencias exóticas".
- EF Core lo soporta igual de bien vía `Npgsql.EntityFrameworkCore.PostgreSQL`, sin ninguna
  desventaja funcional para este proyecto.

### UI library: MUI

Se evaluó contra Bootstrap (react-bootstrap). Se eligió MUI porque sus componentes de tabla, formularios
y diálogos modales encajan directo con las pantallas de CRUD y el calendario semanal, sin tener que
armar mucho a mano.

### Autenticación: tabla `Usuario` en base de datos

Se evaluó contra una credencial fija en `appsettings.json`. Se eligió la tabla en base de datos
(username + hash BCrypt) porque:

- Es más representativo de un sistema real con JWT (el login valida contra un registro persistido,
  no una constante).
- El hash se generó una sola vez con `BCrypt.Net.BCrypt.HashPassword` y quedó fijo en el seed de la
  migración (`HasData`), para que la migración sea reproducible — no se genera un hash nuevo (con
  salt distinto) cada vez que EF recalcula el modelo.

### Connection string parametrizable por variable de entorno

`appsettings.json` solo tiene un valor de desarrollo. `Program.cs` lo lee vía
`builder.Configuration.GetConnectionString(...)`, que ASP.NET Core resuelve por la convención
`ConnectionStrings__DefaultConnection` como variable de entorno — sin tocar código ni el archivo de
configuración. Esto es intencional pensando en el TP2 (la base pasa a vivir en un contenedor, cambia
el host) y el TP6 (la misma app apunta a bases distintas para QA y producción). Lo mismo aplica a la
clave JWT (`Jwt__Key`) y al origen permitido por CORS (`Frontend__Origin`).

### Reglas de negocio agregadas más allá del CRUD

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

### Migraciones automáticas al arrancar

`Program.cs` corre `db.Database.Migrate()` al iniciar la aplicación, para que `dotnet run` funcione
de punta a punta sin pasos manuales adicionales la primera vez que alguien clona el repo. Es una
concesión pensada para friction-less local dev en un proyecto académico; en un pipeline de CI/CD real
(TP6) esto normalmente se separaría en un paso de deploy explícito.

### Qué app y por qué

La app es la misma descripta arriba (Las Melis: .NET 8 + React/Vite + PostgreSQL), elegida contra los
criterios del §3.3 de la guía: buildea y corre local sin magia (probada antes de comprometerse),
tiene superficie para llegar a los 8+4 tests del TP5 (ver "Reglas de negocio" arriba), es un CRUD +
calendario acotado (3 pantallas principales) y es un dominio que entiendo lo suficiente como para
modificarlo en vivo en el Integrador. La app se desarrolló inicialmente en este repositorio y, para
unificar el repositorio oficial de la materia, se migraron aquí las decisiones y evidencias del TP1
y se recrearon sus protecciones según lo indicado por la guía.

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


## TP3 — Planificación y trazabilidad

### Herramienta de planificación

Para organizar el trabajo se utilizó GitHub Projects, mediante el proyecto `IngSoft3 - Las Melis DevOps`.

Se configuraron dos vistas:

- Una vista de tabla para visualizar los ítems y su jerarquía.
- Una vista de tablero con los estados `Todo`, `In Progress` y `Done`.

También se creó el campo de iteración `Sprint` y se configuró `Sprint 1` para el período del 30 de agosto al 12 de septiembre. Se eligió una duración de dos semanas porque permite agrupar una cantidad razonable de trabajo y se alinea mejor con el ritmo de entregas de la materia, evitando tanto sprints demasiado cortos como períodos demasiado largos sin revisión.

### Jerarquía de trabajo

Se utilizó una estructura de Epic → Story → Task para representar distintos niveles de planificación.

La jerarquía implementada fue:

- Epic #7: `Pipeline DevOps completo para Las Melis`
  - Story #8: `CI: build y tests automáticos en cada PR`
    - Task #9: `Escribir el workflow de build y tests`
    - Task #10: `Publicar el reporte de tests como artefacto`

También se agregó el Bug #11: `El frontend carga sin datos si el backend todavía no responde`, para representar trabajo correctivo dentro del backlog.

### Sprint y estados

Los ítems del backlog fueron incorporados a `Sprint 1`.

La Task #9 se movió de `Todo` a `In Progress` al comenzar su implementación y posteriormente a `Done` al completarse mediante Pull Request.

La Task #10 permanece en `Todo`, ya que la publicación del reporte como artefacto depende de disponer primero de tests que generen un reporte real.

### Trazabilidad entre planificación y código

Para la Task #9 se creó desde la propia Issue la rama:

`9-escribir-el-workflow-de-build-y-tests`

En esa rama se agregó el archivo `.github/workflows/ci.yml`, con un workflow inicial de GitHub Actions ejecutado ante Pull Requests hacia `main`.

El cambio se registró en el commit:

`be5a9c5` — `ci: agregar esqueleto del workflow de build y tests`

Luego se creó el Pull Request #12, incluyendo `Closes #9` en su descripción.

El Pull Request ejecutó correctamente el workflow de GitHub Actions, fue integrado a `main` y GitHub cerró automáticamente la Task #9 y la movió a `Done`.

De esta manera quedó establecida la trazabilidad:

Epic #7 → Story #8 → Task #9 → Branch → Commit → Pull Request #12 → `main`.

### Decisiones tomadas

Se decidió utilizar GitHub Projects porque permite mantener la planificación y la implementación dentro de la misma plataforma, vinculando Issues, jerarquías, ramas y Pull Requests.

Se utilizó una iteración de dos semanas para representar el sprint y un tablero simple de tres estados (`Todo`, `In Progress`, `Done`) para visualizar el avance.

En la columna `In Progress` se configuró un límite WIP de 2 elementos. Como el trabajo es individual, se tomó como criterio una persona + 1, permitiendo como máximo dos tareas simultáneas en progreso. El objetivo es evitar comenzar demasiadas tareas al mismo tiempo y favorecer que el trabajo iniciado se termine antes de incorporar uno nuevo.

La Task #10 no se marcó como completada porque actualmente el repositorio no posee un proyecto de tests que genere un reporte real. Se prefirió mantener la tarea pendiente en lugar de publicar un artefacto ficticio únicamente para completar el tablero.

### Diagnóstico de una historia mal escrita

Una historia como `Como desarrollador quiero crear la tabla usuarios` está mal formulada como historia de usuario porque describe directamente una tarea técnica y no expresa qué usuario obtiene valor ni cuál es el beneficio esperado.

Una formulación más adecuada sería: `Como administrador quiero registrar usuarios en el sistema para poder gestionar quiénes tienen acceso a la aplicación`.

De esta manera la historia expresa actor, necesidad y beneficio, mientras que `crear la tabla usuarios` quedaría como una tarea técnica necesaria para implementar esa historia.

### Uso de IA

Se utilizó ChatGPT (OpenAI) como asistente durante el TP3 para guiar la configuración del GitHub Project, la jerarquía Epic → Story → Task, la creación y vinculación de ramas e Issues, el flujo mediante Pull Request y la documentación de las decisiones tomadas.

Las indicaciones se verificaron directamente en Git y GitHub durante el desarrollo. La Task #9 fue implementada mediante una rama vinculada, un commit y el Pull Request #12, y su cierre automático permitió comprobar la trazabilidad entre la planificación y el código.

## TP4 — Integración Continua: Pipelines as Code

### Implementación de la pipeline

Para implementar Integración Continua se utilizó GitHub Actions mediante el archivo `.github/workflows/ci.yml`.

El workflow se configuró para ejecutarse automáticamente ante Pull Requests hacia `main` y también ante pushes a `main`. De esta manera, los cambios propuestos son verificados antes de integrarse y el estado de la rama principal también queda validado después de cada merge.

En este TP la pipeline verifica el build de la aplicación. No se incorporaron tests ni reportes de tests, ya que esa etapa corresponde al TP5.

### Jobs de backend y frontend

La pipeline se dividió en dos jobs independientes:

- `build-backend`
- `build-frontend`

Se decidió separar ambos componentes porque el backend y el frontend poseen procesos de construcción y Dockerfiles diferentes.

Al no existir una dependencia entre los jobs, GitHub Actions puede ejecutarlos en paralelo. Esto permite detectar de forma independiente qué componente falla y evita esperar innecesariamente a que termine un build para comenzar el otro.

Cada job utiliza un runner `ubuntu-latest`, obtiene el código mediante `actions/checkout` y construye la imagen correspondiente utilizando `docker/build-push-action`.

### Uso de los Dockerfiles del TP2

Para validar la construcción de la aplicación se decidió reutilizar los Dockerfiles definidos en el TP2:

- `./backend/Dockerfile`
- `./frontend/Dockerfile`

De esta manera, la pipeline verifica exactamente el mismo mecanismo de construcción utilizado para contenerizar la aplicación.

Se prefirió esta alternativa frente a ejecutar directamente comandos como `dotnet publish` o `npm run build` en el workflow, porque mantener una única definición de build reduce el riesgo de que la construcción local mediante Docker y la construcción realizada por CI evolucionen de forma diferente.

### Caché de capas de Docker

Se configuró Docker Buildx junto con el caché provisto por GitHub Actions mediante:

`cache-from: type=gha`

y

`cache-to: type=gha,mode=max`

Se utilizaron scopes separados (`backend` y `frontend`) para evitar mezclar las capas correspondientes a ambas imágenes.

La primera ejecución construye las capas necesarias y las almacena en caché. En ejecuciones posteriores, las capas que no cambiaron pueden reutilizarse.

Para comprobarlo se realizó una segunda corrida de la pipeline mediante el commit `ci: segunda corrida para ver el cache`. En los logs del backend se observaron múltiples etapas marcadas como `CACHED`.

El caché es únicamente una optimización de rendimiento. Si se elimina o no está disponible, la pipeline debe seguir funcionando correctamente; simplemente deberá reconstruir las capas y tardará más tiempo.

### Quality gate sobre main

La protección de la rama `main` se configuró para exigir que los siguientes status checks finalicen correctamente antes de permitir un merge:

- `build-backend`
- `build-frontend`

También se habilitó la opción que exige que la rama del Pull Request se encuentre actualizada respecto de `main`.

De esta manera, la pipeline deja de ser solamente informativa y pasa a funcionar como un quality gate: un cambio que no construye correctamente no puede incorporarse a la rama principal.

### Demostración del bloqueo y recuperación

Para verificar el funcionamiento real del gate se creó el Pull Request #16 desde la rama `feature/demo-gate`.

Se introdujo intencionalmente la línea `using NoExiste;` en `backend/LasMelis.Api/Program.cs`. Esto provocó un error de compilación durante `dotnet publish`.

Como resultado:

- `build-backend` falló.
- `build-frontend` finalizó correctamente.
- GitHub marcó ambos checks como requeridos.
- El merge quedó bloqueado mientras el backend permanecía en rojo.

Después de comprobar el bloqueo se eliminó el error mediante el commit `fix: saca el using que no existe`.

La pipeline volvió a ejecutarse automáticamente y ambos jobs finalizaron correctamente. Recién entonces el Pull Request quedó habilitado para mergearse.

De esta forma se comprobó el ciclo completo:

`cambio incorrecto → pipeline roja → merge bloqueado → corrección → pipeline verde → merge habilitado`.

### Badge de estado

Se agregó al `README.md` el badge oficial del workflow `CI`.

El badge permite visualizar directamente desde la página principal del repositorio el estado actual de la Integración Continua. Con la pipeline funcionando correctamente se muestra el estado `CI passing`.

### Problemas encontrados y cómo se resolvieron

- El repositorio ya contaba con un workflow mínimo creado durante el TP3. Para el TP4 se reemplazó ese esqueleto por los jobs reales `build-backend` y `build-frontend`.

- Se verificó el funcionamiento del caché mediante una segunda ejecución sin cambios relevantes, comprobando en los logs que Docker reutilizaba capas marcadas como `CACHED`.

- Para comprobar el quality gate se introdujo deliberadamente un error de compilación. Esto permitió verificar que no alcanza con tener una pipeline configurada: para proteger efectivamente `main`, sus checks deben configurarse como requeridos en las reglas de protección de la rama.

- Al incorporar el badge se encontraron dificultades al copiar su Markdown mediante la terminal y el chat, ya que el enlace se deformaba. Se resolvió utilizando directamente el Markdown generado por GitHub mediante la opción `Create status badge`.

### Uso de IA

Se utilizó ChatGPT (OpenAI) como asistente durante el TP4 para guiar la implementación progresiva del workflow, la incorporación del build de backend y frontend, la configuración del caché de Docker, la protección de `main`, la demostración controlada del quality gate y la incorporación del badge de CI.

Las indicaciones fueron verificadas directamente mediante Git, Docker, GitHub Actions y las reglas de protección del repositorio. Se comprobó el uso efectivo del caché observando etapas `CACHED` en los logs y el funcionamiento del gate mediante un Pull Request que pasó de un build fallido y merge bloqueado a checks exitosos y merge habilitado.

## TP5 — Calidad automatizada: tests, coverage y el umbral que frena un merge

### Qué lógica elegí testear y por qué ESA

Testeé la **capa de servicios** del backend (`EmpleadoService`, `AsignacionTurnoService`, `TipoTurnoService`, `AuthService`), `TurnoHorasCalculator` y `ExceptionHandlingMiddleware`, porque ahí viven las reglas de negocio y es donde un bug duele en esta app: asignar un turno a un empleado dado de baja, duplicar una asignación, o calcular mal las horas de un turno que cruza la medianoche (Noche 22:00–06:00) termina en un reporte de horas equivocado. Son 4 reglas centrales, cada una con su caso feliz y sus bordes:

1. DNI único por empleado (incluido el borde "editar sin cambiar mi propio DNI").
2. La fecha de ingreso no puede ser futura (incluido el borde "ingresar hoy").
3. No se asigna un turno a un empleado inactivo ni se duplica la combinación empleado + turno + fecha.
4. Cálculo de horas con turnos que cruzan la medianoche (con `[Theory]`: turno normal, nocturno, `fin == inicio` = 24 hs, media hora).

Además: no se puede borrar un tipo de turno con asignaciones, la baja avisa cuántos turnos futuros quedan, el rango `desde`/`hasta` no puede estar invertido, y el login no revela si falló el usuario o la contraseña. La suite del backend tiene **43 métodos** `[Fact]`/`[Theory]` (50 casos contando los datos de cada `[Theory]`) y la del frontend **37 casos** (`it`/`it.each`: 11 en `fechas.test.js` y 26 en `reglas.test.js`).

En el frontend testeé **lógica pura** extraída de las páginas a `src/utils/reglas.js` (`dniValido`, `validarEmpleado`, `existeAsignacion`, `totalHorasPorEmpleado`, `cargarFilasReporte`) y las fechas de `src/utils/fechas.js` (`lunesDeLaSemana` con el borde del domingo, `sumarDias` cruzando mes y año). No testeé componentes React: se verifican end-to-end en el TP7.

Las tres técnicas, de los dos lados:

| Técnica | Backend | Frontend |
|---|---|---|
| Parametrizado | `[Theory]` + `[InlineData]` en `TurnoHorasCalculatorTests` y en el middleware | `it.each` en `dniValido`, `validarEmpleado`, `existeAsignacion`, `lunesDeLaSemana`, `sumarDias` |
| Caso de error | `Crear_ConFechaDeIngresoFutura_EsRechazadoYNoGuarda`, `Crear_ParaUnEmpleadoInactivo_EsRechazadoYNoGuarda` (el mensaje y el "no guardó" también se verifican) | `validarEmpleado` rechaza cada campo faltante y dice cuál; `cargarFilasReporte` rechaza un rango invertido |
| Mock | Moq sobre `IEmpleadoRepository`, `IAsignacionTurnoRepository`, `ITipoTurnoRepository` y `IUsuarioRepository`: se reemplaza la base y se verifica la interacción (`Verify(..., Times.Once / Times.Never)`) | `vi.fn()` como `obtenerAsignaciones` en `cargarFilasReporte`: `toHaveBeenCalledWith(desde, hasta)` y `not.toHaveBeenCalled()` |

### Si refactoricé para poder mockear

- **Backend: no hizo falta.** Los servicios ya recibían sus repositorios por **interfaz en el constructor** (y están registrados en `Program.cs`), así que Moq pudo fabricar el doble sin tocar el código de la app.
- **Frontend: sí.** La lógica estaba adentro de los componentes: `ReporteSemanal` llamaba directo a `getAsignaciones` y calculaba los totales en un `useMemo` dentro del componente, y `EmpleadoForm`/`AsignacionForm` tenían las validaciones en línea. Eso no se puede testear sin montar la UI. Las saqué a funciones puras en `src/utils/reglas.js` y los componentes ahora las llaman, con el mismo comportamiento. En particular `cargarFilasReporte(desde, hasta, obtenerAsignaciones)` recibe la dependencia que habla con la API **por parámetro**: en la app es `getAsignaciones`, en el test un `vi.fn()`.

### Mi umbral de coverage

| | Backend | Frontend |
|---|---|---|
| Umbral | **65** | **90** |
| Métrica sobre la que frena | **línea y rama** (`ThresholdType=line%2cbranch`: un solo `Threshold` se aplica a las dos y frena por la que quede corta) | **línea y rama** (`thresholds: { lines: 90, branches: 90 }`) |
| Lo que mido hoy | **68,7 % de línea · 85,29 % de rama** | **100 % de línea · 100 % de rama** |

**Por qué esos números.** Anclé el umbral en mi medición real, por debajo, para que me frene si baja y no sea inalcanzable. En el backend puse 65 porque el 68,7 % está arrastrado por código real que hoy **no** tiene test unitario (controllers y repositorios de EF, ver abajo); con 65 tengo ~4 puntos de margen y un cambio con unas 30 líneas nuevas sin tests ya lo cruza. En el frontend la lógica pura es chica y la tengo al 100 %, así que 90 deja margen para una rama menor sin cubrir pero frena un archivo nuevo sin tests. Reporto siempre el de **rama** aunque el umbral sea sobre ambas: es la más honesta, porque un `if` con una sola rama ejercitada da 100 % de línea y 50 % de rama.

**Qué haría falta para subirlo a 85 en el backend:** tests de integración de los repositorios contra una base real y de los controllers (hoy no los hay): eso es del TP7, no de un unit test.

### Qué dejé afuera de la cuenta de cobertura (y por qué cada cosa)

**Backend** (`/p:Exclude` en el `ENTRYPOINT` de la etapa `test` del `backend/Dockerfile`, y el mismo recorte en `-classfilters` del reporte, para que Summary y umbral midan lo mismo):

- `Program*` — el arranque: cablea servicios, JWT y CORS; no tiene reglas de negocio y, si está mal, la app no levanta.
- `LasMelis.Api.Data.*` — el `AppDbContext`: configuración del modelo, sin reglas.
- `LasMelis.Api.Models.*` — clases de datos, sólo propiedades.
- `LasMelis.Api.Migrations.*` — código **generado** por EF Core.

**Lo que dejé ADENTRO sin tests, a propósito:** `Controllers` y `Repositories`. Son código escrito por mí, no generado, y no los excluí porque sacarlos hubiera subido el número sin que los tests verifiquen más. Tienen ~160 líneas sin cubrir y por eso el backend mide 68,7 % y no 95 %. Los controllers sólo delegan en el servicio y los repositorios sólo traducen a consultas de EF; lo que importa de ellos se verifica con integración / e2e (TP7).

**Frontend** (`include: ['src/utils/**']` en `vite.config.js`): entra sólo la lógica pura. Quedan afuera las páginas y componentes (UI, verificada end-to-end en el TP7), `src/api` (adaptadores finos sobre axios) y el arranque (`main.jsx`, `App.jsx`). Verifiqué que el reporte lista los archivos esperados (`fechas.js` y `reglas.js`) y no mide cero.

### Por qué coverage alto no garantiza calidad (con MI ejemplo)

Cuando medí por primera vez, `EmpleadoService` ya tenía sus líneas cubiertas, y aun así le **invertí a mano** el borde de la regla de fecha (`fechaIngreso > hoy` → `>=`) y **los 46 tests seguían en verde**: el mutante sobrevivió. La línea estaba ejecutada, pero nadie verificaba el caso "ingresó hoy". La cobertura medía ejecución, no verificación. Escribí `Crear_ConFechaDeIngresoDeHoy_EsValido`, repetí la mutación y esta vez el test se puso en rojo.

Apliqué el mismo control a 7 mutantes (bordes `<=`/`<`, la regla de inactivos invertida, el rango de fechas, el DNI de 7–8 dígitos, el domingo en `lunesDeLaSemana`): los 7 mueren con la suite actual. Un test de "cobertura sin verdad" sería, conceptualmente, `TurnoHorasCalculator.CalcularHoras(new TimeOnly(8,0), new TimeOnly(16,0));` sin ningún `Assert`: ejecuta todo y no comprueba nada.

### El ejercicio de la rama sin cubrir

Abrí el reporte de cobertura del backend y busqué ramas de código a medias (naranja, `1/2`). Eligí la de **`AsignacionTurnoService.cs`, línea 64**, el `?? throw new NotFoundAppException(...)` al principio de `UpdateAsync`.

1. **Qué línea es:** `var asignacion = await _repository.GetByIdAsync(id) ?? throw ...` — el `??` abre dos caminos: la asignación existe (cubierto) y no existe (sin cubrir).
2. **Qué entrada la recorrería:** `UpdateAsync(999, dto)` con el repositorio devolviendo `null` → tiene que lanzar `NotFoundAppException` y no llamar a `UpdateAsync` del repositorio.
3. **Qué decidí:** **no agregar el test por ahora.** Es el mismo patrón `?? throw new NotFoundAppException(...)` que ya verifican `GetByIdAsync` y `DeleteAsync` en ese mismo servicio, así que el riesgo de que esté mal escrito es bajo. Lo agregaría si ese `UpdateAsync` empezara a tener más lógica antes de la validación, porque ahí el camino "no existe" dejaría de ser trivial.

(La rama de `TipoTurnoService.UpdateAsync`, línea 45, quedó en el mismo estado y por el mismo motivo.)

### Mi Pull Request bloqueado por cobertura

- **PR que cuenta la historia (mergeado):** https://github.com/julietamelinsky01/Pagina-ing3/pull/19 — rojo por cobertura → los tests que faltaban → verde → merge. Agregué `src/utils/carga.js` (`nivelDeCarga` y `avisoDeCarga`: dos funciones con una cadena de `if`) sin un solo test: compila perfecto y los 37 tests pasan, pero la cobertura baja.
- **PR que prueba el freno (abierto y en rojo hasta la defensa):** https://github.com/julietamelinsky01/Pagina-ing3/pull/20 — agrega `src/utils/turnos.js` (`etiquetaDeTurno`) sin tests, desde `main`, después del merge del primero.
- **Corrida roja por umbral, con el número en el log:** https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/37683090184
- **Qué check se puso en rojo y en qué métrica:** `build-frontend`, en el paso "Correr los tests del frontend con coverage", con vitest 5.0.3. El log dice `ERROR: Coverage for lines (66.12%) does not meet global threshold (90%)` y `ERROR: Coverage for branches (55.55%) does not meet global threshold (90%)`: frenó por **líneas y por ramas a la vez**, con los 37 tests en verde. `build-backend` quedó en verde.
- **Qué escribí para arreglarlo:** `src/utils/carga.test.js`, con un test por cada camino que declara `nivelDeCarga` (sin datos con `it.each` para nulo / indefinido / NaN, inválido, sin turnos, parcial, normal, completa, exceso) y los bordes en 20, 40 y 48 horas, más cada aviso de `avisoDeCarga` (exceso, jornada completa, sin turnos y el caso sin aviso). Con eso la corrida pasó a verde: https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/37683900655

Este freno es distinto del del TP4: allá el job se ponía en rojo porque algo **no compilaba**; acá compila perfecto y los tests pasan todos, y el merge se bloquea igual porque un número que yo elegí no se cumple. Lo que deja pasar igual: que el requisito esté mal entendido (los tests custodian lo que yo entendí) y todo lo que no está medido (UI, controllers, repositorios).

### Enlaces que prueban cada decisión

- Resumen de cobertura y reporte descargable (corrida verde, con el resumen del backend en la página y los artefactos `coverage-report` y `coverage-frontend`): https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/37683900655
- Corrida roja por umbral: https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/37683090184
- Secuencia rojo → tests → verde → merge: https://github.com/julietamelinsky01/Pagina-ing3/pull/19
- Freno vigente, abierto y en rojo: https://github.com/julietamelinsky01/Pagina-ing3/pull/20

### Problemas encontrados y cómo los resolví

- **Un mutante sobrevivió** en la regla de fecha de ingreso (ver arriba): la cobertura de líneas no lo veía. Se resolvió con el test del borde "hoy".
- **El umbral del backend no frena con `coverlet.collector`**, que es el paquete que trae el template y sólo mide. Hizo falta `coverlet.msbuild` y tres parámetros (`CollectCoverage`, `Threshold`, `ThresholdType`) en el `ENTRYPOINT`; verifiqué que realmente rompe: con `Threshold=90` el build falla con *"The minimum line coverage is below the specified 90"* aunque los 50 tests pasen.
- **La coma en MSBuild:** `ThresholdType=line,branch` y las listas de `Exclude` se parten por coma y fallan o se ignoran en silencio; se escriben `%2c`.
- **La etapa `test` del backend necesitaba el proyecto de tests dentro de la imagen:** el `backend/Dockerfile` del TP2 sólo copiaba el `.csproj` de la API para el `restore`. Ahora copia la solución y los dos `.csproj` antes del `restore` (si no, `dotnet test` bajaría los paquetes en cada corrida).
- **`@vitest/coverage-v8` tiene que ser de la misma versión que `vitest`** (5.0.3 los dos; lo comprobé con `npm ls`).
- **Un test de frontend que cubre una función no puede usar la red:** por eso la extracción de `cargarFilasReporte` con la dependencia por parámetro.

### Uso de IA

Usé **Claude Code (Anthropic)** para este TP: escribió la primera versión de la suite de tests del backend y del frontend, el refactor del frontend a `src/utils/reglas.js`, la etapa `test` de los dos Dockerfiles, los pasos nuevos del `ci.yml` y este apartado. **Cómo lo verifiqué:** corrí las dos suites (`dotnet test` y `vitest`); medí la cobertura real y elegí los umbrales sobre esos números; comprobé que el umbral **rompe** de verdad (`Threshold=90` falla con los tests en verde); apliqué mutación manual a 7 reglas para ver que algún test se ponga en rojo al invertirlas (así apareció el agujero del borde "hoy"); y revisé que el refactor del frontend no cambió el comportamiento (`vite build` y `oxlint` siguen pasando). Lo que no fue asistido: la app, sus reglas de negocio y la elección del stack (TP1–TP4). **Qué verifica cada assert y qué no está cubierto** lo tengo que poder explicar en la defensa: no están cubiertos los controllers, los repositorios de EF, `Program.cs`, la UI, ni el `UpdateAsync` de asignación/tipo de turno sobre un id inexistente.

## TP6 — CD: environments, aprobaciones y deployment patterns

### Qué publica el pipeline y por qué sólo con la verificación en verde

El pipeline publica **un paquete por Dockerfile**: `pagina-ing3-backend` y `pagina-ing3-frontend` en ghcr.io, etiquetados `sha-<commit>`. La publicación es el **último paso** de cada job, después de correr los tests y de medir el coverage, y sólo ocurre en un `push` a `main`. No existe un `if` que diga «si los tests pasaron»: no hace falta, porque los pasos corren en orden y se cortan al primer error. Son tres controles encadenados: (1) nada entra a `main` sin verde (branch protection del TP4/TP5), (2) sólo `main` publica, (3) publicar es lo último del job que testea.

Si se publicara igual, el registry dejaría de significar «esto pasó la verificación» y pasaría a ser un depósito de imágenes sin garantía: nadie podría confiar en que lo que está publicado es lo que se testeó. Limitación honesta: nada impide que alguien haga un `docker push` a mano; la garantía es de proceso, no criptográfica (un digest atado a la corrida sí lo sería).

### Continuous Delivery vs Continuous Deployment: cuál implementé

Implementé **Continuous Delivery**: cada cambio que pasa la verificación llega **solo** a QA, pero a producción sólo llega con una **aprobación humana** en el environment `production`. Continuous Deployment sería sacar esa aprobación y que todo lo verde llegue a PROD solo. Para esta app, con un solo desarrollador y sin observabilidad ni rollback automático, prefiero que una persona mire QA antes de tocar producción. Para pasar a Deployment me faltarían smoke tests más fuertes, monitoreo con alertas y un rollback automático; hoy no lo querría.

### El diseño de la cadena y el alcance de cada secret

```
build-backend / build-frontend   → verifican y publican la imagen (sólo en main)
deploy-qa    needs: los dos builds · if: main · environment: qa
deploy-prod  needs: deploy-qa      · if: main · environment: production · concurrency: deploy-production
```

- `needs` impone el orden: sin builds verdes no hay QA, y sin QA no hay PROD.
- `if: github.ref == 'refs/heads/main'` hace que los PR sólo verifiquen: en un PR, los dos jobs de deploy quedan salteados.
- Los **deploy hooks** de Render son secrets **de environment**: los de QA viven en `qa` y los de PROD en `production`. Un job que no declara `environment: production` no puede leer los hooks de producción, y un workflow que corre en una rama sin permiso tampoco. Si estuvieran en el repo, cualquier job (o un PR malicioso) podría desplegar a producción sin pasar por la aprobación.
- `production` tiene **required reviewer** (yo) con *Prevent self-review* desactivado (soy la única persona del repo) y *Allow administrators to bypass* desactivado, para que la aprobación sea obligatoria de verdad.
- Los hooks se llaman con **`&ref=$GITHUB_SHA`**: se despliega el commit que el pipeline verificó, no la punta de la rama. Sin eso, con dos merges seguidos, la corrida del primero desplegaría el segundo, que todavía no pasó por nada.
- `concurrency` (`cancel-in-progress: false`) evita dos deploys a PROD pisándose. No me ordena la cola de aprobaciones: si quedan dos corridas esperando, aprobar la vieja haría retroceder PROD, y la regla es rechazar la vieja a mano.

### Qué quedó por variable de entorno y qué quedó dentro de la imagen

**La misma imagen del front** sirve en QA y en PROD; cada entorno le dice a quién llamar por variables:

- Por variable (al arrancar): `BACKEND_URL` y `DNS_RESOLVER` del front (nginx las sustituye en la plantilla `default.conf.template` con `envsubst`); `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer` y `Jwt__Audience` del backend.
- Dentro de la imagen: el código, la plantilla de nginx, y valores por defecto pensados para `docker-compose` (`BACKEND_URL=http://backend:8080`).
- El front llama a la ruta relativa `/api` (no tiene URL de backend horneada) y nginx la reenvía al backend del entorno. El backend expone `/health`, que verifica además que la base responda.
- **Bases separadas por entorno:** dos bases en Neon, `app_qa` y `app_prod`. Lo comprobé creando un empleado sólo en `app_prod` desde el SQL Editor: aparece en PROD y no en QA.

### Qué mira mi aprobador antes de aprobar

Antes de apretar «Approve and deploy» reviso, en esa corrida concreta:

1. Que `build-backend`, `build-frontend` y `deploy-qa` estén en verde, y que el smoke test de QA haya pasado.
2. **Que el cambio esté visible en QA** (no confío en el verde solo: abro la URL de QA y veo el cambio funcionando).
3. Qué commit y qué PR entran (el título de la corrida), para saber qué toca.
4. Que no haya **otra corrida más nueva esperando aprobación**: si la hay, rechazo la vieja para que PROD no retroceda.

Una aprobación sin criterios sería sólo latencia. Cuándo **no** agregaría valor: en un cambio de documentación o en un entorno donde un fallo no cuesta nada.

### El rechazo documentado

La corrida #28 (`d234e47`) quedó con `deploy-prod` en «rejected»: la máquina obedeció al humano, el job terminó fallido y los pasos de deploy **no corrieron**. En Render, `pagina-ing3-front-prod` siguió con un único deploy (`ed6808b`, «First Deploy»), así que producción quedó intacta. Motivo del rechazo (textual del comentario): «Rechazado: todavía no revisé en QA que el cambio se vea bien antes de pasarlo a producción». Fue el primer rechazo, para comprobar que el gate frena; el segundo, con un motivo ligado al contenido de esa corrida, está más abajo.

**Segundo rechazo, con motivo ligado a esa corrida:** la corrida #34 (https://github.com/julietamelinsky01/Pagina-ing3/actions/runs/39174556699) es el merge de este mismo archivo, `decisiones.md`. Rechacé `deploy-prod` escribiendo: «Rechazado: este merge sólo cambia decisiones.md, no toca código ni configuración; no hay nada nuevo que probar en PROD». Lo que vi en esa corrida fue que el diff era sólo documentación: redesplegar PROD no cambia nada que se pueda probar y gasta minutos de build del plan gratis. Resultado: `deploy-qa` corrió y pasó, `deploy-prod` quedó fallido por el rechazo, y los pasos de deploy a producción no se ejecutaron.

### La letra chica del free tier y cómo la maneja el pipeline

- Los servicios gratis de Render **se duermen** por inactividad y el primer pedido tarda ~50 s (cold start). Me pasó: el primer acceso a QA mostró «Ocurrió un error inesperado» y se resolvió recargando.
- Hay 750 horas por workspace y 500 minutos de build por mes: cada deploy gasta build, por eso no despliego en los PR.
- El **smoke test reintenta** (30 intentos cada 20 s, 10 minutos como máximo, con `--max-time 10` por pedido): un `curl` seco daría falsos rojos por el cold start. Neon también suspende la base inactiva, y `/health` la despierta.

### Qué garantía pierdo porque Render reconstruye desde el repo

El deploy **no usa la imagen que publiqué en ghcr.io**: Render reconstruye el commit desde el repositorio. Entonces despliego el **commit verificado, no la imagen verificada**. Dos builds del mismo commit podrían diferir (por ejemplo, una imagen base que cambió entre una construcción y otra). Es lo que cierra el TP7: desplegar la imagen publicada por su digest, para que lo que corre sea exactamente lo que se verificó.

### Qué prueba mi smoke test y qué no

Prueba que el entorno **contesta**: `/health` del backend responde 200 (el proceso vive **y** la base responde), el front sirve la página (200) y el proxy de nginx llega al backend (`/api/empleados` sin token devuelve 401; con el proxy roto daría 502/504/404).

**No** prueba qué versión corre: el hook responde al instante y Render construye en segundo plano; si el build falla, sigue sirviendo la versión anterior y el smoke da verde igual. Lo vi en la práctica: `deploy-qa` tardó 8 s porque QA ya estaba vivo con la versión vieja. Por eso verifico además a mano el cambio visible (el pie «versión 6.0.0») en QA y en PROD.

### Deployment pattern para producción real

Para esta app, con usuarios reales, elegiría **blue-green** y complementaría con **feature flags**:

- Blue-green: levanto la versión nueva en un entorno paralelo («green»), la verifico con el smoke y conmuto el tráfico; el rollback es volver a conmutar, instantáneo. Costo: doble infraestructura durante el cambio.
- Canary lo descarto por ahora: pide partir el tráfico y medir tasa de errores y latencia en tiempo real para decidir, y hoy **me falta esa observabilidad** (métricas, alertas) y un balanceador con pesos.
- Rolling no me convence con una sola instancia por servicio.
- Con base de datos compartida, las migraciones tienen que ser compatibles hacia atrás (agregar primero, borrar después), o blue-green no sirve.
- **Deploy ≠ release:** con un feature flag despliego el código apagado y lo «libero» cuando decido, sin desplegar de nuevo. En mi app lo usaría, por ejemplo, para activar el reporte semanal sólo para algunos usuarios.

### Plan de rollback y su tiempo medido

Como el deploy se dispara con el commit explícito (`&ref=<sha>`), **volver atrás es disparar los mismos hooks con el SHA del último deploy bueno**:

1. Identifico el commit bueno anterior (la release `v6.0.0`: `git rev-list -n1 v6.0.0`).
2. Llamo a los dos hooks de PROD con `&ref=<sha>`.
3. Verifico en Render → *Deploys* que ese commit figure como **Live** en los dos servicios, y en la URL de PROD que volvió la versión anterior.

**Lo medí de verdad.** Subí PROD a la versión `6.0.1` (corrida #32) y volví a `v6.0.0` (commit `be64dbb`): disparé los hooks a las **19:40:12**; en Render el deploy del API empezó a las 19:40:14 y duró 42,8 s (Live ~19:40:57) y el del front duró 44,3 s (Live ~19:40:58). **Rollback medido: ~46 s** desde el disparo hasta que el último servicio quedó Live. Los ids de los deploys son `dep-dbakljlg1s2s73cfoal0` (API) y `dep-dbakljm7bikc73f0psv0` (front).

**Lo que el rollback de código no deshace:** los datos y el esquema de la base. Una migración que borró una columna sigue borrada aunque vuelva el código anterior. En este caso el rollback es seguro porque el cambio de `6.0.1` fue sólo de texto, sin migraciones.

La métrica DORA que ejercito es el **tiempo de recuperación** (*time to restore*); el pipeline también mide frecuencia de despliegue y *lead time* de cada cambio.

### Release

`v6.0.0` marca el commit que **efectivamente llegó a PROD** (`be64dbb`), no la punta de `main`: el número lo fija el práctico (TP6 → `v6.0.0`), no un criterio SemVer del producto. Se publicó como Release de GitHub con las notas generadas desde los PR #22 a #26.

### Problemas encontrados y cómo los resolví

- **Cold start:** el primer acceso a QA mostró un error genérico; era el servicio dormido y se resolvió recargando. El smoke test reintenta por eso.
- **Smoke verde contra la versión vieja:** el primer `deploy-qa` pasó en 8 s porque QA ya respondía con la versión anterior. Lo documenté como limitación y verifico el cambio visible a mano.
- **Render reconstruye en vez de usar la imagen publicada:** lo detecté al leer los deploys (trigger `Deploy Hook`, build desde el repo); lo dejo anotado para el TP7.
- **Comandos pegados donde no iban:** pegué una URL en la terminal y un link de hook en la línea de `read`; aprendí que `read -rs` va solo y espera el valor en la línea siguiente.

### Uso de IA

Usé Claude (Anthropic) como asistente para guiarme paso a paso: diseñar la configuración por entorno (nginx con plantilla, `/health`), escribir los jobs `deploy-qa` y `deploy-prod` del `ci.yml`, y preparar el rollback medido. Yo hice los merges, creé los environments y los secrets en GitHub, configuré Render y Neon, rechacé y aprobé los deploys, y ejecuté el rollback. Verifiqué cada paso contra las corridas de *Actions*, los *Deploys* de Render y las URLs de QA y PROD.

## TP7 — Contenedores de punta a punta: imagen inmutable, integración y e2e

### Build once, deploy many: qué problema del TP6 resuelve

En el TP6 mi pipeline publicaba la imagen en ghcr.io, pero Render **reconstruía** la app desde el repositorio en cada deploy: lo que corría en QA y en PROD no era la imagen que mis tests habían aprobado, sino otra construcción del mismo commit. Mi propio ejemplo: el `Dockerfile` del backend parte de una imagen base de .NET; si esa imagen base se actualiza entre el build de QA y el de PROD, los dos entornos podrían ejecutar binarios distintos aunque el código sea idéntico, y yo no podría demostrarlo.

Desde el TP7 la imagen se **construye una sola vez** (último paso de cada job de build, después de los tests) y los entornos la **ejecutan tal cual**: Render ya no compila nada. QA y PROD corren el mismo binario. Promover es decirle a PROD «ejecutá la imagen `X`», la misma `X` que QA verificó.

### Estrategia de etiquetas y de qué imagen corre cada servicio

- **`sha-<commit>` en el registry** (el commit completo, 40 caracteres): trazabilidad exacta, de qué commit salió la imagen. Es la única etiqueta que publica mi pipeline.
- **`v7.0.0` en git**: marca de cierre del práctico, puesta a mano sobre el commit que efectivamente está en PROD (`db433c2`), no sobre la punta de `main`.
- **Por qué no publico `latest`**: es un puntero que se mueve con cada merge y no dice qué contenido hay detrás; si lo desplegara, después no podría decir qué desplegué, haya pasado la e2e o no. Con una sola etiqueta por imagen no hay forma de desplegar algo sin decir exactamente qué es.
- **Limitación honesta:** los tags son mutables (volver a correr la corrida de un commit republica `sha-<commit>`, no necesariamente con el mismo contenido). Lo único inmutable es el digest (`@sha256:…`); no lo uso en este TP, pero el `imgURL` del hook lo acepta.
- **Con qué imagen configuré los servicios de Render y por qué no es la que corre:** a los cuatro les cambié la fuente a *Existing Image* con `…:sha-59e747abfff4…` (la de un merge anterior), porque Render pide una imagen para cambiar la fuente y esa ya la había comprobado con un `pull`. Es sólo el punto de partida: lo que corre lo nombra cada deploy de mi pipeline con su `imgURL`. Por eso *Settings → Image* sigue mostrando `59e747a…` mientras el servicio ejecuta `db433c2…`. Consecuencia: **«Manual Deploy» desde el panel desplegaría la imagen vieja** y llevaría PROD para atrás sin avisar, así que los deploys los dispara sólo el pipeline.
- **Servicios existentes:** no creé servicios nuevos; les cambié la fuente a los cuatro que ya tenía, lo que conserva sus URLs, variables, hooks e historial.

### Del tag a la imagen, en un paso

`git rev-list -n1 v7.0.0` devuelve el commit (`db433c2…`), y mi pipeline publicó las dos imágenes de ese commit con su nombre: `…-backend:sha-db433c2…` y `…-frontend:sha-db433c2…`. Volver a esa versión es mandarle esa etiqueta al hook (`imgURL`), el mismo mecanismo del deploy.

### Cómo se comprueba, desde afuera, que un entorno ejecuta mi imagen

En *Events* de cada servicio de Render, el deploy dice **«Triggered via Deploy Hook»** y nombra la imagen `sha-<commit>` de esa corrida (no un commit construido), y la misma etiqueta existe en mi paquete. Un deploy hecho a mano desde el panel se nota: no dice «Triggered via Deploy Hook» y no hay una corrida que lo respalde. El encabezado de cada servicio pasó de *Docker* a *Image*.

**Lo que mi smoke test no prueba:** sólo pregunta si el sistema **responde** (`/health` con la base, el front y el proxy). Si un deploy falla, Render sigue sirviendo la versión anterior y el smoke da verde igual. Lo viste en la práctica: el `deploy-qa` tarda segundos porque QA ya está vivo con la versión previa. Cerrarlo automáticamente exigiría hornear el commit en la imagen (un `ARG` con `build-args`), que el endpoint de vida lo devuelva y que el smoke lo compare contra el commit de la corrida; en este TP sólo lo estudio, no lo implemento.

### Mis pruebas: qué elegí y por qué

**Integración** (`frontend/e2e/api.spec.js`, sin navegador, contra la API de QA ya desplegada, con su base de verdad):

1. Alta + verificación + baja: crea un empleado, la base lo devuelve entre los activos, se lo da de baja y deja de estar.
2. DNI inválido: la API contesta 400 y no se creó nada.
3. **(Tercera, mía)** DNI repetido: la API contesta 409 y no aparece un segundo empleado. La elegí porque la unicidad del DNI se resuelve **consultando la base**: un doble de Moq acepta cualquier cosa, y esta prueba cubre justo lo que los unitarios del TP5 no pueden ver. Si se rompe, me escribe quien dé de alta empleados.

**E2E** (`frontend/e2e/empleados.spec.js`, un navegador de verdad contra el front de QA, sin mocks de red):

1. **Login:** ingresar lleva a la lista de empleados y la sesión sobrevive a recargar. **(Tercer flujo, mío)**: es lo que todo usuario hace todos los días; si no anda, me escriben.
2. **Creación:** crear un empleado, verlo en la lista y darlo de baja (comprobando que desaparece de los activos).
3. **Validación:** un DNI inválido muestra el error, el botón «Guardar» queda deshabilitado y no se crea nada.

**Cada una cumple lo que pide la guía:** interactúa (llena y clickea), afirma sobre el dato que ella misma produjo (un nombre único con la hora adentro, un DNI único de 8 dígitos) y limpia lo que creó, comprobándolo. Busca por *label* y *role* (`getByLabel`, `getByRole`), no por CSS; los formularios de mi app ya tenían etiquetas accesibles (MUI `TextField` con `label`).

**Qué NO puse en cada suite y por qué (pirámide):** no repito en e2e lo que ya cubren los unitarios (por ejemplo, cada regla de validación del DNI o de las fechas); ni pruebo en integración la pantalla. No cubrí Calendario ni Reporte semanal con e2e: son pocos flujos críticos a propósito, porque las e2e son las más lentas y las más frágiles.

**Datos que quedan en QA:** mi app no borra empleados de verdad, «dar de baja» es un borrado lógico. Mis pruebas «limpian» dándolos de baja: salen de los activos, pero quedan inactivos en la base de QA. Lo acepto en QA porque cada nombre y DNI es único y no se pisan entre corridas.

### Qué probás en integración y qué en e2e, y por qué no es lo mismo

- **Integración:** que la API desplegada y su base de verdad se entienden (endpoint → base, y lo que la base acepta, rechaza y guarda). **No ve** si la pantalla usa ese endpoint.
- **E2E:** el sistema entero como lo usa una persona. **No ve** lo que no recorren mis pocos flujos, y cuando se pone roja no dice quién se rompió por sí sola.
- **Lo que el unitario no ve y la integración sí:** un cambio de una línea, como cambiar `DateTime.UtcNow` por `DateTime.Now`, deja verdes todos los unitarios (el doble acepta cualquier fecha) pero hace que Postgres rechace la escritura y la API conteste 500.

**El par verde/rojo de mi corrida roja (#42), leído contra la tabla:** rompí **la aplicación** —el texto del botón «Nuevo empleado» pasó a «Agregar empleado» en `Empleados.jsx`—, sin tocar `e2e/`. Resultado: builds verdes, smoke **verde** (el sistema respondía), **integración verde** (3 de 3), **e2e roja** (1 pasó —el login— y 2 fallaron), `deploy-prod` sin arrancar. Diagnóstico con los dos reportes, sin mirar el código: la integración verde dice que la API y la base están sanas (guardó, rechazó y borró sin problemas); la e2e roja falló esperando `getByRole('button', { name: 'Nuevo empleado' })` hasta agotar el minuto, y la captura de la página, tomada al fallar, muestra un botón que dice «Agregar empleado». Entonces **se rompió el front**: fila 1 de la tabla (🟢 integración, 🔴 e2e). Si hubiera roto la API, la integración habría quedado roja y la e2e **no habría corrido** (su `needs` no se cumple).

**Qué NO ataja mi gate:** una rotura que ninguno de mis seis tests recorra (por ejemplo, un cambio en el Calendario o en el Reporte semanal), y todo lo que sólo ocurre en producción con datos o carga reales.

### Por qué mi integración es la «amplia» y no la «estrecha»

Mi integración le habla a **la API de QA ya desplegada**, con su base de verdad. La alternativa «estrecha» sería armar la API en memoria dentro del pipeline (`WebApplicationFactory`) con un Postgres descartable como `services:` del job.

- **Lo que gano:** con una fracción de la configuración pruebo el mismo concepto, y además compruebo que **el despliegue quedó bien** (la imagen, las variables y la conexión de QA), no sólo mi código.
- **Lo que pierdo:** depende de que QA esté vivo (cold start), comparte base con las otras corridas y con la e2e (hay que cuidar que no se ensucien), y no puedo correr contra una base vacía y conocida.

### El gate: cómo está armado y su límite conocido

Cadena en un único workflow, disparada por el `push` a `main`:

`build → deploy-qa (imgURL) → smoke → integracion → e2e → [aprobación] → deploy-prod (misma imagen) → smoke`

con `integracion` en `needs: deploy-qa`, `e2e` en `needs: integracion` y `deploy-prod` en `needs: e2e`. Nada desarma el gate: no hay `continue-on-error`, ni `|| true` ni `exit 0` después de Playwright, ni `if: always()` / `!cancelled()` en los jobs de deploy (los únicos `!cancelled()` son los de los pasos que publican reporte). El environment `production` sigue con su required reviewer del TP6. En la corrida #42 se vio que la e2e roja frenó la promoción **antes** de pedirme la aprobación.

**Límite conocido: QA es uno solo.** Con dos merges seguidos, la corrida B puede redesplegar QA mientras la integración o la e2e de la A lo están usando. Lo reconocería por un rojo que no es del código, o por un verde que en parte probó la imagen de B: mirando en *Events* del servicio de QA qué imagen está corriendo y a qué hora corrió el `deploy-qa` de B. Lo que haría: rechazar a mano la corrida A con su motivo (el `concurrency` no cancela un job que espera aprobación, sólo lo que está en cola) y quedarme con la B; y la regla de **un merge por vez** mientras la cadena corre. Un equipo real lo resuelve con un entorno por corrida, fuera del alcance de este TP.

### Cold start, timeouts y tests flaky

QA (plan gratis de Render) se duerme y tarda ~50 s en despertar. En el pipeline lo despiertan el smoke y la integración antes de la e2e, pero por internet un pedido suelto puede demorarse igual: por eso `playwright.config.js` usa **tiempos largos** (60 s por test, 15 s por aserción) y **un solo reintento**, sin ningún `sleep` fijo (Playwright espera solo a que el elemento aparezca). El cold start se resuelve con timeouts, **nunca salteando** tests.

**Un test flaky** es uno que a veces pasa y a veces falla sin que cambie el código. Es peor que no tener test porque entrena al equipo a ignorar el rojo. Mi configuración no los esconde: un test que pasa recién al reintentar sale como *flaky* en el reporte, y por eso abro el reporte aunque la corrida esté verde.

### Cómo la misma imagen del front sirve en QA y en PROD

La imagen del front no tiene ninguna dirección de backend adentro: llama a la ruta relativa `/api`, y nginx (con una plantilla `default.conf.template` procesada con `envsubst` al arrancar) la reenvía al backend del entorno.

- **Por variable de entorno (cada servicio de Render):** `BACKEND_URL` (la API de **su** entorno, sin barra final) y `DNS_RESOLVER` en los fronts; `ConnectionStrings__DefaultConnection` (la base de **su** entorno: `app_qa` o `app_prod`), `Jwt__Key`, `Jwt__Issuer` y `Jwt__Audience` en las APIs.
- **Adentro de la imagen:** el código, la plantilla de nginx y valores por defecto pensados para `docker-compose`.
- **La prueba:** cambié la fuente de los cuatro servicios y **no toqué ninguna variable**; QA y PROD siguen hablando cada uno con su API y su base. La misma imagen, configuración distinta.

### Problemas encontrados y cómo los resolví

- **Deploy parcial a PROD:** el primer intento del pipeline con `imgURL` falló en `deploy-prod` (`exit code 22`, un `4xx` de `curl -f`) después de haber desplegado la API de PROD: el front de PROD tenía mal configurada la imagen de su servicio. Quedó un deploy parcial (la API ya ejecutaba la imagen nueva; el front, no). Lo resolví corrigiendo la imagen de ese servicio en Render y **re-corriendo sólo el job que falló** (*Re-run failed jobs*, no *Re-run all jobs*, que también habría redesplegado QA). Aprendí que la api lleva `-backend` y el front `-frontend`: con el nombre cruzado Render contesta `400`.
- **Comandos en el lugar equivocado:** pegué el comando de la terminal en el editor SQL de Neon y la URL del hook en la línea del `read`. `read -rs` va solo y espera el valor en la línea siguiente; los comandos de shell van en la terminal y el SQL en Neon.
- **Usuario de pruebas y la contraseña de `admin`:** las pruebas necesitan iniciar sesión en QA. Creé un usuario **solo para las pruebas** (`e2e-qa`), con una contraseña propia cifrada con BCrypt, insertado únicamente en la base `app_qa`; sus credenciales viven en *secrets* del environment `qa` (`E2E_USER`, `E2E_PASSWORD`), nunca en el código. **Límite conocido:** la contraseña del usuario `admin` sembrado figura en el README de un repositorio público; en un sistema real se cambiaría en cada entorno.
- **vitest y Playwright en la misma carpeta:** vitest matchea por defecto todo `*.spec.js` y habría roto el job `build-frontend`. Lo evité con `exclude: [...configDefaults.exclude, 'e2e/**']` en `vite.config.js` (cada runner con su carpeta), y comprobé que los 54 unitarios siguen pasando.
- **Verificación antes de subir:** levanté una copia aislada de mi app con Docker Compose y corrí las 6 pruebas (3 de API y 3 de navegador) en verde antes de mergear el pipeline.
- **Compartir QA entre corridas:** ver el límite conocido del gate.

### Uso de IA

Usé Claude (Anthropic) como asistente para guiarme paso a paso en este TP: escribir las suites de Playwright, los jobs `integracion` y `e2e`, el cambio de los hooks a `imgURL` y el cambio que rompe el front a propósito. **Yo** cambié la fuente de los cuatro servicios en Render, creé el usuario de pruebas en la base de QA, cargué los *secrets* en GitHub, hice los merges, aprobé los despliegues y publiqué la release. Verifiqué cada paso contra *Actions*, los *Events* de Render y los reportes de Playwright. Puedo explicar **qué verifica cada test, contra qué entorno corre y qué pasa si falla**: los seis corren contra QA (los de integración contra la API, los e2e contra el front); si fallan, `deploy-prod` no arranca y producción queda intacta, como pasó en la corrida #42.

## TP8 — Infraestructura como código: el entorno `preprod`

**Riel y versiones.** Riel canónico: **Terraform** (probado con v1.17.0) contra **Neon + Render**, en `infra/`. Providers fijados por versión, con el `.terraform.lock.hcl` commiteado: `kislerdm/neon` **0.18.0**, `render-oss/render` **1.9.1** y `hashicorp/random` **3.8.1** (este último genera la clave JWT de preprod para no escribirla en ningún archivo). Preprod es un entorno **nuevo**: QA y PROD no se declaran ni se tocan. La cadena de conexión se arma con lo que devuelve Neon y la `BACKEND_URL` del front sale de la `url` de su api. El estado vive en mi máquina y está en el `.gitignore` (guarda contraseñas en texto plano). Ciclo `destroy → apply` hecho varias veces: vuelve idéntico, con las mismas URLs de Render, y QA y PROD siguieron intactos.

**Problemas y cómo los resolví.** (1) El primer `apply` falló con `org_id is required`: Neon ahora agrupa los proyectos por organización; obtuve el id con la API (`/api/v2/users/me/organizations`) y lo paso como variable sin default (`TF_VAR_neon_org_id`). (2) Render rechazó el `ownerID` porque había pegado la URL entera del dashboard; los owners válidos salen de `/v1/owners` (el mío es el workspace `tea-…`). (3) Al recrear preprod, Neon lo creó en **otra región** (`us-east-2` y después `us-west-2`), porque no la había declarado; la fijé con `region_id`. Al fijarla, el plan mostró `# forces replacement` en el proyecto y un `~ update in-place` en la api de Render: **no lo apliqué**, porque Render no permite modificar un servicio gratuito desde el provider y el `apply` se habría cortado a mitad; hice `destroy` y `apply`, que es lo que corresponde para un entorno desechable. (4) Como los fallos dejaron recursos a medio crear, comprobé que reintentar el `apply` sólo crea lo que falta: el estado recuerda lo que ya existe.

**Uso de IA.** Usé Claude (Anthropic) para escribir el HCL de `infra/` y diagnosticar los errores de los `apply`. Yo instalé Terraform, generé las claves de Neon y Render, corrí cada `plan`, `apply` y `destroy` desde mi terminal y verifiqué el resultado en las consolas de Neon y Render y con el `/health` de la api de preprod. Puedo leer el plan en voz alta: qué se crea, qué significa `(known after apply)` y `# forces replacement`, y por qué el `destroy` borra primero el front y por último el proyecto de Neon.
