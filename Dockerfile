# ---------- Etapa 1: compilar ----------
# La imagen del SDK trae el compilador y las herramientas de .NET (~800 MB).
# Solo se usa para construir; no llega a la imagen final.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj: si no cambian los paquetes, Docker reutiliza
# la capa del restore y no vuelve a descargar todo en cada build.
COPY src/FerreAPI/FerreAPI.csproj src/FerreAPI/
RUN dotnet restore src/FerreAPI/FerreAPI.csproj

# Después el resto del código, que es lo que cambia con frecuencia.
COPY src/FerreAPI/ src/FerreAPI/
RUN dotnet publish src/FerreAPI/FerreAPI.csproj -c Release -o /app/publish --no-restore

# ---------- Etapa de migraciones ----------
# Genera un "bundle": un ejecutable que solo aplica las migraciones pendientes.
FROM build AS bundle
RUN dotnet tool install --global dotnet-ef --version 10.0.13
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --project src/FerreAPI/FerreAPI.csproj \
    --configuration Release --output /app/efbundle

# Imagen que compose ejecuta una vez antes de la API: migra y termina.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migraciones
WORKDIR /app
COPY --from=bundle /app/efbundle .
USER $APP_UID
ENTRYPOINT ["./efbundle"]

# ---------- Etapa 2: ejecutar ----------
# Va al final para que "docker build ." sin --target construya la API.
# La imagen de runtime solo trae lo necesario para correr la API (~230 MB).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Usuario sin privilegios que ya viene definido en las imágenes de .NET.
USER $APP_UID

# Las imágenes de .NET escuchan por defecto en el puerto 8080 (HTTP).
EXPOSE 8080

ENTRYPOINT ["dotnet", "FerreAPI.dll"]
