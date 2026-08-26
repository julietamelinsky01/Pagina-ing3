# ---- Etapa 1: build ----
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

# La app usa URL absoluta (VITE_API_URL) en vez del proxy relativo /api de nginx
# (ver decisiones.md, sección Docker) — Vite la resuelve en BUILD TIME, así que
# tiene que llegar como build arg, no como variable de entorno del contenedor final.
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Etapa 2: nginx sirve los estáticos ----
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
