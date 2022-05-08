using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RlktServiceController.Services
{
    internal class LocalService : Service
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private Process process { get; set; }

        public override void Tick()
        {
            try
            {
                if (process == null)
                    return;

                //Check if the process closed unexpectedly
                if (process.HasExited && Status != ServiceStatus.STOPPING && Status != ServiceStatus.ERROR)
                {
                    Logger.Add("Service[{0}_{1}] exited unexpectedly.", Name, ID.ToString());
                    Status = ServiceStatus.ERROR;
                    return;
                }

                process.Refresh();

                //Check if service is in starting and check if it started
                if (Status == ServiceStatus.STARTING)
                {
                    if (process.MainWindowTitle.Contains("operational"))
                    {
                        Logger.Add("Service[{0}_{1}] is operational!", Name, ID.ToString());
                        Status = ServiceStatus.RUNNING;
                    }
                    else if (process.MainWindowTitle.Contains("failed"))
                    {
                        Logger.Add("Service[{0}_{1}] failed to start, resulted in error, check the logs!", Name, ID.ToString());
                        Status = ServiceStatus.ERROR;
                    }
                }
                else if (Status == ServiceStatus.STOPPING)
                {
                    if (process.HasExited)
                        Status = ServiceStatus.STOPPED;
                }
            }
            catch (Exception ex)
            {
                Logger.Add("[Exception - Service[{0}_{1}] fault - {2}!", Name, ID.ToString(), ex.Message);
                Status = ServiceStatus.STOPPED;
            }
        }

        public override void Control(ProcessEvent processEvent)
        {
            switch (processEvent)
            {
                case ProcessEvent.START_PROCESS: OnStartProcess(); break;
                case ProcessEvent.STOP_PROCESS: OnStopProcess(); break;
                case ProcessEvent.RESET_PROCESS: OnResetProcess(); break;
            }
        }

        public void OnStartProcess()
        {
            if (Status == ServiceStatus.STARTING ||
                Status == ServiceStatus.STOPPING ||
                Status == ServiceStatus.RUNNING)
            {
                Logger.Add("[OnStartProcess] Failed to start process[{0}] is not in STOPPED state.", Name);
                return;
            }

            try
            {
                process = new Process();
                process.StartInfo.FileName = PathExe;
                process.StartInfo.Arguments = Args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(PathExe);

                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(Process_ErrorDataReceived);

                if (process.Start() == true)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    Thread.Sleep(100); //wait 100ms for the window to become available

                    ShowWindow(process.MainWindowHandle, 2);

                    Status = ServiceStatus.STARTING;

                    Logger.Add("[OnStartProcess] Starting Service[{0}_{1}]", Name, ID.ToString());
                }
            }
            catch (Exception err)
            {
                Status = ServiceStatus.ERROR;
                Logger.Add("[OnStartProcess] Error starting Service[{0}_{1}] Exception[{2}]", Name, ID.ToString(), err.Message);
                process = null;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            EventLog += "[out] " + e.Data + Environment.NewLine;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            EventLog += "[err] " + e.Data + Environment.NewLine;
        }

        public void OnResetProcess()
        {

        }

        public void OnStopProcess()
        {
            if (process == null)
                return;

            //Write exit\n to stdin in order to safely close the service.
            process.StandardInput.WriteLine("exit");
            Status = ServiceStatus.STOPPING;

            Logger.Add("[OnStopProcess] Stopping Service[{0}_{1}]", Name, ID.ToString());
        }
    }
}
