# Las Melis — Gestión de Empleados y Turnos

Sistema de gestión de empleados y turnos para una cafetería (franquicia Havanna). Backend en .NET 8
(ASP.NET Core Web API + EF Core), frontend en React (Vite + MUI), base de datos PostgreSQL.

## Estructura

```
/backend    → LasMelis.Api (.NET 8 Web API)
/frontend   → React (Vite) + MUI
```

Ver [decisiones.md](decisiones.md) para las decisiones técnicas (motor de base de datos, librería de
UI, autenticación) y la lista de reglas de negocio implementadas.

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18+ y npm
- PostgreSQL 16 (local o en un contenedor Docker — ver abajo)
- `dotnet-ef` (herramienta de migraciones): `dotnet tool install --global dotnet-ef`

### Instalar el SDK de .NET sin permisos de administrador (macOS)

Si `brew install --cask dotnet-sdk` pide contraseña de sudo y no la tenés a mano, se puede instalar
en el home del usuario con el instalador oficial de Microsoft, sin privilegios:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"

# Agregar al shell (~/.zshrc o ~/.bashrc):
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

## Levantar Postgres

Con Docker (recomendado, no requiere instalar Postgres en el sistema):

```bash
docker run -d --name lasmelis-postgres \
  -e POSTGRES_USER=lasmelis \
  -e POSTGRES_PASSWORD=lasmelis_dev \
  -e POSTGRES_DB=lasmelis \
  -p 5432:5432 \
  -v lasmelis_pgdata:/var/lib/postgresql/data \
  postgres:16
```

O con Homebrew: `brew install postgresql@16 && brew services start postgresql@16` y crear la base y
el usuario manualmente.

## Backend

```bash
cd backend/LasMelis.Api
dotnet restore
dotnet ef database update   # crea el esquema y aplica el seed (tipos de turno + usuario admin)
dotnet run --urls "http://localhost:5080"
```

La API queda en `http://localhost:5080` (Swagger en `/swagger` en modo Development). Al arrancar,
`Program.cs` aplica las migraciones pendientes automáticamente — no hace falta correr
`dotnet ef database update` a mano salvo la primera vez que se quiere inspeccionar el resultado.

### Usuario admin seedeado

| Usuario | Contraseña |
|---|---|
| `admin` | `Havanna2026!` |

### Configuración — la cadena de conexión NO está fija en el código

`appsettings.json` trae un valor de desarrollo (`ConnectionStrings:DefaultConnection`) que apunta al
contenedor Docker de arriba. Para apuntar a otra base (otro host, otro entorno) **no se toca el
código ni el `appsettings.json`** — se sobreescribe con una variable de entorno, que ASP.NET Core
mapea automáticamente por la convención de doble guion bajo:

```bash
export ConnectionStrings__DefaultConnection="Host=otro-host;Port=5432;Database=lasmelis;Username=...;Password=..."
dotnet run
```

Lo mismo aplica para el resto de la configuración sensible: `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`,
`Frontend__Origin` (el origen permitido por CORS).

## Frontend

```bash
cd frontend
npm install
cp .env.example .env   # ya viene con VITE_API_URL=http://localhost:5080/api
npm run dev
```

Queda en `http://localhost:5173`. Iniciar sesión con las credenciales del admin seedeado.

## Reglas de negocio (más allá del CRUD)

Ver el detalle y el motivo de cada una en [decisiones.md](decisiones.md). Resumen:

- DNI único por empleado.
- No se puede duplicar una asignación (mismo empleado + tipo de turno + fecha).
- No se puede asignar un turno a un empleado inactivo.
- Cálculo de horas por turno soporta turnos que cruzan la medianoche (ej. Noche 22:00–06:00).
- La fecha de ingreso de un empleado no puede ser futura.
- Dar de baja a un empleado con asignaciones futuras se permite, pero el sistema avisa cuántas tiene.
- No se puede eliminar un tipo de turno que ya tiene asignaciones asociadas.

## Endpoints principales

- `POST /api/auth/login`
- `GET|POST /api/empleados`, `PUT|DELETE /api/empleados/{id}` (DELETE = baja lógica)
- `GET|POST /api/tipos-turno`, `PUT|DELETE /api/tipos-turno/{id}`
- `GET /api/asignaciones?desde=YYYY-MM-DD&hasta=YYYY-MM-DD`, `POST|PUT|DELETE /api/asignaciones/{id}`

Todos (salvo `/api/auth/login`) requieren `Authorization: Bearer <token>`.
