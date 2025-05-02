# TaskManager API

API REST para administrar tareas, con autenticación de usuarios y manejo de tareas personales.

## Decisiones tomadas:
- Se decide usar base de datos relacional para generar la relacion entre tarea y usuario (la tarea será del usuario creador)
- Aunque el enunciado no establece reglas específicas sobre los permisos entre usuarios, decidí implementar la posibilidad de que cualquier usuario autenticado pueda editar o eliminar tareas, incluso si no es su creador ya que creo que eso es lo que le da el enfoque colaborativo que se menciona en el challenge (mismo cuando se menciona crear, editar, eliminar no dice que solo las suyas), se toma como referencia trello/notion que los miembros de un equipo pueden gestionar tareas ajenas.
- Si fuera necesario este comportamiento podría modificarse fácilmente agregando validaciones de propiedad/ownership (de hecho para hacer mas consistente se agrega modelo y registro de historial pero no se termina aplicando en front por temas de tiempo), la tabla tasks tiene campos OwnerId,Priority pensados en caso de mejoras.
- Para este mvp cualquier usuario logueado puede administrar tareas
- Los dominios de la aplicacion en Controller, Service, y Repository (en caso de interaccion con una api externa podriamos agregar una capa core/client), esto hace que la aplicación sea fácil de extender y modificar sin tener que afectar a otras funcionalidades. 
- Aplicamos inversión de dependencias para favorecer el desacoplamiento entre componentes. 
- Se realizan pruebas unitarias sobre la capa service que suele ser donde se maneja la lógica de negocio principal, se incluyeron pruebas de integración para validar el funcionamiento de la API de forma completa.
- Se eligió C# .NET aunque el enunciado sugiere implementar el backend en Java, Kotlin o Python porque tengo mas experiencia con el mismo lo que me permite implementar una solución más prolija, manteniendo buenas prácticas como la inyección de dependencias, separación de responsabilidades y uso de DTOs y validadores (aunque todo lo mencionado no es exlusivo del lenguaje y de hecho en algunas partes se puede mejorar, se intentó entregar un mvp lo mas prolijo posible).
- Al igual que Java o Kotlin, C# es fuertemente tipado, lo que ayuda a prevenir muchos errores en tiempo de compilación.

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
2. **Instalar .NET EF Core tools (solo la primera vez)**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Levantar la base de datos SQL Server con Docker**

   El proyecto incluye un archivo `docker-compose.yml` para facilitar la creación de la instancia de SQL Server:

   ```bash
   docker-compose up -d sqlserver
   ```

   Esto levantará un contenedor de SQL Server en el puerto 14333 con las siguientes credenciales:
   - Usuario: sa
   - Contraseña: YourStrong!Passw0rd
   - Base de datos: TaskManagerDb (se creará automáticamente al ejecutar la aplicación)

4. **Ejecutar migraciones**
   ```bash
   # En Linux/Mac
   chmod +x init-db.sh
   ./init-db.sh

   # O manualmente
   cd src/TaskManager.API
   dotnet ef database update
   ```

5. **Levantar la API**
   ```bash
   docker-compose up -d api
   ```
   La API estará disponible en: 
   - https://localhost:5102

   Puedes acceder a la documentación de Swagger en: http://localhost:5102/swagger


## Ejecutar las pruebas

El proyecto incluye pruebas unitarias y de integración que puedes ejecutar fácilmente. Las pruebas de integración utilizan una base de datos en memoria, por lo que no necesitas tener SQL Server configurado específicamente para ellas.

### Requisitos para ejecutar tests:
- .NET SDK 8.0 instalado en tu máquina local

### Pasos para ejecutar las pruebas:

```bash
# Ejecutar todas las pruebas
dotnet test

# O navegar al directorio de pruebas
cd Tests/TaskManager.Tests
dotnet test
```

### Tipos de pruebas incluidas:

1. **Pruebas unitarias**: Verifican componentes individuales como servicios usando mocks para sus dependencias.
2. **Pruebas de integración**: Prueban la interacción entre componentes usando una base de datos en memoria.

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

1. Verifica que los contenedores Docker estén corriendo correctamente:
   ```bash
   docker ps
   ```

2. Verifica los logs del contenedor de SQL Server:
   ```bash
   docker logs taskmanager_sqlserver
   ```

3. Si estás ejecutando la API localmente (sin Docker), asegúrate de que la cadena de conexión en `appsettings.json` use `localhost`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,14333;Database=TaskManagerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
   }
   ```

4. Si estás usando la API en Docker, la cadena de conexión debe usar el nombre del servicio:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=sqlserver,1433;Database=TaskManagerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
   }
   ```

### Problemas con la API en Docker

1. Verifica los logs de la API:
   ```bash
   docker logs taskmanager_api
   ```

2. Si hay problemas de CORS al acceder desde el frontend:
   - Asegúrate de que tu frontend esté usando `http://localhost:5102` (o el puerto correcto)
   - Verifica que en `Program.cs` esté configurado correctamente el origen permitido:
     ```csharp
     policy.WithOrigins("http://localhost:7000") // Ajusta según el puerto de tu frontend
     ```

3. Si los contenedores no pueden comunicarse entre sí:
   - Asegúrate de que están en la misma red de Docker
   - Verifica que la API está esperando a que SQL Server esté disponible

### Errores en las migraciones

1. Si el script `init-db.sh` falla:
   ```bash
   # Ejecuta manualmente las migraciones
   cd src/TaskManager.API
   dotnet ef database update
   ```

2. Si las migraciones no pueden conectarse a la base de datos:
   - Verifica que SQL Server esté en ejecución
   - Asegúrate de que la cadena de conexión sea correcta para el entorno local

### Errores de compilación o ejecución

1. Para problemas con la API local:
   - Asegúrate de tener instalado .NET 8.0 SDK:
     ```bash
     dotnet --version
     ```
   - Limpia la solución y reconstruye:
     ```bash
     dotnet clean
     dotnet build
     ```

2. Para problemas con la imagen Docker:
   - Reconstruye la imagen:
     ```bash
     docker-compose build api
     ```
   - Reinicia los contenedores:
     ```bash
     docker-compose down
     docker-compose up -d
     ```

3. Para reiniciar toda la aplicación desde cero:
   ```bash
   docker-compose down -v  # El flag -v elimina los volúmenes (¡cuidado! borrará los datos)
   docker-compose up -d
   ```

Esta sección actualizada cubre los problemas más comunes que pueden surgir en el entorno dockerizado y proporciona soluciones específicas para cada caso, distinguiendo entre ejecución local y en contenedores.

## Tecnologías utilizadas

- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- Docker
- xUnit (para pruebas) 