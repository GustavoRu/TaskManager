# TaskManager API

API REST para administrar tareas, con autenticación de usuarios y manejo de tareas personales.

## Requisitos previos

Para ejecutar esta aplicación, necesitarás tener instalado:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Configuración inicial

1. **Clonar el repositorio**

   ```bash
   git clone <url-del-repositorio>
   cd TaskManager
   ```

2. **Levantar la base de datos SQL Server con Docker**

   El proyecto incluye un archivo `docker-compose.yml` para facilitar la creación de la instancia de SQL Server:

   ```bash
   docker-compose up -d
   ```

   Esto levantará un contenedor de SQL Server en el puerto 14333 con las siguientes credenciales:
   - Usuario: sa
   - Contraseña: YourStrong!Passw0rd
   - Base de datos: TaskManagerDb (se creará automáticamente al ejecutar la aplicación)

3. **Restaurar las dependencias del proyecto**

   ```bash
   dotnet restore
   ```

## Ejecutar la aplicación

1. **Navegar al directorio del proyecto API**

   ```bash
   cd src/TaskManager.API
   ```

2. **Ejecutar migraciones (primera vez)**

   Si es la primera vez que ejecutas la aplicación, necesitas aplicar las migraciones para crear el esquema de la base de datos:

   ```bash
   dotnet ef database update
   ```

3. **Iniciar la API**

   ```bash
   dotnet run
   ```

   La API estará disponible en: 
   - https://localhost:5102

   Puedes acceder a la documentación de Swagger en: http://localhost:5102/swagger

## Ejecutar las pruebas

El proyecto incluye pruebas unitarias e integración que puedes ejecutar con los siguientes comandos:

```bash
cd Tests/TaskManager.Tests
dotnet test
```

## Características principales

- **Autenticación**: Registro de usuarios y login con tokens JWT
- **Gestión de tareas**: Crear, leer, actualizar y eliminar tareas
- **Validación de datos**: Validación de entradas mediante FluentValidation
- **Documentación API**: Documentación automática con Swagger

## Estructura del proyecto

- `src/TaskManager.API`: Proyecto principal de la API
  - `Auth`: Controladores y servicios de autenticación
  - `Task`: Controladores y servicios de gestión de tareas
  - `User`: Modelos y servicios relacionados con usuarios
  - `Shared`: Componentes compartidos (utilidades, modelos, etc.)

- `Tests/TaskManager.Tests`: Proyecto de pruebas
  - `Auth`: Pruebas de autenticación
  - `Task`: Pruebas de gestión de tareas

## Solución de problemas comunes

### Error de conexión a la base de datos

Si tienes problemas para conectar con la base de datos:

1. Verifica que el contenedor Docker esté corriendo:
   ```bash
   docker ps
   ```

2. Asegúrate de que la cadena de conexión en `appsettings.json` coincida con la configuración del contenedor:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,14333;Database=TaskManagerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
   }
   ```

### Errores de compilación o ejecución

- Asegúrate de tener instalado .NET 8.0 SDK:
  ```bash
  dotnet --version
  ```

- Limpia la solución y reconstruye:
  ```bash
  dotnet clean
  dotnet build
  ```

## Tecnologías utilizadas

- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- Docker
- xUnit (para pruebas) 