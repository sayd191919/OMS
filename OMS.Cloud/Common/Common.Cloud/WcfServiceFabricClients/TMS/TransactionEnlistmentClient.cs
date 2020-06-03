﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class TransactionEnlistmentClient : WcfSeviceFabricClientBase<ITransactionEnlistmentContract>, ITransactionEnlistmentContract
    {
        private static readonly string microserviceName = MicroserviceNames.TransactionManagerService;
        private static readonly string listenerName = "";

        public TransactionEnlistmentClient(WcfCommunicationClientFactory<ITransactionEnlistmentContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static TransactionEnlistmentClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<TransactionEnlistmentClient, ITransactionEnlistmentContract>(microserviceName);
            }
            else
            {
                return factory.CreateClient<TransactionEnlistmentClient, ITransactionEnlistmentContract>(serviceUri);
            }
        }

        #region ITransactionEnlistmentContract
        public bool Enlist(string actorName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}