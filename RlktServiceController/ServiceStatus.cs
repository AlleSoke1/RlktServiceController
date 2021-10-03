using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController
{
    public enum ServiceStatus
    {
        STOPPED,
        STARTING,
        STOPPING,
        RUNNING,
        ERROR,
    }

    public enum ProcessEvent
    {
        START_PROCESS,
        STOP_PROCESS,
        RESET_PROCESS,
        REDIRECT_STDOUT,
        REDIRECT_STDERR,
    }

}
