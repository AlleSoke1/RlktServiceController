using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController
{
    class Logger
    {
        public static Logger logger = new Logger();
        public static void Add(string message, params string[] param) => logger.AddEntry(string.Format(message,param));
        public static void Add(string message) => logger.AddEntry(message);

        //
        public Queue<string> logList = new Queue<string>();
        public void AddEntry(string message) => logList.Enqueue( message + Environment.NewLine);
    }
}
