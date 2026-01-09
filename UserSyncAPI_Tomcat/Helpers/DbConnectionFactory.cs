using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Linq;
namespace UserSyncAPI_Tomcat.Helpers
{
    public static class DbConnectionFactory
    {
        private static IConfiguration? _configuration;

        // Initialize with IConfiguration once
        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Get SqlConnection by key
        public static SqlConnection GetSqlConnection(string key)
        {
            if (_configuration == null)
                throw new Exception("DbHelper not initialized.");
            string? conxnStr = _configuration.GetConnectionString(key);
            return new SqlConnection(conxnStr);
        }

        public static SqlConnection GetDefaultConnection()
        {
            // Return the first connection string in the section
            var section = _configuration?.GetSection("ConnectionStrings").GetChildren();
            var first = section?.FirstOrDefault();
            if (first != null)
            {
                return new SqlConnection(first?.Value);
            
            }
            else
            {
                throw new Exception("ConnectionStrings list in app setting is empty");
            }
        }

        public static string? GetDefaultConnectionKey()
        {
            // Return the first connection string in the section
            var section = _configuration?.GetSection("ConnectionStrings").GetChildren();
            var first = section?.FirstOrDefault();
            if (first != null)
            {
                return first?.Key;

            }
            else
            {
                return null;
            }
        }
    }
}