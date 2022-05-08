using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RlktServiceController.Remote_Network;

namespace RlktServiceController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServiceLogWindow serviceLogWindow = null;
        public MainWindow()
        {
            InitializeComponent();
            InitializeServiceList();
            InitializeNetwork();
            CompositionTarget.Rendering += MainLoop;
            mainWindow = this;
        }

        public void MainLoop(object sender, EventArgs e)
        {
            //process services main loop
            ServiceManager.Process();

            //process networking
            NetworkClient.Process();
            NetworkServer.Process();

            //process packets
            if (ServiceManager.GetServerType() == ServiceManagerType.CLIENT)
                PacketManagerServerClient.Process();
            if (ServiceManager.GetServerType() == ServiceManagerType.SERVER)
                PacketManagerClientServer.Process();

            //process logger
            if (Logger.logger.logList.Count > 0)
            {
                log.AppendText(Logger.logger.logList.Dequeue());
            }

        }

        public void InitializeServiceList()
        {
            ServiceManager.Initialize();
            foreach(Service service in ServiceManager.GetServices())
            {
                serviceList.Items.Add(service);
            }

            UpdateTitleByInstanceType();
        }

        public void InitializeNetwork()
        {
            if(ServiceManager.GetServerType() == ServiceManagerType.CLIENT)
                NetworkClient.Initialize();

            if (ServiceManager.GetServerType() == ServiceManagerType.SERVER)
                NetworkServer.Initialize();
        }

        private void UpdateTitleByInstanceType()
        {
            if (ServiceManager.GetServerType() == ServiceManagerType.STANDALONE)
                Title += " - [Standalone]";

            if (ServiceManager.GetServerType() == ServiceManagerType.SERVER)
                Title += " - [Server Instance]"; 

            if (ServiceManager.GetServerType() == ServiceManagerType.CLIENT)
                Title += " - [Client/Remote Instance]";
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            Service service = (sender as Button).DataContext as Service;
            service.Control(ProcessEvent.START_PROCESS);
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            Service service = (sender as Button).DataContext as Service;
            service.Control(ProcessEvent.STOP_PROCESS);
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Service service = (sender as Button).DataContext as Service;
            service.Control(ProcessEvent.RESET_PROCESS);
        }

        private void BtnStartAll_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.StartAll();
            Logger.Add("Starting all services.");
        }

        private void BtnStopAll_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.StopAll();
            Logger.Add("Stopping all services.");
        }

        /// <summary>
        /// Prevent window from closing by mistake
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Do you want to exit?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            Service service = (sender as Button).DataContext as Service;

            if (serviceLogWindow == null || serviceLogWindow.IsActive == false)
                serviceLogWindow = new ServiceLogWindow();

            serviceLogWindow.ShowLogForService(service);
        }

        public void OnRemoteServiceReceived(Service service)
        {
            if (serviceList.Items.Contains(service) == false)
            {
                serviceList.Items.Add(service);
            }
        }

        public static MainWindow mainWindow = null;
    }
}
