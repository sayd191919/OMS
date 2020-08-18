﻿using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.ModelAccess
{
	public class ConsumerAccessClient : WcfSeviceFabricClientBase<IConsumerAccessContract>, IConsumerAccessContract
	{
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsConsumerAccessEndpoint;
        public ConsumerAccessClient(WcfCommunicationClientFactory<IConsumerAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static IConsumerAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ConsumerAccessClient, IConsumerAccessContract>(microserviceName);
        }

        public static IConsumerAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ConsumerAccessClient, IConsumerAccessContract>(serviceUri, servicePartitionKey);
        }

		public Task<Consumer> AddConsumer(Consumer consumer)
		{
			return InvokeWithRetryAsync(client => client.Channel.AddConsumer(consumer));
		}

		//TODO: solve serialization of Expression<Func<Consumer, bool>>
		//public Task<IEnumerable<Consumer>> FindConsumer(Expression<Func<Consumer, bool>> predicate)
		//{
		//	return InvokeWithRetryAsync(client => client.Channel.FindConsumer(predicate));
		//}

		public Task<IEnumerable<Consumer>> GetAllConsumers()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllConsumers());
		}

		public Task<Consumer> GetConsumer(long gid)
		{
			return InvokeWithRetryAsync(client => client.Channel.GetConsumer(gid));
		}

		public Task RemoveAllConsumers()
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveAllConsumers());
		}

		public Task RemoveConsumer(Consumer consumer)
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveConsumer(consumer));
		}

		public Task UpdateConsumer(Consumer consumer)
		{
			return InvokeWithRetryAsync(client => client.Channel.UpdateConsumer(consumer));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}