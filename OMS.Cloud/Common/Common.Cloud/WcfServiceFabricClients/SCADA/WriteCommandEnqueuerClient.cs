﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using Outage.Common;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class WriteCommandEnqueuerClient : WcfSeviceFabricClientBase<IWriteCommandEnqueuer>, IWriteCommandEnqueuer
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaFunctionExecutorService;
        private static readonly string listenerName = EndpointNames.ScadaWriteCommandEnqueuerEndpoint;

        public WriteCommandEnqueuerClient(WcfCommunicationClientFactory<IWriteCommandEnqueuer> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static WriteCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuer>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuer>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunctions)
        {
            return InvokeWithRetryAsync(client => client.Channel.EnqueueWriteCommand(modbusFunctions));
        }
        #endregion
    }
}