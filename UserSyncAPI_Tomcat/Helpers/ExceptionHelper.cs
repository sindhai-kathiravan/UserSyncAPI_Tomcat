using System.DirectoryServices.Protocols;
using System.Text;

namespace UserSyncAPI_Tomcat.Helpers
{
    public static class ExceptionHelper
    {
        public static string BuildExceptionDetails(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== Exception Details ===");

            AppendExceptionInfo(sb, ex, 0);

            sb.AppendLine("=== End Exception ===");

            return sb.ToString();
        }

        private static void AppendExceptionInfo(StringBuilder sb, Exception ex, int level)
        {
            string indent = new string(' ', level * 2);

            sb.AppendLine($"{indent}Level {level}:");
            sb.AppendLine($"{indent}Type: {ex.GetType().FullName}");
            sb.AppendLine($"{indent}Message: {ex.Message}");
            sb.AppendLine($"{indent}HResult: {ex.HResult}");
            sb.AppendLine($"{indent}StackTrace:");
            sb.AppendLine($"{indent}{ex.StackTrace}");

            // LDAP-specific details
            if (ex is LdapException ldapEx)
            {
                sb.AppendLine($"{indent}LDAP Error Code: {ldapEx.ErrorCode}");
                sb.AppendLine($"{indent}LDAP Server Message: {ldapEx.ServerErrorMessage}");
            }

            // Process next inner exception
            if (ex.InnerException != null)
            {
                sb.AppendLine();
                AppendExceptionInfo(sb, ex.InnerException, level + 1);
            }
        }
    }
}
