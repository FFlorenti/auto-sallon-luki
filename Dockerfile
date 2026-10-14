FROM node:24-alpine AS web
WORKDIR /web

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ .

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

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src

COPY Directory.Build.props ./
COPY src/AutoLuki.Domain/AutoLuki.Domain.csproj src/AutoLuki.Domain/
COPY src/AutoLuki.Application/AutoLuki.Application.csproj src/AutoLuki.Application/
COPY src/AutoLuki.Infrastructure/AutoLuki.Infrastructure.csproj src/AutoLuki.Infrastructure/
COPY src/AutoLuki.Api/AutoLuki.Api.csproj src/AutoLuki.Api/
RUN dotnet restore src/AutoLuki.Api/AutoLuki.Api.csproj -r linux-x64

COPY src/ src/
RUN dotnet publish src/AutoLuki.Api/AutoLuki.Api.csproj -c Release -o /app --no-restore -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=api /app .
COPY --from=web /web/dist ./wwwroot

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "AutoLuki.Api.dll"]
