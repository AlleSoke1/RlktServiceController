using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController
{
    public enum ServiceManagerType
    {
        /// <summary>
        /// Standalone instance, has no network components and processes are started locally.
        /// This is the Default service.
        /// </summary>
        STANDALONE, 

        /// <summary>
        /// Standalone / Server instance, the network component will be enabled and clients can remotely control the services.
        /// </summary>
        SERVER,

        /// <summary>
        /// Client instance, as a client you can only connect to a service server and do operations on the services.
        /// </summary>
        CLIENT,
    };
}
