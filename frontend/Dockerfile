FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG VITE_DEALER_NAME="Auto Sallon Luki"
ARG VITE_DEALER_PHONE=""
ARG VITE_DEALER_WHATSAPP=""
ARG VITE_DEALER_ADDRESS=""
ARG VITE_TURNSTILE_SITE_KEY=""
ARG VITE_DEMO_MODE="false"
ENV VITE_DEALER_NAME=$VITE_DEALER_NAME \
    VITE_DEALER_PHONE=$VITE_DEALER_PHONE \
    VITE_DEALER_WHATSAPP=$VITE_DEALER_WHATSAPP \
    VITE_DEALER_ADDRESS=$VITE_DEALER_ADDRESS \
    VITE_TURNSTILE_SITE_KEY=$VITE_TURNSTILE_SITE_KEY \
    VITE_DEMO_MODE=$VITE_DEMO_MODE

RUN npm run build -- --mode production

FROM nginx:1.29-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY security-headers.conf /etc/nginx/security-headers.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
