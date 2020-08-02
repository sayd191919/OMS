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
    public class ValidateResolveConditionsClient : WcfSeviceFabricClientBase<IValidateResolveConditionsContract>, IValidateResolveConditionsContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.ValidateResolveConditionsEndpoint;
        public ValidateResolveConditionsClient(WcfCommunicationClientFactory<IValidateResolveConditionsContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ValidateResolveConditionsClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ValidateResolveConditionsClient, IValidateResolveConditionsContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ValidateResolveConditionsClient, IValidateResolveConditionsContract>(serviceUri, servicePartition);
            }
        }
        public Task<bool> ValidateResolveConditions(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.ValidateResolveConditions(outageId));
        }
    }
}