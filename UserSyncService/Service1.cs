using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace UserSyncService
{
    public partial class Service1 : ServiceBase
    {
        private Process _apiProcess;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string apiExePath = ConfigurationManager.AppSettings["ApiExePath"];
           // string apiExePath = "Path\\To\\Your\\Api.exe";
            string arguments = "--urls http://0.0.0.0:5000"; // Use the port you need (e.g., 5000)

            string apiExeDirectory = "C:\\Sun\\JSW\\UserSyncAPI_Tomcat\\UserSyncAPI_Tomcat\\bin\\Release\\net6.0\\publish\\";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = apiExePath,
                Arguments = arguments,
                // THE CRITICAL LINE: Set the working directory to the API's folder
                WorkingDirectory = apiExeDirectory
            };
            Process.Start(startInfo);
            //_apiProcess = new Process();
            //_apiProcess.StartInfo.FileName = apiExePath;
            //_apiProcess.StartInfo.UseShellExecute = false;
            //_apiProcess.StartInfo.CreateNoWindow = true;
            //_apiProcess.StartInfo.RedirectStandardOutput = true;
            //_apiProcess.StartInfo.RedirectStandardError = true;
            //_apiProcess.Start();

            //// Capture output to log files
            //_apiProcess.OutputDataReceived += (s, e) =>
            //{
            //    if (!string.IsNullOrEmpty(e.Data))
            //        System.IO.File.AppendAllText(@"C:\Logs\ApiOutput.log", e.Data + Environment.NewLine);
            //};

            //_apiProcess.ErrorDataReceived += (s, e) =>
            //{
            //    if (!string.IsNullOrEmpty(e.Data))
            //        System.IO.File.AppendAllText(@"C:\Logs\ApiError.log", e.Data + Environment.NewLine);
            //};
        }

        protected override void OnStop()
        {
            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                _apiProcess.Kill();
            }
        }
    }
}
