using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TaskManager.API.Auth;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.Shared.Data;
using Xunit;

namespace TaskManager.Tests.Integration
{
    public class AuthControllerIntegrationTests
    {
        // Método para crear una nueva instancia de WebApplicationFactory con una base de datos en memoria única
        private WebApplicationFactory<Program> CreateWebApplicationFactory()
        {
            // Generamos un GUID único para cada test
            string dbTest = $"TestTaskManagerDb_{Guid.NewGuid()}";

            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Encontramos el descriptor del servicio para el DbContext
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    // Removemos el registro del DbContext real
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }

                    // Agregamos el DbContext en memoria con un nombre único
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbTest);
                    });

                    // Creamos un service provider
                    var serviceProvider = services.BuildServiceProvider();

                    // Creamos un scope para usar el DbContext
                    using var scope = serviceProvider.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // Aseguramos que la base de datos esté creada
                    db.Database.EnsureCreated();

                    // Aquí se podrían sembrar datos iniciales si fuera necesario
                    // SeedDatabase(db);
                });
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange - Creamos una nueva factory para este test
            var factory = CreateWebApplicationFactory();
            var client = factory.CreateClient();

            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            ((bool)result!.isSuccess).Should().BeTrue();

            // Limpieza
            await factory.DisposeAsync();
        }

        [Fact]
        public async System.Threading.Tasks.Task Register_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange - Creamos una nueva factory para este test
            var factory = CreateWebApplicationFactory();
            var client = factory.CreateClient();

            var registerDto = new RegisterDto
            {
                Name = "",  // Nombre vacío, debería fallar la validación
                Email = "invalid-email",
                Password = "short"
                
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Limpieza
            await factory.DisposeAsync();
        }

        // Prueba para el login exitoso
        [Fact]
        public async System.Threading.Tasks.Task Login_WithValidCredentials_ReturnsTokenAndUserId()
        {
            // Arrange, creamos una nueva factory para este test
            var factory = CreateWebApplicationFactory();
            var client = factory.CreateClient();

            // Primero registramos un usuario para poder hacer login
            var registerDto = new RegisterDto
            {
                Name = "Login Test User",
                Email = "login@example.com",
                Password = "Password123!"
            };
            await client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Preparamos las credenciales de login
            var loginDto = new LoginDto
            {
                Email = "login@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            ((bool)result!.isSuccess).Should().BeTrue();
            ((string)result.token).Should().NotBeNullOrEmpty();
            ((int)result.userId).Should().BeGreaterThan(0);

            // Limpieza
            await factory.DisposeAsync();
        }

        [Fact]
        public async System.Threading.Tasks.Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange, creamos una nueva factory para este test
            var factory = CreateWebApplicationFactory();
            var client = factory.CreateClient();

            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ((bool)result!.isSuccess).Should().BeFalse();

            // Limpieza
            await factory.DisposeAsync();
        }

        // Helper method para enviar peticiones JSON sin usar PostAsJsonAsync
        private StringContent GetJsonStringContent(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json");
        }
    }
}