﻿using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class SwitchStatusCommandingClient : WcfSeviceFabricClientBase<ISwitchStatusCommandingContract>, ISwitchStatusCommandingContract
	{
		private static readonly string microserviceName = MicroserviceNames.MeasurementProviderService;
		private static readonly string listenerName = EndpointNames.SwitchStatusCommandingEndpoint;

		public SwitchStatusCommandingClient(WcfCommunicationClientFactory<ISwitchStatusCommandingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static SwitchStatusCommandingClient CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(microserviceName);
		}

		public static SwitchStatusCommandingClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(serviceUri, servicePartitionKey);
		}

		public Task SendCloseCommand(long gid)
		{
            return InvokeWithRetryAsync(client => client.Channel.SendCloseCommand(gid));
		}

		public Task SendOpenCommand(long gid)
		{
            return InvokeWithRetryAsync(client => client.Channel.SendOpenCommand(gid));
		}
	}
}