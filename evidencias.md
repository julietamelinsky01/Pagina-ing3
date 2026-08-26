# Evidencias — TP2 (Docker y Compose)

## 1. Build y arranque

```
docker compose up -d --build
```

Los tres servicios levantan correctamente:

```
NAME                     SERVICE    STATUS
pagina-ing3-backend-1    backend    Up
pagina-ing3-db-1         db         Up (healthy)
pagina-ing3-frontend-1   frontend   Up
```

Migraciones de EF Core aplicadas automáticamente al arrancar el backend (`db.Database.Migrate()`),
seed de tipos de turno y usuario admin incluido.

## 2. Comparación de tamaño de imágenes (multi-stage)

| Imagen | Rol | Tamaño |
|---|---|---|
| `mcr.microsoft.com/dotnet/sdk:8.0` | Etapa de build del backend (compilador, no viaja a producción) | 1.25 GB |
| `mcr.microsoft.com/dotnet/aspnet:8.0` | Runtime base de la etapa final del backend | 350 MB |
| **`pagina-ing3-backend`** | **Imagen final del backend** (runtime + binarios publicados) | **364 MB** |
| `node:22-alpine` | Etapa de build del frontend (no viaja a producción) | 228 MB |
| `nginx:alpine` | Runtime base de la etapa final del frontend | 92.7 MB |
| **`pagina-ing3-frontend`** | **Imagen final del frontend** (nginx + estáticos del build de Vite) | **93.5 MB** |

El multi-stage deja la imagen final del backend ~3.4x más chica que el SDK que la compiló, y la del
frontend ~2.4x más chica que la imagen de Node que la buildeó — en ninguna de las dos viaja a
producción el compilador/toolchain.

## 3. Prueba funcional end-to-end

```
$ curl -s http://localhost:8080/api/tipos-turno
HTTP 401                                          # sin token: correctamente rechazado (JWT + [Authorize])

$ curl -s -X POST http://localhost:8080/api/auth/login -d '{"username":"admin","password":"Havanna2026!"}'
{ "token": "eyJhbGciOiJIUzI1NiIs..." }            # login contra la tabla Usuario (seed de la migración)

$ curl -s -H "Authorization: Bearer <token>" http://localhost:8080/api/tipos-turno
[{"id":1,"nombre":"Mañana",...}, {"id":2,"nombre":"Tarde",...}, {"id":3,"nombre":"Noche",...}]
HTTP 200

$ curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/
200                                                # frontend servido por nginx
```

CORS validado implícitamente: el frontend (`localhost:3000`) y el backend (`localhost:8080`) son
orígenes distintos y la app funciona porque `Frontend__Origin=http://localhost:3000` está en la
whitelist de `AddCors` (ver `decisiones.md`, sección Docker).

## 4. Prueba de persistencia (volumen `db_data`)

Se creó un empleado real por API (no un dato de seed, para que la prueba sea válida):

```
$ curl -s -X POST http://localhost:8080/api/empleados -d '{"nombre":"Test","apellido":"Persistencia","dni":"99999999",...}'
{"id":1,"nombre":"Test","apellido":"Persistencia",...}
HTTP 201
```

**`docker compose down` (sin `-v`) + `docker compose up -d`** — el contenedor de la base se destruye
y se recrea, el volumen no se toca:

```
$ curl -s -H "Authorization: Bearer <token>" http://localhost:8080/api/empleados
[{"id":1,"nombre":"Test","apellido":"Persistencia",...}]   # SIGUE — el volumen sobrevivió
```

**`docker compose down -v` + `docker compose up -d`** — esta vez se borra también el volumen:

```
$ curl -s -H "Authorization: Bearer <token>" http://localhost:8080/api/empleados
[]                                                           # VACÍO — datos perdidos junto con el volumen
```

Confirma la separación contenedor-efímero / estado-persistente: el contenedor de Postgres es
descartable, el volumen `pagina-ing3_db_data` es lo único que importa para no perder datos.

(Nota: los `tipos-turno` reaparecen incluso después de `down -v`, porque están cargados vía
`HasData` en la migración — son seed, no estado real. Por eso la prueba de arriba usa un empleado
creado por API en vez de esos datos.)

## 5. Volumen

```
$ docker volume ls | grep pagina-ing3
local     pagina-ing3_db_data

$ docker volume inspect pagina-ing3_db_data
# Driver: local — Mountpoint dentro de la VM de Docker Desktop (no visible en el filesystem del host en macOS)
```

## 6. Imágenes publicadas en el registry (ghcr.io)

Publicadas con tag semver `v0.1.0`:

- https://github.com/users/julietamelinsky01/packages/container/package/lasmelis-backend
- https://github.com/users/julietamelinsky01/packages/container/package/lasmelis-frontend

```
$ docker tag pagina-ing3-backend:latest ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0
$ docker tag pagina-ing3-frontend:latest ghcr.io/julietamelinsky01/lasmelis-frontend:v0.1.0
$ docker push ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0
v0.1.0: digest: sha256:e4db23b17daa119dc9cf8742cef2b3c964405fb028d225bcd996e7c37f33811c size: 856
$ docker push ghcr.io/julietamelinsky01/lasmelis-frontend:v0.1.0
v0.1.0: digest: sha256:782cb0dfdfa4fa6c5f2ad660806a1251924cc9f06db87c41ad293c8d767380c5 size: 856
```

Visibilidad cambiada a pública desde Package settings → Danger Zone → Change visibility, confirmada
contra la API:

```
$ curl -s https://api.github.com/user/packages/container/lasmelis-backend | jq .visibility
"public"
$ curl -s https://api.github.com/user/packages/container/lasmelis-frontend | jq .visibility
"public"
```

**Prueba real de que son públicas: pull sin estar logueada.** La primera prueba de `docker pull` dio
falso positivo — bajó "instantáneo" porque las capas ya estaban en el disco local (compartidas con
`pagina-ing3-backend:latest`, la imagen que ya había buildeado antes: es exactamente el cacheo de
capas del §2.3 de la guía). Para una prueba real hubo que borrar también esas imágenes locales:

```
$ docker logout ghcr.io
$ docker rmi pagina-ing3-backend:latest pagina-ing3-frontend:latest \
    ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0 ghcr.io/julietamelinsky01/lasmelis-frontend:v0.1.0

$ docker pull ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0
v0.1.0: Pulling from julietamelinsky01/lasmelis-backend
5d83a6863e6a: Pull complete
b3470b04f6c6: Pull complete
Status: Downloaded newer image for ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0
```

Sin `docker login` de por medio: confirma que el pull anónimo funciona, es decir, que el package es
público de verdad. (Dos capas seguían marcadas `Already exists`: son las de la imagen base
`aspnet:8.0` que ya estaba en el disco de antes — caché de capas compartidas entre imágenes
distintas que parten de la misma base, tal como describe el §2.3 de la guía; no invalida la prueba,
porque esas capas no son específicas de esta imagen.)

## 7. `docker-compose.registry.yml` probado de punta a punta

Sin buildear nada local, solo bajando las imágenes publicadas:

```
$ cp .env.example .env
$ docker compose -f docker-compose.registry.yml up -d
 Container pagina-ing3-db-1        Started
 Container pagina-ing3-backend-1   Started
 Container pagina-ing3-frontend-1  Started

$ docker compose -f docker-compose.registry.yml ps
NAME                     IMAGE                                                 STATUS
pagina-ing3-backend-1    ghcr.io/julietamelinsky01/lasmelis-backend:v0.1.0    Up (healthy dependency)
pagina-ing3-db-1         postgres:16-alpine                                    Up (healthy)
pagina-ing3-frontend-1   ghcr.io/julietamelinsky01/lasmelis-frontend:v0.1.0   Up

$ curl -s -X POST http://localhost:8080/api/auth/login -d '{"username":"admin","password":"Havanna2026!"}'
{"token":"eyJhbGciOiJIUzI1NiIs..."}

$ curl -s -H "Authorization: Bearer <token>" http://localhost:8080/api/tipos-turno
[{"id":1,"nombre":"Mañana",...}, {"id":2,"nombre":"Tarde",...}, {"id":3,"nombre":"Noche",...}]
HTTP 200

$ curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/
200
```

Sistema completo funcionando end-to-end (login, JWT, CORS, frontend) corriendo enteramente desde
imágenes publicadas, sin un solo `docker build` local.
