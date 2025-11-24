using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models; // update this with your actual namespace


namespace UserSyncAPI_Tomcat
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //var builder = WebApplication.CreateBuilder(args);
            var options = new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,  // Ensures correct path when run as a service
                EnvironmentName = Environments.Production,   // or use "Development" / "Staging"
                ApplicationName = typeof(Program).Assembly.FullName,
                Args = args
            };

            var builder = WebApplication.CreateBuilder(options);
            //  Add Windows Service integration
            builder.Host.UseWindowsService();

            // Add services to the container
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Bind BasicAuth section from appsettings.json
            builder.Services.Configure<BasicAuthConfig>(
                builder.Configuration.GetSection("BasicAuth")
            );
            //Reading connection string
            DbConnectionFactory.Init(builder.Configuration);

            builder.Services.AddScoped<ValidateModelAttribute>();

            //logging settings
            // builder.Services.Configure<LoggingSettings>(builder.Configuration.GetSection("LoggingSettings"));
            builder.Services.Configure<LdapSettings>(builder.Configuration.GetSection("LdapSettings"));


            // Register Basic Authentication
            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                    "BasicAuthentication", null);

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
