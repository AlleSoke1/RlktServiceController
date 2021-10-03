using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController
{
    class ServiceManager
    {
        List<Service> services = new List<Service>();
        bool startAllServices;

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

            int serviceIndex = 0;
            while(true)
            {
                string sectionName = string.Format("Service{0}", serviceIndex);
                if(ini.KeyExists("Name", sectionName) == false)
                    break;

                Service service = new Service();
                service.ID      = serviceIndex;
                service.Name    = ini.Read("Name", sectionName);
                service.PathExe = ini.Read("PathExe", sectionName);
                service.Args    = ini.Read("Args", sectionName);
                AddService(service);

                serviceIndex++;
            }
        }

        public Service GetServiceByID(int id)
        {
            foreach(Service service in services)
            {
                if (service.ID == id)
                    return service;
            }

            return null;
        }

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
        public static void StartAll() => manager.startAllServices = true;
        public static void StopAll() => manager.StopAllServices();
    }
}
