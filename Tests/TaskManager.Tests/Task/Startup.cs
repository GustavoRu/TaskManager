using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManager.API.Shared.Data;
using TaskManager.API.Shared.Utils;
using TaskManager.API.Auth;
using TaskManager.API.Task;
using TaskManager.API.Task.Validators;

namespace TaskManager.Tests.Task
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //DbContext using in-memory database 
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: "TestDb"));

            // JWT Configuration
            var jwtSecret = Configuration["JWT:Secret"] ?? "your-test-secret-key-must-be-at-least-16-characters-long";
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // core services
            services.AddSingleton<IJwtUtility>(provider => new JwtUtility(Configuration));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITaskService, TaskService>();

            // repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();

            // validators
            services.AddScoped<RegisterDtoValidator>();
            services.AddScoped<LoginDtoValidator>();
            services.AddScoped<TaskPostDtoValidator>();
            services.AddScoped<TaskUpdateDtoValidator>();

            // User context
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();

            services.AddControllers()
                .AddApplicationPart(typeof(TaskManager.API.Task.TaskController).Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
} 