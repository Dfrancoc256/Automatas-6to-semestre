# Build de Nuxt.
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Imagen de producción.
FROM node:20-alpine AS final
WORKDIR /app

ENV NODE_ENV=production
ENV NITRO_HOST=0.0.0.0
ENV NITRO_PORT=3000

COPY --from=build --chown=node:node /app/.output ./.output
USER node

EXPOSE 3000
CMD ["node", ".output/server/index.mjs"]
