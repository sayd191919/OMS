﻿using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class ResolveOutageClient: WcfSeviceFabricClientBase<IResolveOutageContract>,IResolveOutageContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.ResolveOutageEndpoint;
        public ResolveOutageClient(WcfCommunicationClientFactory<IResolveOutageContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ResolveOutageClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ResolveOutageClient, IResolveOutageContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ResolveOutageClient, IResolveOutageContract>(serviceUri, servicePartition);
            }
        }

        public Task<bool> ResolveOutage(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.ResolveOutage(outageId));
        }
    }
}