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
                //Logger.Log($"Main Starting.");

                var options = new WebApplicationOptions
                {
                    ContentRootPath = AppContext.BaseDirectory,
                    EnvironmentName = Environments.Production,
                    ApplicationName = typeof(Program).Assembly.FullName,
                    Args = args
                };
                //Logger.Log($"options initialised.");

                var builder = WebApplication.CreateBuilder(options);
                //Logger.Log($"builder CreateBuilder.");

                // Windows Service
                builder.Host.UseWindowsService();
                //Logger.Log($"builder host UseWindowsService.");

                // Services
                builder.Services.AddControllers();
                //Logger.Log($"builder Services AddControllers.");

                builder.Services.AddEndpointsApiExplorer();
                //Logger.Log($"builder Services AddEndpointsApiExplorer.");

                builder.Services.AddSwaggerGen();
                //Logger.Log($"builder Services AddSwaggerGen.");

                builder.Services.Configure<BasicAuthConfig>(
                    builder.Configuration.GetSection("BasicAuth"));
                //Logger.Log($"builder Services config BasicAuth.");

                DbConnectionFactory.Init(builder.Configuration);
                //Logger.Log($"builder Configuration passed as constructor to DB connection.");

                builder.Services.AddScoped<ValidateModelAttribute>();
                //Logger.Log($"builder Services ValidateModelAttribute.");

                builder.Services.AddControllers();
                //Logger.Log($"builder Services ResponseLoggingFilter.");

                builder.Services.Configure<LdapSettings>(
                    builder.Configuration.GetSection("LdapSettings"));
                //Logger.Log($"builder Services reading LDAP settings.");

                builder.Services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                        "BasicAuthentication", null);
                //Logger.Log($"builder Services AddAuthentication BasicAuthentication");

                builder.Services.AddAuthorization();
                //Logger.Log($"builder Services AddAuthorization ");

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });
                //Logger.Log($"builder Services AddCors");

                var app = builder.Build();
                //Logger.Log($"builder builder Build app");

                // -----------------------------
                // HTTP PIPELINE (RE-ORDERED)
                // -----------------------------

                app.UseSwagger();
                app.UseSwaggerUI();
                //Logger.Log($"app Swagger");

                app.UseStaticFiles();
                //Logger.Log($"app static files");

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(AppContext.BaseDirectory, "Logs")),
                    RequestPath = "/Logs"
                });
                //Logger.Log($"app Logs folder added.");

                app.UseRouting();
                //Logger.Log($"app.UseRouting");

               // app.UseMiddleware<RequestResponseLoggingMiddleware>();


                //  CORS must be before auth
                app.UseCors("AllowAll");
                //Logger.Log($"app.AllowAll");

                app.UseAuthentication();
                //Logger.Log($"app.UseAuthentication");

                app.UseAuthorization();
                //Logger.Log($"app.UseAuthorization");

                //  Logging middleware AFTER routing & auth, BEFORE endpoints
                //app.UseMiddleware<RequestResponseLoggingMiddleware>();
                //Logger.Log($"app.UseMiddleware");

                app.MapControllers();
                //Logger.Log($"app.MapControllers()");

                app.Run();
                //Logger.Log($"app.Run()");
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                //Logger.Log($"Startup FAILURE. \n\t {errorDetails}");
            }
        }

    }
}
