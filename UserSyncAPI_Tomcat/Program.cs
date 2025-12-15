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
            try
            {

                Logger.Log($"Main Starting.");


                //var builder = WebApplication.CreateBuilder(args);
                var options = new WebApplicationOptions
                {
                    ContentRootPath = AppContext.BaseDirectory,  // Ensures correct path when run as a service
                    EnvironmentName = Environments.Production,   // or use "Development" / "Staging"
                    ApplicationName = typeof(Program).Assembly.FullName,
                    Args = args
                };
                Logger.Log($"options initialised.");

                var builder = WebApplication.CreateBuilder(options);
                Logger.Log($"builder CreateBuilder.");

                //  Add Windows Service integration
                builder.Host.UseWindowsService();
                Logger.Log($"builder host UseWindowsService.");

                // Add services to the container
                builder.Services.AddControllers();
                Logger.Log($"builder Services AddControllers.");

                // Swagger
                builder.Services.AddEndpointsApiExplorer();
                Logger.Log($"builder Services AddEndpointsApiExplorer.");

                builder.Services.AddSwaggerGen();
                Logger.Log($"builder Services AddSwaggerGen.");


                // Bind BasicAuth section from appsettings.json
                builder.Services.Configure<BasicAuthConfig>(
                    builder.Configuration.GetSection("BasicAuth")
                );
                Logger.Log($"builder Services config BasicAuth.");

                //Reading connection string
                DbConnectionFactory.Init(builder.Configuration);
                Logger.Log($"builder Configuration passed as constructor to DB connection.");

                builder.Services.AddScoped<ValidateModelAttribute>();
                Logger.Log($"builder Services ValidateModelAttribute.");

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<ResponseLoggingFilter>();
                });
                Logger.Log($"builder Services ResponseLoggingFilter.");



                //logging settings
                // builder.Services.Configure<LoggingSettings>(builder.Configuration.GetSection("LoggingSettings"));
                builder.Services.Configure<LdapSettings>(builder.Configuration.GetSection("LdapSettings"));
                Logger.Log($"builder Services reading LDAP settings.");


                // Register Basic Authentication
                builder.Services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                        "BasicAuthentication", null);
                Logger.Log($"builder Services AddAuthentication BasicAuthentication");

                builder.Services.AddAuthorization();
                Logger.Log($"builder Services AddAuthorization ");

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

                Logger.Log($"builder Services AddCors");


                var app = builder.Build();
                Logger.Log($"builder builder Build app");

                // Configure the HTTP request pipeline
                //if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                Logger.Log($"app Swagger");

                app.UseStaticFiles(); // <-- IMPORTANT
                Logger.Log($"app static files");

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "Logs")),
                    RequestPath = "/Logs"
                });
                Logger.Log($"app Logs folder added.");

                app.UseRouting();
                Logger.Log($"app.UseRouting");

                app.UseAuthentication();
                Logger.Log($"app.UseAuthentication");

                app.UseAuthorization();
                Logger.Log($"app.UseAuthorization");

                app.UseMiddleware<RequestResponseLoggingMiddleware>();
                Logger.Log($"app.UseMiddleware");

                app.UseCors("AllowAll");
                Logger.Log($"app.AllowAll");


                app.MapControllers();
                Logger.Log($"app.MapControllers()");

                app.Run();
                Logger.Log($"app.Run()");
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                Logger.Log($"Startup FAILURE. \n\t {errorDetails}");
            }

        }
    }
}
