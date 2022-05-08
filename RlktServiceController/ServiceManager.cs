using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RlktServiceController.Remote_Network;
using RlktServiceController.Services;
using RlktServiceControllerCommon;

namespace RlktServiceController
{
    class ServiceManager
    {
        List<Service> services = new List<Service>();
        bool startAllServices;
        ServiceManagerType managerType = ServiceManagerType.STANDALONE;

        /// <summary>
        /// Adds a service to the service manager.
        /// </summary>
        /// <param name="service">Service to be added to the list</param>
        public void AddService(Service service) => services.Add(service);

        /// <summary>
        /// Main loop Tick() function
        /// </summary>
        public void Tick()
        {
            bool isPrevServiceStarted = false;
            foreach(Service service in services)
            {
                service.Tick();
                
                if(startAllServices && (isPrevServiceStarted || service.ID == 0) && service.Status == ServiceStatus.STOPPED)
                {
                    service.Control(ProcessEvent.START_PROCESS);
                }

                isPrevServiceStarted = (service.Status == ServiceStatus.RUNNING);
            }

            CheckIfAllServicesAreStarted();
        }

        /// <summary>
        /// Create a remote service, hosted on another instance that is a server.
        /// </summary>
        /// <param name="serviceInfo"></param>
        internal void CreateRemoteService(PServiceInfo serviceInfo)
        {
            if (GetServiceByID(serviceInfo.serviceId) != null)
            {
                Service service = GetServiceByID(serviceInfo.serviceId);
                service.Status = (ServiceStatus)serviceInfo.serviceStatus;

                Logger.Add($"[ServiceUpdate] Service {serviceInfo.serviceId}/{serviceInfo.serviceName} status updated => {service.Status}!");
            }
            else
            {
                RemoteService service = new RemoteService();
                service.ID = serviceInfo.serviceId;
                service.Name = serviceInfo.serviceName;
                service.Status = (ServiceStatus)serviceInfo.serviceStatus;
                AddService(service);

                MainWindow.mainWindow?.OnRemoteServiceReceived(service);

                Logger.Add($"[NewService] Service {serviceInfo.serviceId}/{serviceInfo.serviceName} with status {service.Status} added!");
            }
        }

        /// <summary>
        /// Check if all services are started to stop the auto starting of services. 
        /// </summary>
        public void CheckIfAllServicesAreStarted()
        {
            if (startAllServices == false)
                return;

            bool bAllStarted = true;
            foreach (Service service in services)
            {
                if (service.Status != ServiceStatus.RUNNING)
                {
                    bAllStarted = false;
                    break;
                }
            }

            if (bAllStarted)
            {
                startAllServices = false;
                Logger.Add("All services automatically started successfully!");
            }
        }

        /// <summary>
        /// Load services configuration file
        /// </summary>
        public void LoadServiceConfig()
        {
            IniFile ini = new IniFile("ServiceConfig.ini");

            string sectionName = "Network";

            //Check if its a client instance.
            if (ini.KeyExists("IsClientInstance", "Network"))
            {
                bool isClientInstance = Convert.ToBoolean(ini.Read("IsClientInstance", sectionName));
                if (isClientInstance)
                {
                    string connectIP = ini.Read("ConnectIP", sectionName);
                    string connectPort = ini.Read("ConnectPort", sectionName);
                    int iPort = Convert.ToInt32(connectPort.Length > 0 ? connectPort.ToString() : RlktServiceControllerCommon.CommonConfig.DefaultPort.ToString());

                    if (connectIP.Length > 0 && iPort != 0)
                    {
                        NetworkClient.client.SetConnectInfo(connectIP, iPort);
                    }

                    managerType = ServiceManagerType.CLIENT;

                    CommonConfig.NetworkSharedKey = ini.Read("SharedKey", sectionName);

                    return; //Stop here, don't load services as its not necessary for a client only instance
                }
            }

            //Check if its a server instance.
            if (ini.KeyExists("IsServerInstance", "Network"))
            {
                bool isServerInstance = Convert.ToBoolean(ini.Read("IsServerInstance", sectionName));
                if (isServerInstance)
                {
                    string listenIP = ini.Read("ListenIP", sectionName);
                    string listenPort = ini.Read("ListenPort", sectionName);
                    int iPort = Convert.ToInt32(listenPort.Length > 0 ? listenPort.ToString() : RlktServiceControllerCommon.CommonConfig.DefaultPort.ToString());

                    if (listenIP.Length > 0 && iPort != 0)
                    {
                        NetworkServer.server.SetListenInfo(listenIP, iPort);
                    }

                    managerType = ServiceManagerType.SERVER;

                    CommonConfig.NetworkSharedKey = ini.Read("SharedKey", sectionName);
                }
            }

            int serviceIndex = 0;
            while(true)
            {
                sectionName = string.Format("Service{0}", serviceIndex);
                if(ini.KeyExists("Name", sectionName) == false)
                    break;

                LocalService service = new LocalService();
                service.ID      = serviceIndex;
                service.Name    = ini.Read("Name", sectionName);
                service.PathExe = ini.Read("PathExe", sectionName);
                service.Args    = ini.Read("Args", sectionName);
                AddService(service);

                serviceIndex++;
            }
        }

        /// <summary>
        /// Get a Service object by its service id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Service GetServiceByID(int id)
        {
            foreach(Service service in services)
            {
                if (service.ID == id)
                    return service;
            }

            return null;
        }

        /// <summary>
        /// Start all services
        /// </summary>
        public void StartAllServices()
        {
            manager.startAllServices = true;
        }

        /// <summary>
        /// Stop all services
        /// </summary>
        public void StopAllServices()
        {
            foreach (Service service in services)
            {
                service.Control(ProcessEvent.STOP_PROCESS);
            }
        }

        //
        public static ServiceManager manager = new ServiceManager();
        public static void Process() => manager.Tick();
        public static List<Service> GetServices() => manager.services;
        public static Service GetService(int id) => manager.GetServiceByID(id);
        public static void Initialize() => manager.LoadServiceConfig();
        public static void StartAll() => manager.StartAllServices();
        public static void StopAll() => manager.StopAllServices();
        public static ServiceManagerType GetServerType() => manager.managerType;

    }
}
