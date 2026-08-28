FROM node:22-slim AS builder
WORKDIR /client
RUN npm install -g npm@11.6.2
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/. .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=builder /client/dist/FoodDelivery-client/browser /usr/share/nginx/html
EXPOSE 80

