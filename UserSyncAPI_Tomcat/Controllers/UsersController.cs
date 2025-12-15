using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Transactions;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Common;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models;
using UserSyncAPI_Tomcat.Security;

namespace UserSyncAPI_Tomcat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = Constants.AuthenticationSchemes.BasicAuthentication)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class UsersController : ControllerBase
    {
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            ApiResponse<object> apiResponse;
            try
            {
                List<User> users = new List<User>();
                using (SqlConnection conn = DbConnectionFactory.GetDefaultConnection())
                {
                    conn.Open();
                    string query = @"SELECT (SELECT STUFF((SELECT ',' + rtrim(FactoryCode)FROM UserFactoryMapping AS UFM WHERE UFM.UserId = U.user_id FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as FactoryList,
                                   user_id, user_name, user_loginname, user_loggedonat, user_fullname, user_email,
                                   user_initials, user_password, user_department, user_loggedin, user_inmodule,
                                   g2version, lastlogin, os, clr, user_menus, logincount, libraries_readonly,
                                   group_company, screen1_res, screen2_res, screen3_res, screen4_res,
                                   po_auth_id, po_auth_all, order_alerts, po_auth_temp_user_id, maxordervalue,
                                   outofoffice, default_order_department, porole_id, deleted, alias_username_1,
                                   invoicebarcodeprinter, smtpserver, factory_id, piecemonitoringaccesslevel,
                                   exclassedit, timesheetsaccesslevel, initial_windows, fabscheduleaccesslevel,
                                   fablinescheduleaccesslevel, paintlinescheduleaccesslevel, contractsaccesslevel,
                                   g2updaterversion, updatelocation_id, allocationadmin, user_password_last_changed,
                                   date_created, createdbyuser_id, date_modified, modifiedbyuser_id,
                                   loggedinoncomputer, yloc, ylocdsc, locked, fattempt, remarks, releasedt,
                                   releaseby, inactive, inactiveremarks, inactivereleasedt, inactivereleaseby,
                                   password_attempts--, password_updated_flag, unlock_date
                            FROM Users U
                            WHERE deleted = 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                    FactoryList = reader["FactoryList"] == DBNull.Value ? null : Convert.ToString(reader["FactoryList"]).Split(',').Select(f => f.Trim()).ToList(),
                                    UserName = reader["user_name"] != DBNull.Value ? Convert.ToString(reader["user_name"]) : null,
                                    UserLoginName = reader["user_loginname"] != DBNull.Value ? reader["user_loginname"].ToString() : null,
                                    UserLoggedOnAt = reader["user_loggedonat"] != DBNull.Value ? Convert.ToString(reader["user_loggedonat"]) : null,
                                    UserFullName = reader["user_fullname"] != DBNull.Value ? reader["user_fullname"].ToString() : null,
                                    UserEmail = reader["user_email"] != DBNull.Value ? reader["user_email"].ToString() : null,
                                    UserInitials = reader["user_initials"] != DBNull.Value ? reader["user_initials"].ToString() : null,
                                    UserPassword = reader["user_password"] != DBNull.Value ? reader["user_password"].ToString() : null,
                                    UserDepartment = reader["user_department"] != DBNull.Value ? Convert.ToInt32(reader["user_department"]) : 0,
                                    UserLoggedIn = reader["user_loggedin"] != DBNull.Value ? Convert.ToBoolean(reader["user_loggedin"]) : false,
                                    UserInModule = reader["user_inmodule"] != DBNull.Value ? Convert.ToInt32(reader["user_inmodule"]) : 0,
                                    G2Version = reader["g2version"] != DBNull.Value ? reader["g2version"].ToString() : null,
                                    LastLogin = reader["lastlogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastlogin"]) : (DateTime?)null,
                                    OS = reader["os"] != DBNull.Value ? reader["os"].ToString() : null,
                                    CLR = reader["clr"] != DBNull.Value ? reader["clr"].ToString() : null,
                                    UserMenus = reader["user_menus"] != DBNull.Value ? reader["user_menus"].ToString() : null,
                                    LoginCount = reader["logincount"] != DBNull.Value ? Convert.ToInt32(reader["logincount"]) : 0,
                                    LibrariesReadOnly = reader["libraries_readonly"] != DBNull.Value ? Convert.ToBoolean(reader["libraries_readonly"]) : false,
                                    GroupCompany = reader["group_company"] != DBNull.Value ? Convert.ToInt32(reader["group_company"]) : 0,
                                    Screen1Res = reader["screen1_res"] != DBNull.Value ? reader["screen1_res"].ToString() : null,
                                    Screen2Res = reader["screen2_res"] != DBNull.Value ? reader["screen2_res"].ToString() : null,
                                    Screen3Res = reader["screen3_res"] != DBNull.Value ? reader["screen3_res"].ToString() : null,
                                    Screen4Res = reader["screen4_res"] != DBNull.Value ? reader["screen4_res"].ToString() : null,
                                    POAuthId = reader["po_auth_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_id"]) : 0,
                                    POAuthAll = reader["po_auth_all"] != DBNull.Value ? Convert.ToBoolean(reader["po_auth_all"]) : false,
                                    OrderAlerts = reader["order_alerts"] != DBNull.Value ? Convert.ToBoolean(reader["order_alerts"]) : false,
                                    POAuthTempUserId = reader["po_auth_temp_user_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_temp_user_id"]) : 0,
                                    MaxOrderValue = reader["maxordervalue"] != DBNull.Value ? Convert.ToInt32(reader["maxordervalue"]) : 0,
                                    OutOfOffice = reader["outofoffice"] != DBNull.Value ? Convert.ToBoolean(reader["outofoffice"]) : false,
                                    DefaultOrderDepartment = reader["default_order_department"] != DBNull.Value ? Convert.ToInt32(reader["default_order_department"]) : 0,
                                    PORoleId = reader["porole_id"] != DBNull.Value ? Convert.ToInt32(reader["porole_id"]) : 0,
                                    Deleted = reader["deleted"] != DBNull.Value ? Convert.ToBoolean(reader["deleted"]) : false,
                                    AliasUserName1 = reader["alias_username_1"] != DBNull.Value ? reader["alias_username_1"].ToString() : null,
                                    InvoiceBarcodePrinter = reader["invoicebarcodeprinter"] != DBNull.Value ? reader["invoicebarcodeprinter"].ToString() : null,
                                    SmtpServer = reader["smtpserver"] != DBNull.Value ? reader["smtpserver"].ToString() : null,
                                    FactoryId = reader["factory_id"] != DBNull.Value ? Convert.ToInt32(reader["factory_id"]) : 0,
                                    PieceMonitoringAccessLevel = reader["piecemonitoringaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["piecemonitoringaccesslevel"]) : 0,
                                    ExClassEdit = reader["exclassedit"] != DBNull.Value ? Convert.ToInt32(reader["exclassedit"]) : 0,
                                    TimesheetsAccessLevel = reader["timesheetsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["timesheetsaccesslevel"]) : 0,
                                    InitialWindows = reader["initial_windows"] != DBNull.Value ? reader["initial_windows"].ToString() : null,
                                    FabScheduleAccessLevel = reader["fabscheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fabscheduleaccesslevel"]) : 0,
                                    FabLineScheduleAccessLevel = reader["fablinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fablinescheduleaccesslevel"]) : 0,
                                    PaintLineScheduleAccessLevel = reader["paintlinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["paintlinescheduleaccesslevel"]) : 0,
                                    ContractsAccessLevel = reader["contractsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["contractsaccesslevel"]) : 0,
                                    G2UpdaterVersion = reader["g2updaterversion"] != DBNull.Value ? reader["g2updaterversion"].ToString() : null,
                                    UpdateLocationId = reader["updatelocation_id"] != DBNull.Value ? Convert.ToInt32(reader["updatelocation_id"]) : 0,
                                    AllocationAdmin = reader["allocationadmin"] != DBNull.Value ? Convert.ToBoolean(reader["allocationadmin"]) : false,
                                    UserPasswordLastChanged = reader["user_password_last_changed"] != DBNull.Value ? Convert.ToDateTime(reader["user_password_last_changed"]) : (DateTime?)null,
                                    DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : (DateTime?)null,
                                    CreatedByUserId = reader["createdbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["createdbyuser_id"]) : 0,
                                    DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : (DateTime?)null,
                                    ModifiedByUserId = reader["modifiedbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["modifiedbyuser_id"]) : 0,
                                    LoggedInOnComputer = reader["loggedinoncomputer"] != DBNull.Value ? reader["loggedinoncomputer"].ToString() : null,
                                    YLoc = reader["yloc"] != DBNull.Value ? reader["yloc"].ToString() : null,
                                    YLocDsc = reader["ylocdsc"] != DBNull.Value ? reader["ylocdsc"].ToString() : null,
                                    Locked = reader["locked"] != DBNull.Value ? Convert.ToInt32(reader["locked"]) : 0,
                                    FAttempt = reader["fattempt"] != DBNull.Value ? Convert.ToInt32(reader["fattempt"]) : 0,
                                    Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : null,
                                    ReleaseDt = reader["releasedt"] != DBNull.Value ? Convert.ToDateTime(reader["releasedt"]) : (DateTime?)null,
                                    ReleaseBy = reader["releaseby"] != DBNull.Value ? reader["releaseby"].ToString() : null,
                                    Inactive = reader["inactive"] != DBNull.Value ? Convert.ToInt32(reader["inactive"]) : 0,
                                    InactiveRemarks = reader["inactiveremarks"] != DBNull.Value ? reader["inactiveremarks"].ToString() : null,
                                    InactiveReleaseDt = reader["inactivereleasedt"] != DBNull.Value ? Convert.ToDateTime(reader["inactivereleasedt"]) : (DateTime?)null,
                                    InactiveReleaseBy = reader["inactivereleaseby"] != DBNull.Value ? reader["inactivereleaseby"].ToString() : null,
                                    PasswordAttempts = reader["password_attempts"] != DBNull.Value ? Convert.ToString(reader["password_attempts"]) : null//,
                                    //PasswordUpdatedFlag = reader["password_updated_flag"] != DBNull.Value ? Convert.ToString(reader["password_updated_flag"]) : null,
                                    //UnlockDate = reader["unlock_date"] != DBNull.Value ? Convert.ToDateTime(reader["unlock_date"]) : (DateTime?)null
                                });
                            }
                        }
                    }
                }
                string message = users.Count > 0 ? $"Fetched {users.Count} users successfully." : "No users found.";

                apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = message,
                    Data = new { UsersCount = users.Count, Users = users },
                    Error = null,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetAllUser: {ex.Message}");
                Logger.Log($"StackTrace in GetAllUser: {ex.StackTrace?.ToString()}");
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

        [HttpGet("get")]
        public IActionResult Get(int id)
        {
            User? userObj = null;
            ApiResponse<object>? apiResponse = null;
            string? message = null;
            string? error = null;
            bool success = false;
            object? data = null;
            try
            {
                using (SqlConnection conn = DbConnectionFactory.GetDefaultConnection())
                {
                    conn.Open();
                    string query = @"SELECT (SELECT STUFF((SELECT ',' + rtrim(FactoryCode)FROM UserFactoryMapping AS UFM WHERE UFM.UserId = U.user_id FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as FactoryList,
                       user_id, user_name, user_loginname, user_loggedonat, user_fullname, user_email,
                       user_initials, user_password, user_department, user_loggedin, user_inmodule,
                       g2version, lastlogin, os, clr, user_menus, logincount, libraries_readonly,
                       group_company, screen1_res, screen2_res, screen3_res, screen4_res,
                       po_auth_id, po_auth_all, order_alerts, po_auth_temp_user_id, maxordervalue,
                       outofoffice, default_order_department, porole_id, deleted, alias_username_1,
                       invoicebarcodeprinter, smtpserver, factory_id, piecemonitoringaccesslevel,
                       exclassedit, timesheetsaccesslevel, initial_windows, fabscheduleaccesslevel,
                       fablinescheduleaccesslevel, paintlinescheduleaccesslevel, contractsaccesslevel,
                       g2updaterversion, updatelocation_id, allocationadmin, user_password_last_changed,
                       date_created, createdbyuser_id, date_modified, modifiedbyuser_id,
                       loggedinoncomputer, yloc, ylocdsc, locked, fattempt, remarks, releasedt,
                       releaseby, inactive, inactiveremarks, inactivereleasedt, inactivereleaseby,
                       password_attempts--, password_updated_flag, unlock_date
                FROM Users U
                WHERE deleted = 0 and user_id = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userObj = new User
                                {
                                    UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                    FactoryList = reader["FactoryList"] == DBNull.Value ? null : Convert.ToString(reader["FactoryList"]).Split(',').Select(f => f.Trim()).ToList(),
                                    UserName = reader["user_name"] != DBNull.Value ? Convert.ToString(reader["user_name"]) : null,
                                    UserLoginName = reader["user_loginname"] != DBNull.Value ? reader["user_loginname"].ToString() : null,
                                    UserLoggedOnAt = reader["user_loggedonat"] != DBNull.Value ? Convert.ToString(reader["user_loggedonat"]) : null,
                                    UserFullName = reader["user_fullname"] != DBNull.Value ? reader["user_fullname"].ToString() : null,
                                    UserEmail = reader["user_email"] != DBNull.Value ? reader["user_email"].ToString() : null,
                                    UserInitials = reader["user_initials"] != DBNull.Value ? reader["user_initials"].ToString() : null,
                                    UserPassword = reader["user_password"] != DBNull.Value ? reader["user_password"].ToString() : null,
                                    UserDepartment = reader["user_department"] != DBNull.Value ? Convert.ToInt32(reader["user_department"]) : 0,
                                    UserLoggedIn = reader["user_loggedin"] == DBNull.Value ? false : Convert.ToBoolean(reader["user_loggedin"]),
                                    UserInModule = reader["user_inmodule"] != DBNull.Value ? Convert.ToInt32(reader["user_inmodule"]) : 0,
                                    G2Version = reader["g2version"] != DBNull.Value ? reader["g2version"].ToString() : null,
                                    LastLogin = reader["lastlogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastlogin"]) : (DateTime?)null,
                                    OS = reader["os"] != DBNull.Value ? reader["os"].ToString() : null,
                                    CLR = reader["clr"] != DBNull.Value ? reader["clr"].ToString() : null,
                                    UserMenus = reader["user_menus"] != DBNull.Value ? reader["user_menus"].ToString() : null,
                                    LoginCount = reader["logincount"] != DBNull.Value ? Convert.ToInt32(reader["logincount"]) : 0,
                                    LibrariesReadOnly = reader["libraries_readonly"] != DBNull.Value ? Convert.ToBoolean(reader["libraries_readonly"]) : false,
                                    GroupCompany = reader["group_company"] != DBNull.Value ? Convert.ToInt32(reader["group_company"]) : 0,
                                    Screen1Res = reader["screen1_res"] != DBNull.Value ? reader["screen1_res"].ToString() : null,
                                    Screen2Res = reader["screen2_res"] != DBNull.Value ? reader["screen2_res"].ToString() : null,
                                    Screen3Res = reader["screen3_res"] != DBNull.Value ? reader["screen3_res"].ToString() : null,
                                    Screen4Res = reader["screen4_res"] != DBNull.Value ? reader["screen4_res"].ToString() : null,
                                    POAuthId = reader["po_auth_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_id"]) : 0,
                                    POAuthAll = reader["po_auth_all"] != DBNull.Value ? Convert.ToBoolean(reader["po_auth_all"]) : false,
                                    OrderAlerts = reader["order_alerts"] != DBNull.Value ? Convert.ToBoolean(reader["order_alerts"]) : false,
                                    POAuthTempUserId = reader["po_auth_temp_user_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_temp_user_id"]) : 0,
                                    MaxOrderValue = reader["maxordervalue"] != DBNull.Value ? Convert.ToInt32(reader["maxordervalue"]) : 0,
                                    OutOfOffice = reader["outofoffice"] != DBNull.Value ? Convert.ToBoolean(reader["outofoffice"]) : false,
                                    DefaultOrderDepartment = reader["default_order_department"] != DBNull.Value ? Convert.ToInt32(reader["default_order_department"]) : 0,
                                    PORoleId = reader["porole_id"] != DBNull.Value ? Convert.ToInt32(reader["porole_id"]) : 0,
                                    Deleted = reader["deleted"] != DBNull.Value ? Convert.ToBoolean(reader["deleted"]) : false,
                                    AliasUserName1 = reader["alias_username_1"] != DBNull.Value ? reader["alias_username_1"].ToString() : null,
                                    InvoiceBarcodePrinter = reader["invoicebarcodeprinter"] != DBNull.Value ? reader["invoicebarcodeprinter"].ToString() : null,
                                    SmtpServer = reader["smtpserver"] != DBNull.Value ? reader["smtpserver"].ToString() : null,
                                    FactoryId = reader["factory_id"] != DBNull.Value ? Convert.ToInt32(reader["factory_id"]) : 0,
                                    PieceMonitoringAccessLevel = reader["piecemonitoringaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["piecemonitoringaccesslevel"]) : 0,
                                    ExClassEdit = reader["exclassedit"] != DBNull.Value ? Convert.ToInt32(reader["exclassedit"]) : 0,
                                    TimesheetsAccessLevel = reader["timesheetsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["timesheetsaccesslevel"]) : 0,
                                    InitialWindows = reader["initial_windows"] != DBNull.Value ? reader["initial_windows"].ToString() : null,
                                    FabScheduleAccessLevel = reader["fabscheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fabscheduleaccesslevel"]) : 0,
                                    FabLineScheduleAccessLevel = reader["fablinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fablinescheduleaccesslevel"]) : 0,
                                    PaintLineScheduleAccessLevel = reader["paintlinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["paintlinescheduleaccesslevel"]) : 0,
                                    ContractsAccessLevel = reader["contractsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["contractsaccesslevel"]) : 0,
                                    G2UpdaterVersion = reader["g2updaterversion"] != DBNull.Value ? reader["g2updaterversion"].ToString() : null,
                                    UpdateLocationId = reader["updatelocation_id"] != DBNull.Value ? Convert.ToInt32(reader["updatelocation_id"]) : 0,
                                    AllocationAdmin = reader["allocationadmin"] != DBNull.Value ? Convert.ToBoolean(reader["allocationadmin"]) : false,
                                    UserPasswordLastChanged = reader["user_password_last_changed"] != DBNull.Value ? Convert.ToDateTime(reader["user_password_last_changed"]) : (DateTime?)null,
                                    DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : (DateTime?)null,
                                    CreatedByUserId = reader["createdbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["createdbyuser_id"]) : 0,
                                    DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : (DateTime?)null,
                                    ModifiedByUserId = reader["modifiedbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["modifiedbyuser_id"]) : 0,
                                    LoggedInOnComputer = reader["loggedinoncomputer"] != DBNull.Value ? reader["loggedinoncomputer"].ToString() : null,
                                    YLoc = reader["yloc"] != DBNull.Value ? reader["yloc"].ToString() : null,
                                    YLocDsc = reader["ylocdsc"] != DBNull.Value ? reader["ylocdsc"].ToString() : null,
                                    Locked = reader["locked"] != DBNull.Value ? Convert.ToInt32(reader["locked"]) : 0,
                                    FAttempt = reader["fattempt"] != DBNull.Value ? Convert.ToInt32(reader["fattempt"]) : 0,
                                    Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : null,
                                    ReleaseDt = reader["releasedt"] != DBNull.Value ? Convert.ToDateTime(reader["releasedt"]) : (DateTime?)null,
                                    ReleaseBy = reader["releaseby"] != DBNull.Value ? reader["releaseby"].ToString() : null,
                                    Inactive = reader["inactive"] != DBNull.Value ? Convert.ToInt32(reader["inactive"]) : 0,
                                    InactiveRemarks = reader["inactiveremarks"] != DBNull.Value ? reader["inactiveremarks"].ToString() : null,
                                    InactiveReleaseDt = reader["inactivereleasedt"] != DBNull.Value ? Convert.ToDateTime(reader["inactivereleasedt"]) : (DateTime?)null,
                                    InactiveReleaseBy = reader["inactivereleaseby"] != DBNull.Value ? reader["inactivereleaseby"].ToString() : null,
                                    PasswordAttempts = reader["password_attempts"] != DBNull.Value ? Convert.ToString(reader["password_attempts"]) : null//,
                                    //PasswordUpdatedFlag = reader["password_updated_flag"] != DBNull.Value ? Convert.ToString(reader["password_updated_flag"]) : null,
                                    //UnlockDate = reader["unlock_date"] != DBNull.Value ? Convert.ToDateTime(reader["unlock_date"]) : (DateTime?)null
                                };
                                success = true;
                                message = Common.Constants.Messages.USER_RETRIEVED_SUCCESSFULLY;
                                error = null;
                                data = new { User = userObj };
                            }
                            else
                            {
                                success = false;
                                message = $"User with Id {id} was not found.";
                                error = Common.Constants.Errors.ERR_NOT_FOUND;
                                data = null;
                            }
                        }
                    }
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = success,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = message,
                    Data = data,
                    Error = error,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetUser: {ex.Message}");
                Logger.Log($"StackTrace in GetUser: {ex.StackTrace?.ToString()}");
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

        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateUserRequest request)
        {
            ApiResponse<object>? apiResponse = null;
            List<object> newUser = new List<object>();
            try
            {
                int newId = 0;
                //using (var scope = new TransactionScope())
                {
                    var config = new ConfigurationBuilder()
                                     .SetBasePath(AppContext.BaseDirectory)
                                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                     .Build();

                    // Get all connection strings
                    var connectionStringsSection = config.GetSection("ConnectionStrings");
                    var allConnectionStrings = connectionStringsSection
                        .GetChildren()
                        .ToDictionary(x => x.Key, x => x.Value);


                    foreach (var cs in allConnectionStrings)
                    {

                        using (SqlConnection conn = new SqlConnection(cs.Value))
                        {
                            conn.Open();

                            string sql = @"
                INSERT INTO Users (
                    user_name, user_loginname, user_loggedonat, user_fullname, user_email,
                    user_initials, user_password, user_department, user_loggedin, user_inmodule,
                    g2version, lastlogin, os, clr, user_menus, logincount, libraries_readonly,
                    group_company, screen1_res, screen2_res, screen3_res, screen4_res,
                    po_auth_id, po_auth_all, order_alerts, po_auth_temp_user_id, maxordervalue,
                    outofoffice, default_order_department, porole_id, deleted, alias_username_1,
                    invoicebarcodeprinter, smtpserver, factory_id, piecemonitoringaccesslevel,
                    exclassedit, timesheetsaccesslevel, initial_windows, fabscheduleaccesslevel,
                    fablinescheduleaccesslevel, paintlinescheduleaccesslevel, contractsaccesslevel,
                    g2updaterversion, updatelocation_id, allocationadmin, user_password_last_changed,
                    date_created, createdbyuser_id, date_modified, modifiedbyuser_id,
                    loggedinoncomputer, yloc, ylocdsc, locked, fattempt, remarks, releasedt,
                    releaseby, inactive, inactiveremarks, inactivereleasedt, inactivereleaseby,
                    password_attempts--, password_updated_flag, unlock_date
                )
                VALUES (
                    @UserName, @UserLoginName, @UserLoggedOnAt, @UserFullName, @UserEmail,
                    @UserInitials, @UserPassword, @UserDepartment, @UserLoggedIn, @UserInModule,
                    @G2Version, @LastLogin, @OS, @CLR, @UserMenus, @LoginCount, @LibrariesReadOnly,
                    @GroupCompany, @Screen1Res, @Screen2Res, @Screen3Res, @Screen4Res,
                    @POAuthId, @POAuthAll, @OrderAlerts, @POAuthTempUserId, @MaxOrderValue,
                    @OutOfOffice, @DefaultOrderDepartment, @PORoleId, @Deleted, @AliasUserName1,
                    @InvoiceBarcodePrinter, @SmtpServer, @FactoryId, @PieceMonitoringAccessLevel,
                    @ExClassEdit, @TimesheetsAccessLevel, @InitialWindows, @FabScheduleAccessLevel,
                    @FabLineScheduleAccessLevel, @PaintLineScheduleAccessLevel, @ContractsAccessLevel,
                    @G2UpdaterVersion, @UpdateLocationId, @AllocationAdmin, @UserPasswordLastChanged,
                    GETDATE(), @CreatedByUserId, GETDATE(), @ModifiedByUserId,
                    @LoggedInOnComputer, @Yloc, @YlocDsc, @Locked, @FAttempt, @Remarks, @ReleaseDt,
                    @ReleaseBy, @Inactive, @InactiveRemarks, @InactiveReleaseDt, @InactiveReleaseBy,
                    @PasswordAttempts--, @PasswordUpdatedFlag, @UnlockDate
                );
                SELECT SCOPE_IDENTITY();";

                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@UserName", request.UserName);
                                cmd.Parameters.AddWithValue("@UserLoginName", request.UserLoginName);
                                cmd.Parameters.AddWithValue("@UserLoggedOnAt", request.UserLoggedOnAt);
                                cmd.Parameters.AddWithValue("@UserFullName", request.UserFullName);
                                cmd.Parameters.AddWithValue("@UserEmail", request.UserEmail);
                                cmd.Parameters.AddWithValue("@UserInitials", request.UserInitials);
                                cmd.Parameters.AddWithValue("@UserPassword", SecurityExtensions.Encrypt(request.UserPassword));
                                cmd.Parameters.AddWithValue("@UserDepartment", request.UserDepartment);
                                cmd.Parameters.AddWithValue("@UserLoggedIn", request.UserLoggedIn);
                                cmd.Parameters.AddWithValue("@UserInModule", request.UserInModule);
                                cmd.Parameters.AddWithValue("@G2Version", (object)request.G2Version ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@LastLogin", (object)request.LastLogin ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@OS", request.OS);
                                cmd.Parameters.AddWithValue("@CLR", request.CLR);
                                cmd.Parameters.AddWithValue("@UserMenus", request.UserMenus);
                                cmd.Parameters.AddWithValue("@LoginCount", request.LoginCount);
                                cmd.Parameters.AddWithValue("@LibrariesReadOnly", request.LibrariesReadOnly);
                                cmd.Parameters.AddWithValue("@GroupCompany", request.GroupCompany);
                                cmd.Parameters.AddWithValue("@Screen1Res", request.Screen1Res);
                                cmd.Parameters.AddWithValue("@Screen2Res", request.Screen2Res);
                                cmd.Parameters.AddWithValue("@Screen3Res", request.Screen3Res);
                                cmd.Parameters.AddWithValue("@Screen4Res", request.Screen4Res);
                                cmd.Parameters.AddWithValue("@POAuthId", request.POAuthId);
                                cmd.Parameters.AddWithValue("@POAuthAll", request.POAuthAll);
                                cmd.Parameters.AddWithValue("@OrderAlerts", request.OrderAlerts);
                                cmd.Parameters.AddWithValue("@POAuthTempUserId", request.POAuthTempUserId);
                                cmd.Parameters.AddWithValue("@MaxOrderValue", request.MaxOrderValue);
                                cmd.Parameters.AddWithValue("@OutOfOffice", request.OutOfOffice);
                                cmd.Parameters.AddWithValue("@DefaultOrderDepartment", request.DefaultOrderDepartment);
                                cmd.Parameters.AddWithValue("@PORoleId", request.PORoleId);
                                cmd.Parameters.AddWithValue("@Deleted", request.Deleted);
                                cmd.Parameters.AddWithValue("@AliasUserName1", (object)request.AliasUserName1 ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@InvoiceBarcodePrinter", request.InvoiceBarcodePrinter);
                                cmd.Parameters.AddWithValue("@SmtpServer", request.SmtpServer);
                                cmd.Parameters.AddWithValue("@FactoryId", request.FactoryId);
                                cmd.Parameters.AddWithValue("@PieceMonitoringAccessLevel", request.PieceMonitoringAccessLevel);
                                cmd.Parameters.AddWithValue("@ExClassEdit", request.ExClassEdit);
                                cmd.Parameters.AddWithValue("@TimesheetsAccessLevel", request.TimesheetsAccessLevel);
                                cmd.Parameters.AddWithValue("@InitialWindows", request.InitialWindows);
                                cmd.Parameters.AddWithValue("@FabScheduleAccessLevel", request.FabScheduleAccessLevel);
                                cmd.Parameters.AddWithValue("@FabLineScheduleAccessLevel", request.FabLineScheduleAccessLevel);
                                cmd.Parameters.AddWithValue("@PaintLineScheduleAccessLevel", request.PaintLineScheduleAccessLevel);
                                cmd.Parameters.AddWithValue("@ContractsAccessLevel", request.ContractsAccessLevel);
                                cmd.Parameters.AddWithValue("@G2UpdaterVersion", (object)request.G2UpdaterVersion ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdateLocationId", request.UpdateLocationId);
                                cmd.Parameters.AddWithValue("@AllocationAdmin", request.AllocationAdmin);
                                cmd.Parameters.AddWithValue("@UserPasswordLastChanged", (object)request.UserPasswordLastChanged ?? DBNull.Value);
                                //cmd.Parameters.AddWithValue("@DateCreated", (object)request.DateCreated ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CreatedByUserId", (object)request.CreatedByUserId ?? DBNull.Value);
                                //cmd.Parameters.AddWithValue("@DateModified", (object)request.DateModified ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ModifiedByUserId", (object)request.ModifiedByUserId ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@LoggedInOnComputer", request.LoggedInOnComputer);
                                cmd.Parameters.AddWithValue("@Yloc", (object)request.YLoc ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@YlocDsc", (object)request.YLocDsc ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Locked", (object)request.Locked ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@FAttempt", (object)request.FAttempt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Remarks", (object)request.Remarks ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ReleaseDt", (object)request.ReleaseDt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ReleaseBy", (object)request.ReleaseBy ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Inactive", (object)request.Inactive ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@InactiveRemarks", (object)request.InactiveRemarks ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@InactiveReleaseDt", (object)request.InactiveReleaseDt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@InactiveReleaseBy", (object)request.InactiveReleaseBy ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@PasswordAttempts", (object)request.PasswordAttempts ?? DBNull.Value);
                                //cmd.Parameters.AddWithValue("@PasswordUpdatedFlag", (object)request.PasswordUpdatedFlag ?? DBNull.Value);
                                //cmd.Parameters.AddWithValue("@UnlockDate", (object)request.UnlockDate ?? DBNull.Value);
                                newId = Convert.ToInt32(cmd.ExecuteScalar());
                                newUser.Add(new { Database = cs.Key, NewUserId = newId });
                                foreach (string factoryCode in request.TargetFactories)
                                {
                                    InsertUserFactoryMapping(newId, factoryCode, request.CreatedByUserId, conn);
                                }
                            }
                        }
                    }
                    //scope.Complete();
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = Common.Constants.Messages.USER_CREATED_SUCCESSFULLY,
                    Data = newUser,
                    Error = null,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in CreateUser: {ex.Message}");
                Logger.Log($"StackTrace in CreateUser: {ex.StackTrace?.ToString()}");
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

        [HttpPut]
        [Route("update")]
        public IActionResult Update([FromBody] UpdateUserRequest request)
        {
            ApiResponse<object> apiResponse;
            try
            {
                //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var config = new ConfigurationBuilder()
                                     .SetBasePath(AppContext.BaseDirectory)
                                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                     .Build();
                    // Get all connection strings
                    var connectionStringsSection = config.GetSection("ConnectionStrings");
                    var allConnectionStrings = connectionStringsSection
                        .GetChildren()
                        .ToDictionary(x => x.Key, x => x.Value);
                    foreach (var cs in allConnectionStrings)
                    {
                        using (var conn = new SqlConnection(cs.Value))
                        {
                            conn.Open();

                            var setClauses = new List<string>();
                            using var cmd = conn.CreateCommand();

                            void Add<T>(string column, string param, SqlDbType type, T value)
                            {
                                setClauses.Add($"{column} = @{param}");
                                cmd.Parameters.Add($"@{param}", type).Value = value!;
                            }

                            // -------- BASIC USER INFO --------
                            if (request.UserName != null)
                                Add("user_name", "UserName", SqlDbType.VarChar, request.UserName);

                            if (request.UserLoginName != null)
                                Add("user_loginname", "UserLoginName", SqlDbType.VarChar, request.UserLoginName);

                            if (request.UserLoggedOnAt != null)
                                Add("user_loggedonat", "UserLoggedOnAt", SqlDbType.DateTime, request.UserLoggedOnAt);

                            if (request.UserFullName != null)
                                Add("user_fullname", "UserFullName", SqlDbType.VarChar, request.UserFullName);

                            if (request.UserEmail != null)
                                Add("user_email", "UserEmail", SqlDbType.VarChar, request.UserEmail);

                            if (request.UserInitials != null)
                                Add("user_initials", "UserInitials", SqlDbType.VarChar, request.UserInitials);


                            if (request.UserDepartment != null)
                                Add("user_department", "UserDepartment", SqlDbType.VarChar, request.UserDepartment);

                            if (request.UserInModule != null)
                                Add("user_inmodule", "UserInModule", SqlDbType.VarChar, request.UserInModule);

                            // -------- SYSTEM INFO --------
                            if (request.G2Version != null)
                                Add("g2version", "G2Version", SqlDbType.VarChar, request.G2Version);

                            if (request.OS != null)
                                Add("os", "OS", SqlDbType.VarChar, request.OS);

                            if (request.CLR != null)
                                Add("clr", "CLR", SqlDbType.VarChar, request.CLR);

                            if (request.UserMenus != null)
                                Add("user_menus", "UserMenus", SqlDbType.VarChar, request.UserMenus);

                            if (request.LoginCount.HasValue)
                                Add("logincount", "LoginCount", SqlDbType.Int, request.LoginCount.Value);

                            if (request.LibrariesReadOnly.HasValue)
                                Add("libraries_readonly", "LibrariesReadonly", SqlDbType.Bit, request.LibrariesReadOnly.Value);

                            if (request.GroupCompany != null)
                                Add("group_company", "GroupCompany", SqlDbType.VarChar, request.GroupCompany);

                            // -------- SCREEN / UI --------
                            if (request.Screen1Res != null)
                                Add("screen1_res", "Screen1Res", SqlDbType.VarChar, request.Screen1Res);

                            if (request.Screen2Res != null)
                                Add("screen2_res", "Screen2Res", SqlDbType.VarChar, request.Screen2Res);

                            if (request.Screen3Res != null)
                                Add("screen3_res", "Screen3Res", SqlDbType.VarChar, request.Screen3Res);

                            if (request.Screen4Res != null)
                                Add("screen4_res", "Screen4Res", SqlDbType.VarChar, request.Screen4Res);

                            // -------- PURCHASE / ORDER --------
                            if (request.POAuthId.HasValue)
                                Add("po_auth_id", "PoAuthId", SqlDbType.Int, request.POAuthId.Value);

                            if (request.POAuthAll.HasValue)
                                Add("po_auth_all", "PoAuthAll", SqlDbType.Bit, request.POAuthAll.Value);

                            if (request.OrderAlerts.HasValue)
                                Add("order_alerts", "OrderAlerts", SqlDbType.Bit, request.OrderAlerts.Value);

                            if (request.POAuthTempUserId.HasValue)
                                Add("po_auth_temp_user_id", "PoAuthTempUserId", SqlDbType.Int, request.POAuthTempUserId.Value);

                            if (request.MaxOrderValue.HasValue)
                                Add("maxordervalue", "MaxOrderValue", SqlDbType.Decimal, request.MaxOrderValue.Value);

                            if (request.OutOfOffice.HasValue)
                                Add("outofoffice", "OutOfOffice", SqlDbType.Bit, request.OutOfOffice.Value);

                            if (request.DefaultOrderDepartment != null)
                                Add("default_order_department", "DefaultOrderDepartment", SqlDbType.VarChar, request.DefaultOrderDepartment);

                            if (request.PORoleId.HasValue)
                                Add("porole_id", "PoRoleId", SqlDbType.Int, request.PORoleId.Value);

                            // -------- HARDWARE / SYSTEM --------
                            if (request.AliasUserName1 != null)
                                Add("alias_username_1", "AliasUserName1", SqlDbType.VarChar, request.AliasUserName1);

                            if (request.InvoiceBarcodePrinter != null)
                                Add("invoicebarcodeprinter", "InvoiceBarcodePrinter", SqlDbType.VarChar, request.InvoiceBarcodePrinter);

                            if (request.SmtpServer != null)
                                Add("smtpserver", "SmtpServer", SqlDbType.VarChar, request.SmtpServer);

                            if (request.FactoryId.HasValue)
                                Add("factory_id", "FactoryId", SqlDbType.Int, request.FactoryId.Value);

                            // -------- ACCESS LEVELS --------
                            if (request.PieceMonitoringAccessLevel.HasValue)
                                Add("piecemonitoringaccesslevel", "PieceMonitoringAccessLevel", SqlDbType.Int, request.PieceMonitoringAccessLevel.Value);

                            if (request.ExClassEdit.HasValue)
                                Add("exclassedit", "ExClassEdit", SqlDbType.Bit, request.ExClassEdit.Value);

                            if (request.TimesheetsAccessLevel.HasValue)
                                Add("timesheetsaccesslevel", "TimesheetsAccessLevel", SqlDbType.Int, request.TimesheetsAccessLevel.Value);

                            if (request.InitialWindows != null)
                                Add("initial_windows", "InitialWindows", SqlDbType.VarChar, request.InitialWindows);

                            if (request.FabScheduleAccessLevel.HasValue)
                                Add("fabscheduleaccesslevel", "FabScheduleAccessLevel", SqlDbType.Int, request.FabScheduleAccessLevel.Value);

                            if (request.FabLineScheduleAccessLevel.HasValue)
                                Add("fablinescheduleaccesslevel", "FabLineScheduleAccessLevel", SqlDbType.Int, request.FabLineScheduleAccessLevel.Value);

                            if (request.PaintLineScheduleAccessLevel.HasValue)
                                Add("paintlinescheduleaccesslevel", "PaintLineScheduleAccessLevel", SqlDbType.Int, request.PaintLineScheduleAccessLevel.Value);

                            if (request.ContractsAccessLevel.HasValue)
                                Add("contractsaccesslevel", "ContractsAccessLevel", SqlDbType.Int, request.ContractsAccessLevel.Value);

                            // -------- SECURITY / STATUS --------
                            if (request.G2UpdaterVersion != null)
                                Add("g2updaterversion", "G2UpdaterVersion", SqlDbType.VarChar, request.G2UpdaterVersion);

                            if (request.UpdateLocationId.HasValue)
                                Add("updatelocation_id", "UpdateLocationId", SqlDbType.Int, request.UpdateLocationId.Value);

                            if (request.AllocationAdmin.HasValue)
                                Add("allocationadmin", "AllocationAdmin", SqlDbType.Bit, request.AllocationAdmin.Value);

                            if (request.UserPasswordLastChanged.HasValue)
                                Add("user_password_last_changed", "UserPasswordLastChanged", SqlDbType.DateTime, request.UserPasswordLastChanged.Value);

                            if (request.LoggedInOnComputer != null)
                                Add("loggedinoncomputer", "LoggedInOnComputer", SqlDbType.VarChar, request.LoggedInOnComputer);

                            if (request.YLoc != null)
                                Add("yloc", "YLoc", SqlDbType.VarChar, request.YLoc);

                            if (request.YLocDsc != null)
                                Add("ylocdsc", "YLocDsc", SqlDbType.VarChar, request.YLocDsc);

                            if (request.Locked.HasValue)
                                Add("locked", "Locked", SqlDbType.Bit, request.Locked.Value);

                            if (request.FAttempt.HasValue)
                                Add("fattempt", "FAttempt", SqlDbType.Int, request.FAttempt.Value);

                            if (request.Remarks != null)
                                Add("remarks", "Remarks", SqlDbType.VarChar, request.Remarks);

                            if (request.ReleaseDt.HasValue)
                                Add("releasedt", "ReleaseDt", SqlDbType.DateTime, request.ReleaseDt.Value);

                            if (request.ReleaseBy!=null)
                                Add("releaseby", "ReleaseBy", SqlDbType.Int, request.ReleaseBy);

                            if (request.Inactive.HasValue)
                                Add("inactive", "Inactive", SqlDbType.Bit, request.Inactive.Value);

                            if (request.InactiveRemarks != null)
                                Add("inactiveremarks", "InactiveRemarks", SqlDbType.VarChar, request.InactiveRemarks);

                            if (request.InactiveReleaseDt.HasValue)
                                Add("inactivereleasedt", "InactiveReleaseDt", SqlDbType.DateTime, request.InactiveReleaseDt.Value);

                            if (request.InactiveReleaseBy!= null)
                                Add("inactivereleaseby", "InactiveReleaseBy", SqlDbType.Int, request.InactiveReleaseBy);

                            if (request.PasswordAttempts!= null)
                                Add("password_attempts", "PasswordAttempts", SqlDbType.Int, request.PasswordAttempts);

                            // -------- AUDIT (ALWAYS UPDATED) --------
                            setClauses.Add("date_modified = GETDATE()");
                            setClauses.Add("modifiedbyuser_id = @ModifiedByUserId");
                            cmd.Parameters.Add("@ModifiedByUserId", SqlDbType.Int).Value = request.ModifiedByUserId;

                            if (!setClauses.Any())
                                throw new InvalidOperationException("No fields provided for update.");

                            cmd.CommandText = $@"
        UPDATE Users
        SET {string.Join(", ", setClauses)}
        WHERE user_id = @UserId";

                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = request.UserId;

                            cmd.ExecuteNonQuery();

                            // -------- FACTORY MAPPING --------
                            if (request.TargetFactories != null)
                            {
                                DeleteUserFactoryMapping(request.UserId, conn);

                                foreach (var factoryCode in request.TargetFactories)
                                {
                                    InsertUserFactoryMapping(
                                        request.UserId,
                                        factoryCode,
                                        request.ModifiedByUserId,
                                        conn);
                                }
                            }
                        }

                    }
                    //    scope.Complete();
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = Common.Constants.Messages.USER_UPDATED_SUCCESSFULLY,
                    Data = null,
                    Error = null,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in UpdateUser: {ex.Message}");
                Logger.Log($"StackTrace in UpdateUser: {ex.StackTrace?.ToString()}");
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

        [HttpDelete]
        [Route("delete")]
        public IActionResult Delete([FromBody] DeleteUserRequest request)
        {
            ApiResponse<object> apiResponse;
            try
            {
                //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var config = new ConfigurationBuilder()
                                     .SetBasePath(AppContext.BaseDirectory)
                                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                     .Build();
                    // Get all connection strings
                    var connectionStringsSection = config.GetSection("ConnectionStrings");
                    var allConnectionStrings = connectionStringsSection
                        .GetChildren()
                        .ToDictionary(x => x.Key, x => x.Value);
                    foreach (var cs in allConnectionStrings)
                    {
                        using (SqlConnection conn = new SqlConnection(cs.Value))
                        {
                            conn.Open();
                            using (var command = new SqlCommand("UPDATE Users SET deleted = 1, modifiedbyuser_id = @ModifiedBy, date_modified = GETDATE() WHERE user_id = @UserId", conn))
                            {
                                command.Parameters.AddWithValue("@UserId", request.UserId);
                                command.Parameters.AddWithValue("@ModifiedBy", request.ModifiedByUserId);
                                int rows = command.ExecuteNonQuery();

                                SoftDeleteUserFactoryMapping(request.UserId, request.ModifiedByUserId, conn);
                            }
                        }
                    }
                    //scope.Complete();
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = Common.Constants.Messages.USER_DELETED_SUCCESSFULLY,
                    Data = null,
                    Error = null,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in DeleteUser: {ex.Message}");
                Logger.Log($"StackTrace in DeleteUser: {ex.StackTrace?.ToString()}");
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

        private void SoftDeleteUserFactoryMapping(int userId, int DeletedBy, SqlConnection connection)
        {
            string query = @"UPDATE UserFactoryMapping SET IsDeleted = 1, DeletedBy = @DeletedBy, DeletedDate = GETDATE() WHERE UserId = @UserId;";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@DeletedBy", DeletedBy);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertUserFactoryMapping(int userId, string factoryCode, int createdBy, SqlConnection connection)
        {
            string query = @"INSERT INTO UserFactoryMapping (UserId, FactoryCode, CreatedBy, CreatedDate)
                                    VALUES (@UserId, @FactoryCode, @CreatedBy, GETDATE());";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@FactoryCode", factoryCode);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                cmd.ExecuteNonQuery();
            }
        }
        private void DeleteUserFactoryMapping(int userId, SqlConnection connection)
        {
            string query = @"DELETE FROM UserFactoryMapping WHERE UserId = @UserId;";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
