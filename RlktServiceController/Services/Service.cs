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
    public class Service : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PathExe { get; set; }
        public string Args { get; set; }
        public string EventLog { get; set; } = "";
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
                NotifyPropertyChanged("IsStateChanged");

                //If this instance is a server, broadcast the status change to all clients
                if (ServiceManager.GetServerType() == ServiceManagerType.SERVER)
                {
                    Remote_Network.PacketManagerServerClient.Instance.BroadcastUpdateServiceInfo(this);
                }
            } 
        }

        public Service()
        {
            Status = ServiceStatus.STOPPED;
        }

        public virtual void Tick() { throw new Exception("Implement me."); }
        public virtual void Control(ProcessEvent processEvent) { throw new Exception("Implement me."); }


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

        public bool IsStateChanged
        {
            get
            {
                if (Status != ServiceStatus.STOPPED)
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
