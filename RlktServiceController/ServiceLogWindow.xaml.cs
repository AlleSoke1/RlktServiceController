using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RlktServiceController
{
    /// <summary>
    /// Interaction logic for ServiceLog.xaml
    /// </summary>
    public partial class ServiceLogWindow : Window
    {
        List<ServiceLog> activeServiceLogs = new List<ServiceLog>();

        public ServiceLogWindow()
        {
            InitializeComponent();
        }

        public void ShowLogForService(Service service)
        {
            //Open the log window
            if (IsActive == false)
                Show();

            Focus();

            //Check if service already exists, if exists, select the tab
            foreach (ServiceLog serviceLog in activeServiceLogs)
            {
                if(serviceLog.service == service)
                {
                    tabServiceLogs.SelectedItem = serviceLog.tabItem;
                    return;
                }
            }

            //Create a new service log
            ServiceLog log = new ServiceLog();
            log.service = service;
            log.tabID = -1;

            //Generate a new tab
            TabItem newTabItem = new TabItem
            {
                HeaderTemplate = FindResource("TabHeader") as DataTemplate,
                ContentTemplate = FindResource("TabItem") as DataTemplate,
                Header = log,
                Content = service,
            };

            //Add and store the tabid
            log.tabID = tabServiceLogs.Items.Add(newTabItem);

            //Add it to the active tabs list
            log.tabItem = newTabItem;
            activeServiceLogs.Add(log);

            //Select added tab
            tabServiceLogs.SelectedIndex = log.tabID;
        }

        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnCloseTab(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                ServiceLog data = btn.DataContext as ServiceLog;
                if (data != null)
                {
                    tabServiceLogs.Items.Remove(data.tabItem);
                    activeServiceLogs.Remove(data);
                }
            }
        }
    }
}
