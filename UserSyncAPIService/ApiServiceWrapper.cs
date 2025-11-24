using System.Diagnostics;
using System.ServiceProcess;

namespace UserSyncAPIService
{
    public class ApiServiceWrapper : ServiceBase
    {
        private Process _apiProcess;
        //private readonly string _exePath = @"C:\Sun\JSW\TomcotService\UserSyncAPI_Tomcat.exe";
        //private readonly string _logFile = @"C:\Sun\JSW\TomcotService\ServiceWrapperLog.txt";
        private bool _stopping = false;
        private Thread _monitorThread;
        private ServiceConfig _config = new();

        public ApiServiceWrapper()
        {
            Log($"Service Const");
            ServiceName = "UserSyncAPI_Service";
            LoadConfiguration();
        }
        private void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            _config = config.GetSection("ServiceConfig").Get<ServiceConfig>() ?? new ServiceConfig();
        }
        protected override void OnStart(string[] args)
        {
            Log($"Service Start");

            _stopping = false;
            StartApiProcess();

            // Start monitor thread to restart API if it crashes
            _monitorThread = new Thread(MonitorProcess)
            {
                IsBackground = true
            };
            _monitorThread.Start();
        }
        protected override void OnStop()
        {
            Log($"Service stop");
            _stopping = true;

            try
            {
                if (_apiProcess != null && !_apiProcess.HasExited)
                {
                    _apiProcess.Kill(true);
                    _apiProcess.Dispose();
                    _apiProcess = null;
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to stop API - {ex}");
            }

            _monitorThread?.Join();
        }
        private void StartApiProcess()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _config.ExePath,
                    Arguments = "", // add arguments if needed
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_config.ExePath)
                };

                _apiProcess = new Process { StartInfo = startInfo };
                _apiProcess.EnableRaisingEvents = true;
                _apiProcess.Exited += (s, e) => Log("API process exited unexpectedly.");
                _apiProcess.Start();
                Log("API process started successfully.");
            }
            catch (Exception ex)
            {
                Log($"Failed to start API - {ex}");
            }
        }

        private void MonitorProcess()
        {
            while (!_stopping)
            {
                try
                {
                    if (_apiProcess == null || _apiProcess.HasExited)
                    {
                        Log("API not running. Restarting...");
                        StartApiProcess();
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error in monitor thread - {ex}");
                }

                Thread.Sleep(5000); // check every 5 seconds
            }
        }
        private void Log(string message)
        {
            try
            {
                File.AppendAllText(_config.LogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch
            {
                // ignore logging errors
            }
        }

    }
}
