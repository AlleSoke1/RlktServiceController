using RlktServiceController.Remote_Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktServiceController.Services
{
    internal class RemoteService : Service
    {
        public override void Tick()
        {
        }

        public override void Control(ProcessEvent processEvent)
        {
            switch (processEvent)
            {
                case ProcessEvent.START_PROCESS: SendServiceControl(PServiceControl.ServiceOperation.START); break;
                case ProcessEvent.STOP_PROCESS:  SendServiceControl(PServiceControl.ServiceOperation.STOP); break;
            }
        }

        void SendServiceControl(PServiceControl.ServiceOperation operation)
        {
            PServiceControl serviceControl = new PServiceControl();

            serviceControl.serviceId = ID;
            serviceControl.operation = operation;

            NetworkClient.client.SendPacket(serviceControl);
        }

    }
}
