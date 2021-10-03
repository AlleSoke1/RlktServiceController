using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;

namespace RlktServiceController
{
    class Service : INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        public int ID { get; set; }
        public string Name { get; set; }
        public string PathExe { get; set; }
        public string Args { get; set; }
        public ServiceStatus _status;
        public ServiceStatus Status { 
            get 
            {
                return _status; 
            } 
            set 
            {
                _status = value;
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("StatusColor");
                NotifyPropertyChanged("CanStart"); 
                NotifyPropertyChanged("CanStop"); 
                NotifyPropertyChanged("CanRestart");  
            } 
        }

        private Process process { get; set; }

        public Service()
        {
            Status = ServiceStatus.STOPPED;
        }

        public void Tick()
        {
            if (process == null)
                return;

            //Check if the process closed unexpectedly
            if(process.HasExited && Status != ServiceStatus.STOPPING && Status != ServiceStatus.ERROR)
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

        public void Control(ProcessEvent processEvent)
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
                process.StartInfo.RedirectStandardInput = true;
                process.Start();

                Thread.Sleep(100); //wait 100ms for the window to become available
                
                ShowWindow(process.MainWindowHandle, 2);

                Status = ServiceStatus.STARTING;

                Logger.Add("[OnStartProcess] Starting Service[{0}_{1}]", Name, ID.ToString());
            }
            catch (Exception err)
            {
                Status = ServiceStatus.ERROR;

                Logger.Add("[OnStartProcess] Error starting Service[{0}_{1}] Exception[{2}]", Name, ID.ToString(), err.Message);
            }
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


        #region Button and Status Bindings
        public bool CanStart
        {
            get
            {
                if (Status == ServiceStatus.ERROR ||
                    Status == ServiceStatus.STOPPED)
                    return true;

                return false;
            }
        }

        public bool CanStop
        {
            get
            {
                if (Status == ServiceStatus.RUNNING)
                    return true;

                return false;
            }
        }

        public bool CanRestart
        {
            get
            {
                if (Status == ServiceStatus.RUNNING ||
                    Status == ServiceStatus.ERROR)
                    return true;

                return false;
            }
        }

        public Brush StatusColor
        {
            get
            {
                switch (Status)
                {
                    case ServiceStatus.STOPPED:
                        return Brushes.Gray;

                    case ServiceStatus.STOPPING:
                    case ServiceStatus.ERROR: 
                        return Brushes.Red;

                    case ServiceStatus.RUNNING:
                        return Brushes.Green;

                    case ServiceStatus.STARTING:
                        return Brushes.DarkGreen;
                }

                return Brushes.Black;
            }
        }

        #endregion

        #region Refresh Bindings
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion
    }
}
