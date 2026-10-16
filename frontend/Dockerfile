# ---- Etapa 1: build ----
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

RUN npm run build

# ---- Etapa 2: tests (parte del build: ya tiene Node y las dependencias de desarrollo) ----
# Corren en `docker run`, no al construir. `test:ci` mide cobertura y, si baja del umbral
# declarado en vite.config.js (thresholds), sale con error aunque todos los tests pasen.
FROM build AS test
ENTRYPOINT ["npm", "run", "test:ci"]

# ---- Etapa 3: nginx sirve los estáticos (queda ÚLTIMA: es la imagen que se despliega) ----
FROM nginx:alpine
# nginx procesa los archivos de /etc/nginx/templates/ al arrancar y reemplaza ${VARIABLE}
# por su valor: la dirección del backend entra por el ENTORNO, no queda escrita en la imagen.
# Los valores por defecto son los de docker compose; en QA/PROD se pisan con variables.
COPY default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html
ENV BACKEND_URL=http://backend:8080 \
    DNS_RESOLVER=127.0.0.11
EXPOSE 80
