//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Options;
//using System.Net.Http.Headers;
//using System.Security.Claims;
//using System.Text;
//using System.Text.Encodings.Web;
//using UserSyncAPI_Tomcat.Models;
//namespace UserSyncAPI_Tomcat.Authentication
//{
//    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
//    {
//        private readonly BasicAuthConfig _authConfig;

//        public BasicAuthenticationHandler(
//            IOptionsMonitor<AuthenticationSchemeOptions> options,
//            ILoggerFactory logger,
//            UrlEncoder encoder,
//            ISystemClock clock,
//        IOptions<BasicAuthConfig> authConfigOptions)
//            : base(options, logger, encoder, clock) { _authConfig = authConfigOptions.Value; }

//        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
//        {


//            if (!Request.Headers.ContainsKey("Authorization"))
//                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

//            try
//            {
//                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
//                if (authHeader.Scheme != "Basic")
//                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));

//                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
//                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
//                if (credentials.Length != 2)
//                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));

//                var username = credentials[0];
//                var password = credentials[1];

//                // TODO: Replace with your actual user validation logic (DB, config, etc.)
//                if (username != _authConfig.Username || password != _authConfig.Password)
//                    return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

//                var claims = new[] { new Claim(ClaimTypes.Name, username) };
//                var identity = new ClaimsIdentity(claims, Scheme.Name);
//                var principal = new ClaimsPrincipal(identity);
//                var ticket = new AuthenticationTicket(principal, Scheme.Name);

//                return Task.FromResult(AuthenticateResult.Success(ticket));
//            }
//            catch
//            {
//                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
//            }
//        }
//    }
//}



using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using UserSyncAPI_Tomcat.Models;
namespace UserSyncAPI_Tomcat.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly BasicAuthConfig _authConfig;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<BasicAuthConfig> authConfigOptions)
            : base(options, logger, encoder, clock)
        {
            _authConfig = authConfigOptions.Value;
        }

        private string _failureReason = "";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                _failureReason = "Missing Authorization Header";
                return Task.FromResult(AuthenticateResult.Fail(_failureReason));
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization);
                if (authHeader.Scheme != "Basic")
                {
                    _failureReason = "Invalid Authorization Scheme";
                    return Task.FromResult(AuthenticateResult.Fail(_failureReason));
                }

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                if (credentials.Length != 2)
                {
                    _failureReason = "Invalid Authorization Header";
                    return Task.FromResult(AuthenticateResult.Fail(_failureReason));
                }

                var username = credentials[0];
                var password = credentials[1];

                if (username != _authConfig.Username || password != _authConfig.Password)
                {
                    _failureReason = "Invalid Username or Password";
                    return Task.FromResult(AuthenticateResult.Fail(_failureReason));
                }

                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                _failureReason = "Invalid Authorization Header Format";
                return Task.FromResult(AuthenticateResult.Fail(_failureReason));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.ContentType = "application/json";

            ApiResponse<EmptyData> responseObj = new ApiResponse<EmptyData>
            {
                Success = false,
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Status = HttpStatusCode.Unauthorized.ToString(),
                Message = _failureReason, // exact reason
                Error = UserSyncAPI_Tomcat.Common.Constants.Errors.ERR_UNAUTHORIZED,
                Data = new EmptyData(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(responseObj);
            await Response.WriteAsync(json);
        }
    }
}