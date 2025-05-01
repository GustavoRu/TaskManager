using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using TaskManager.API.Shared.Data;
using Microsoft.Extensions.Configuration;
using TaskManager.API;

namespace TaskManager.Tests.Task
{
    public class IntegrationTestBase : IDisposable
    {
        protected readonly TestServer _server;
        protected readonly HttpClient _client;
        protected readonly ApplicationDbContext _context;
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public IntegrationTestBase()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["JWT:Secret"] = "your-test-secret-key-must-be-at-least-16-characters-long"
                })
                .Build();

            var builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseStartup<Startup>();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
            _context = _server.Services.GetRequiredService<ApplicationDbContext>();

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    _context.Database.EnsureCreated();
                    _databaseInitialized = true;
                }
            }
            CleanDatabase();
        }

        protected void CleanDatabase()
        {
            _context.TaskHistories.RemoveRange(_context.TaskHistories);
            _context.Tasks.RemoveRange(_context.Tasks);
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
            _context.Dispose();
        }
    }
} 