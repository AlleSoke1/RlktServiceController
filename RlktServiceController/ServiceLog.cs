using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RlktServiceController
{
    public class ServiceLog
    {
        public int tabID { get; set; }
        public string tabName { get { return service.Name; } }
        public Service service { get; set; }
        public TabItem tabItem { get; set; }
    }
}
