# Decisiones técnicas

Este archivo reúne las decisiones tomadas en los trabajos prácticos de Ingeniería de Software 3.

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
