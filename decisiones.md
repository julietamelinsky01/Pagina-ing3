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

## Docker: frontend con URL absoluta + CORS, no proxy relativo

El TP2 (§2.6) da dos caminos para que el frontend contenerizado hable con el backend: (a) rutas
relativas `/api` con un proxy en nginx, o (b) URL absoluta al puerto publicado del backend + CORS. Ya
existía código con el camino (b) desde antes de dockerizar (`VITE_API_URL` en `frontend/src/api/client.js`,
consumida por Axios, más `AddCors`/`UseCors` en `Program.cs` habilitando el origen del frontend), así
que se mantuvo esa decisión en vez de reescribirla al camino (a):

- `VITE_API_URL` se pasa como build arg al `frontend/Dockerfile` (Vite resuelve las env vars en
  **build time**, no en runtime) apuntando a `http://localhost:8080/api`, el puerto que
  `docker-compose.yml` publica del backend.
- El backend habilita CORS para `http://localhost:3000` (variable `Frontend__Origin`), que es el
  puerto publicado del frontend.
- `frontend/nginx.conf` no tiene bloque `location /api/`: no hace falta, todas las llamadas salen
  directo del browser al backend. Sí tiene el `try_files` para el fallback de `react-router`
  (`BrowserRouter`), porque eso es necesario sea cual sea el camino elegido.

El costo del camino (b), tal como advierte la guía, es que la URL del backend queda fija en la imagen
del frontend: cambiar de entorno implica rebuildear con otro `VITE_API_URL`. Se acepta ese costo
porque es consistente con lo que ya tenía la app.

## Migraciones automáticas al arrancar

`Program.cs` corre `db.Database.Migrate()` al iniciar la aplicación, para que `dotnet run` funcione
de punta a punta sin pasos manuales adicionales la primera vez que alguien clona el repo. Es una
concesión pensada para friction-less local dev en un proyecto académico; en un pipeline de CI/CD real
(TP6) esto normalmente se separaría en un paso de deploy explícito.
