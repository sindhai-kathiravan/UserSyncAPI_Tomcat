using System.Globalization;
namespace UserSyncAPI_Tomcat
{
    public static class Logger
    {
        private static readonly IConfigurationRoot _configuration;
        private static readonly string LogFolder;
        private static readonly int RetentionDays;
        private static readonly bool DeleteOldLogs;

        static Logger()
        {
            // Build configuration manually
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var section = _configuration.GetSection("LoggingSettings");

            string folder = section.GetValue<string>("LogFolder") ?? "Logs";
            LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

            RetentionDays = section.GetValue<int>("LogFileRetentionDays", 7);
            DeleteOldLogs = section.GetValue<bool>("DeleteOldLogs", false);
        }

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                {
                    Directory.CreateDirectory(LogFolder);
                }

                string todayFile = Path.Combine(LogFolder, $"Log_{DateTime.Now:yyyyMMdd}.txt");

                // Clean old logs if enabled
                if (DeleteOldLogs)
                {
                    CleanupOldLogs();
                }

                string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
                File.AppendAllText(todayFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore errors
            }
        }

        private static void CleanupOldLogs()
        {
            try
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-RetentionDays);

                foreach (var file in Directory.GetFiles(LogFolder, "Log_*.txt"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string datePart = fileName.Replace("Log_", "");
                    if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
                    {
                        if (fileDate < cutoffDate.Date)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}