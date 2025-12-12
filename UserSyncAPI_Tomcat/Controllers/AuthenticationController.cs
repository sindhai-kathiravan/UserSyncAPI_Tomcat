using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models;
using UserSyncAPI_Tomcat.Security;

namespace UserSyncAPI_Tomcat.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class AuthenticationController : ControllerBase
    {
        private readonly LdapSettings _ldapSettings;

        public AuthenticationController(IOptions<LdapSettings> ldapOptions)
        {
            _ldapSettings = ldapOptions.Value;
        }
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            ApiResponse<object>? apiResponse = null;
            string? message = null;
            string? error = null;
            bool success = false;
            try
            {
                var config = new ConfigurationBuilder()
                                 .SetBasePath(AppContext.BaseDirectory)
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                 .Build();

                string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

                using (SqlConnection connection = new SqlConnection(cnxnStr))
                {
                    connection.Open();
                    var sql = @"
                               IF EXISTS (SELECT 1 FROM Users U    
                                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
                                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
                                    BEGIN
                                        UPDATE Users 
                                        SET user_loggedin = 1, lastlogin = GETDATE() 
                                        WHERE user_name = @UserName;
                                        SELECT 1;
                                    END
                                    ELSE
                                    BEGIN
                                        UPDATE Users 
                                        SET fattempt = fattempt + 1 
                                        WHERE user_name = @UserName;
                                        SELECT 0;
                                    END";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", request.Username);
                        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
                        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

                        int result = (int)command.ExecuteScalar();
                        if (result == 1)
                        {
                            success = true;
                            message = Common.Constants.Messages.USER_DATABASE_AUTHENTICATION_SUCCESSFUL;
                        }
                        else
                        {
                            success = false;
                            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SOURCE_SYSTEM;
                            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
                        }
                    }
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = success,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = message,
                    Data = null,
                    Error = error,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);

            }
            catch (Exception ex)
            {
                Logger.Log($"Error in Login: {ex.Message}");
                Logger.Log($"StackTrace in Login: {ex.StackTrace}");
                apiResponse = new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Status = HttpStatusCode.InternalServerError.ToString(),
                    Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
                    Data = null,
                    Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            }
        }

        //[HttpPost]
        //[Route("domain-login")]
        //public IActionResult DomainLogin([FromBody] LoginRequest request)
        //{
        //    ApiResponse<object>? apiResponse = null;
        //    string? message = null;
        //    string? error = null;
        //    bool success = false;
        //    try
        //    {
        //        var ldapServer = _ldapSettings.Server;
        //        var credential = new NetworkCredential(request.Username, request.Password);
        //        using var connection = new LdapConnection(ldapServer);
        //        connection.Credential = credential;
        //        connection.Bind();


        //        //var config = new ConfigurationBuilder()
        //        //                 .SetBasePath(AppContext.BaseDirectory)
        //        //                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        //                 .Build();

        //        //string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

        //        //using (SqlConnection connection = new SqlConnection(cnxnStr))
        //        //{
        //        //    connection.Open();
        //        //    var sql = @"
        //        //               IF EXISTS (SELECT 1 FROM Users U    
        //        //                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
        //        //                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
        //        //                    BEGIN
        //        //                        UPDATE Users 
        //        //                        SET user_loggedin = 1, lastlogin = GETDATE() 
        //        //                        WHERE user_name = @UserName;
        //        //                        SELECT 1;
        //        //                    END
        //        //                    ELSE
        //        //                    BEGIN
        //        //                        UPDATE Users 
        //        //                        SET fattempt = fattempt + 1 
        //        //                        WHERE user_name = @UserName;
        //        //                        SELECT 0;
        //        //                    END";
        //        //    using (var command = new SqlCommand(sql, connection))
        //        //    {
        //        //        command.Parameters.AddWithValue("@UserName", request.Username);
        //        //        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
        //        //        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

        //        //        int result = (int)command.ExecuteScalar();
        //        //        if (result == 1)
        //        //        {
        //        //            success = true;
        //        //            message = Common.Constants.Messages.USER_AUTHENTICATION_SUCCESSFUL;
        //        //        }
        //        //        else
        //        //        {
        //        //            success = false;
        //        //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
        //        //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
        //        //        }
        //        //    }
        //        //}
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = success,
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Status = HttpStatusCode.OK.ToString(),
        //            Message = message,
        //            Data = null,
        //            Error = error,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(StatusCodes.Status200OK, apiResponse);
        //    }
        //    catch (LdapException ex)
        //    {
        //        Logger.Log($"Error in DomainLogin: {ex.Message}");
        //        Logger.Log($"StackTrace in DomainLogin: {ex.StackTrace}");
        //        var status= HttpStatusCode.InternalServerError.ToString();
        //        var statusCode = (int)HttpStatusCode.OK;
        //        success = false;
        //        if (ex.Message == "The supplied credential is invalid.")
        //        {
        //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
        //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
        //            status = HttpStatusCode.OK.ToString();
        //            statusCode = (int)HttpStatusCode.OK;
        //        }
        //        else
        //        {
        //            message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED;
        //            error = Common.Constants.Errors.ERR_INTERNAL_SERVER;
        //            status = HttpStatusCode.InternalServerError.ToString();
        //            statusCode = (int)HttpStatusCode.InternalServerError;
        //        }
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = success,
        //            StatusCode = statusCode,
        //            Status = status,
        //            Message = message,
        //            Data = null,
        //            Error = error,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(statusCode, apiResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log($"Error in Login: {ex.Message}");
        //        Logger.Log($"StackTrace in Login: {ex.StackTrace}");
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = false,
        //            StatusCode = (int)HttpStatusCode.InternalServerError,
        //            Status = HttpStatusCode.InternalServerError.ToString(),
        //            Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
        //            Data = null,
        //            Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
        //    }
        //}

        [HttpPost]
        [Route("domain-login")]
        public IActionResult DomainLogin([FromBody] LoginRequest request)
        {
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            ApiResponse<object>? apiResponse = null;
            string? error = null;
            Logger.Log($"{correlationId} > DomainLogin");

            bool adAvailable = IsADAvailable();

            if (adAvailable)
            {
                Logger.Log($"{correlationId} > AD Is Available");

                bool adSuccess = TryADAuthentication(request);

                if (adSuccess)
                {
                    Logger.Log($"{correlationId} > AD authentication successfull");

                    apiResponse = new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.USER_DOMAIN_AUTHENTICATION_SUCCESSFUL,
                        Data = null,
                        Error = error,
                        CorrelationId = correlationId
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
                else
                {
                    Logger.Log($"{correlationId} > Credentials are invalid, AD authentication FAILURE");

                    // AD reachable but login failed → Fallback to DB
                    apiResponse = new ApiResponse<object>
                    {
                        Success = false,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_DOMAIN,
                        Data = null,
                        Error = Common.Constants.Errors.ERR_LOGIN_FAILED,
                        CorrelationId = correlationId
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
            }
            else
            {
                Logger.Log($"{correlationId} > AD / Domain unavalable, trying for DB authentication.");

                // AD is DOWN → Direct DB login
                return Login(request);
            }
            //try
            //{
            //    var ldapServer = _ldapSettings.Server;
            //    var credential = new NetworkCredential(request.Username, request.Password);
            //    using var connection = new LdapConnection(ldapServer);
            //    connection.Credential = credential;
            //    connection.Bind();


            //    //var config = new ConfigurationBuilder()
            //    //                 .SetBasePath(AppContext.BaseDirectory)
            //    //                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    //                 .Build();

            //    //string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

            //    //using (SqlConnection connection = new SqlConnection(cnxnStr))
            //    //{
            //    //    connection.Open();
            //    //    var sql = @"
            //    //               IF EXISTS (SELECT 1 FROM Users U    
            //    //                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
            //    //                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
            //    //                    BEGIN
            //    //                        UPDATE Users 
            //    //                        SET user_loggedin = 1, lastlogin = GETDATE() 
            //    //                        WHERE user_name = @UserName;
            //    //                        SELECT 1;
            //    //                    END
            //    //                    ELSE
            //    //                    BEGIN
            //    //                        UPDATE Users 
            //    //                        SET fattempt = fattempt + 1 
            //    //                        WHERE user_name = @UserName;
            //    //                        SELECT 0;
            //    //                    END";
            //    //    using (var command = new SqlCommand(sql, connection))
            //    //    {
            //    //        command.Parameters.AddWithValue("@UserName", request.Username);
            //    //        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
            //    //        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

            //    //        int result = (int)command.ExecuteScalar();
            //    //        if (result == 1)
            //    //        {
            //    //            success = true;
            //    //            message = Common.Constants.Messages.USER_AUTHENTICATION_SUCCESSFUL;
            //    //        }
            //    //        else
            //    //        {
            //    //            success = false;
            //    //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
            //    //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
            //    //        }
            //    //    }
            //    //}
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = success,
            //        StatusCode = (int)HttpStatusCode.OK,
            //        Status = HttpStatusCode.OK.ToString(),
            //        Message = message,
            //        Data = null,
            //        Error = error,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(StatusCodes.Status200OK, apiResponse);
            //}
            //catch (LdapException ex)
            //{
            //    Logger.Log($"Error in DomainLogin: {ex.Message}");
            //    Logger.Log($"StackTrace in DomainLogin: {ex.StackTrace}");
            //    var status = HttpStatusCode.InternalServerError.ToString();
            //    var statusCode = (int)HttpStatusCode.OK;
            //    success = false;
            //    if (ex.Message == "The supplied credential is invalid.")
            //    {
            //        message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
            //        error = Common.Constants.Errors.ERR_LOGIN_FAILED;
            //        status = HttpStatusCode.OK.ToString();
            //        statusCode = (int)HttpStatusCode.OK;
            //    }
            //    else
            //    {
            //        message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED;
            //        error = Common.Constants.Errors.ERR_INTERNAL_SERVER;
            //        status = HttpStatusCode.InternalServerError.ToString();
            //        statusCode = (int)HttpStatusCode.InternalServerError;
            //    }
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = success,
            //        StatusCode = statusCode,
            //        Status = status,
            //        Message = message,
            //        Data = null,
            //        Error = error,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(statusCode, apiResponse);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Log($"Error in Login: {ex.Message}");
            //    Logger.Log($"StackTrace in Login: {ex.StackTrace}");
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = false,
            //        StatusCode = (int)HttpStatusCode.InternalServerError,
            //        Status = HttpStatusCode.InternalServerError.ToString(),
            //        Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
            //        Data = null,
            //        Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            //}
        }

        private bool IsADAvailable()
        {
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            Logger.Log($"{correlationId} > Inside IsADAvailable function");

            try
            {
                string ldapServer = _ldapSettings.Server; // ex: "ldap://192.168.1.10:389"
                Logger.Log($"{correlationId} > LDAB Server domain {ldapServer}");

                using var connection = new LdapConnection(ldapServer);
                connection.Timeout = new TimeSpan(0, 0, 3); // 3 seconds timeout

                connection.Bind(); // simple ping bind
                Logger.Log($"{correlationId} > IsADAvailable LDAB Connection bind successfull");

                return true; // AD reachable
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                Logger.Log($"{correlationId} > IsADAvailable() FAILURE. \n\t {errorDetails}");
                return false; // AD unavailable (network/DNS/server down)
            }
        }

        //private bool TryADAuthentication(LoginRequest request)
        //{
        //    Logger.Log($"TryADAuthenticatione function");

        //    try
        //    {
        //        string ldapServer = _ldapSettings.Server;

        //        var credential = new NetworkCredential(request.Username, request.Password);

        //        using var connection = new LdapConnection(ldapServer)
        //        {
        //            Credential = credential,
        //            AuthType = AuthType.Basic
        //        };
        //        Logger.Log($"TryADAuthentication bind successfull");

        //        connection.Bind(); // Will fail if wrong password

        //        return true;
        //    }
        //    catch
        //    {
        //        Logger.Log($"TryADAuthentication failed");

        //        return false; // Authentication failed
        //    }
        //}

        private bool TryADAuthentication(LoginRequest request)
        {
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            Logger.Log($"{correlationId} > TryADAuthentication started");

            try
            {
                string ldapServer = _ldapSettings.Server;// "jssl.in";   // your domain controller DNS
                Logger.Log($"{correlationId} > ldapServer {ldapServer}");

                string userPrincipalName = $"{request.Username}@{ldapServer}";
                Logger.Log($"{correlationId} > userPrincipalName {userPrincipalName}");

                var credential = new NetworkCredential(userPrincipalName, request.Password);

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer))
                {
                    Credential = credential,
                    AuthType = AuthType.Negotiate   // secure authentication
                };

                // Optional: Skip certificate verification (development only)
            //    connection.SessionOptions.VerifyServerCertificate += (conn, cert) => true;

                // If password is wrong, this throws an exception
                connection.Bind();

                Logger.Log($"{correlationId} > TryADAuthentication successful");
                return true;
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                Logger.Log($"{correlationId} > TryADAuthentication() FAILURE. \n\t {errorDetails}");
                return false; // TryADAuthentication unavailable (network/DNS/server down)
            }
        }


    }
}