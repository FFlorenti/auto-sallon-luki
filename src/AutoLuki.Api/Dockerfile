FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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

RUN mkdir -p /app/uploads && chown $APP_UID /app/uploads

COPY --from=build /app .

USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoLuki.Api.dll"]
