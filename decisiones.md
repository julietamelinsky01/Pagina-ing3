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
3. **Qué decidí:** *(completar con tu decisión — las tres respuestas valen, incluida "no lo agregué"; lo que se evalúa es que miraste el código)*. Mi lectura: es el mismo patrón `?? throw` que ya verifican `GetByIdAsync` y `DeleteAsync` en ese servicio, así que el riesgo es bajo, pero el test cuesta cinco líneas, así que lo agregaría si ese `UpdateAsync` empezara a tener más lógica.

(La rama de `TipoTurnoService.UpdateAsync`, línea 45, quedó en el mismo estado y por el mismo motivo.)

### Mi Pull Request bloqueado por cobertura

*(completar después de la demostración — §3.5 de la guía)*

- **PR que cuenta la historia (mergeado):** `<URL del PR>` — rojo por cobertura → los tests que faltaban → verde → merge.
- **PR que prueba el freno (abierto y en rojo hasta la defensa):** `<URL del PR>`.
- **Corrida roja por umbral, con el número en el log:** `<URL de la corrida>`.
- Qué check se puso en rojo y en qué métrica: `<build-frontend / build-backend>`, `<líneas / ramas>`, con vitest 5.0.3 / coverlet.msbuild 10.1.0.
- Qué escribí para arreglarlo: `<tests por cada camino del código nuevo>`.

Este freno es distinto del del TP4: allá el job se ponía en rojo porque algo **no compilaba**; acá compila perfecto y los tests pasan todos, y el merge se bloquea igual porque un número que yo elegí no se cumple. Lo que deja pasar igual: que el requisito esté mal entendido (los tests custodian lo que yo entendí) y todo lo que no está medido (UI, controllers, repositorios).

### Enlaces que prueban cada decisión

- Resumen de cobertura y reporte descargable: `<URL de la corrida verde en main — …/actions/runs/<id>>`
- Corrida roja por umbral: `<URL de la corrida roja — …/actions/runs/<id>>`
- Secuencia rojo → tests → verde → merge: `<URL del primer PR — …/pull/<n>>`
- Freno vigente: `<URL del segundo PR, abierto — …/pull/<m>>`

### Problemas encontrados y cómo los resolví

- **Un mutante sobrevivió** en la regla de fecha de ingreso (ver arriba): la cobertura de líneas no lo veía. Se resolvió con el test del borde "hoy".
- **El umbral del backend no frena con `coverlet.collector`**, que es el paquete que trae el template y sólo mide. Hizo falta `coverlet.msbuild` y tres parámetros (`CollectCoverage`, `Threshold`, `ThresholdType`) en el `ENTRYPOINT`; verifiqué que realmente rompe: con `Threshold=90` el build falla con *"The minimum line coverage is below the specified 90"* aunque los 50 tests pasen.
- **La coma en MSBuild:** `ThresholdType=line,branch` y las listas de `Exclude` se parten por coma y fallan o se ignoran en silencio; se escriben `%2c`.
- **La etapa `test` del backend necesitaba el proyecto de tests dentro de la imagen:** el `backend/Dockerfile` del TP2 sólo copiaba el `.csproj` de la API para el `restore`. Ahora copia la solución y los dos `.csproj` antes del `restore` (si no, `dotnet test` bajaría los paquetes en cada corrida).
- **`@vitest/coverage-v8` tiene que ser de la misma versión que `vitest`** (5.0.3 los dos; lo comprobé con `npm ls`).
- **Un test de frontend que cubre una función no puede usar la red:** por eso la extracción de `cargarFilasReporte` con la dependencia por parámetro.

### Uso de IA

Usé **Claude Code (Anthropic)** para este TP: escribió la primera versión de la suite de tests del backend y del frontend, el refactor del frontend a `src/utils/reglas.js`, la etapa `test` de los dos Dockerfiles, los pasos nuevos del `ci.yml` y este apartado. **Cómo lo verifiqué:** corrí las dos suites (`dotnet test` y `vitest`); medí la cobertura real y elegí los umbrales sobre esos números; comprobé que el umbral **rompe** de verdad (`Threshold=90` falla con los tests en verde); apliqué mutación manual a 7 reglas para ver que algún test se ponga en rojo al invertirlas (así apareció el agujero del borde "hoy"); y revisé que el refactor del frontend no cambió el comportamiento (`vite build` y `oxlint` siguen pasando). Lo que no fue asistido: la app, sus reglas de negocio y la elección del stack (TP1–TP4). **Qué verifica cada assert y qué no está cubierto** lo tengo que poder explicar en la defensa: no están cubiertos los controllers, los repositorios de EF, `Program.cs`, la UI, ni el `UpdateAsync` de asignación/tipo de turno sobre un id inexistente.
