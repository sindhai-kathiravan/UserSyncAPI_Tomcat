using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Filter;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Middleware;
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
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ResponseLoggingFilter>();
            });



            //logging settings
            // builder.Services.Configure<LoggingSettings>(builder.Configuration.GetSection("LoggingSettings"));
            builder.Services.Configure<LdapSettings>(builder.Configuration.GetSection("LdapSettings"));


            // Register Basic Authentication
            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                    "BasicAuthentication", null);

            builder.Services.AddAuthorization();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles(); // <-- IMPORTANT
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
        Path.Combine(AppContext.BaseDirectory, "Logs")),
                RequestPath = "/Logs"
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
