FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar los archivos de proyecto y restaurar dependencias
COPY src/TaskManager.API/*.csproj ./src/TaskManager.API/
COPY *.sln .
RUN dotnet restore src/TaskManager.API/TaskManager.API.csproj

# Copiar el resto del código y compilar
COPY src/. ./src/
WORKDIR /app/src/TaskManager.API
RUN dotnet publish -c Release -o out

# Construir la imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/TaskManager.API/out ./

# Exponer el puerto en el que se ejecuta la aplicación
EXPOSE 80
EXPOSE 443

# Configurar el punto de entrada
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]