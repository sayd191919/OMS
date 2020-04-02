﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    class ScadaIntegrityUpdateClient : WcfSeviceFabricClientBase<IScadaIntegrityUpdateContract>, IScadaIntegrityUpdateContract
    {
        public ScadaIntegrityUpdateClient(WcfCommunicationClientFactory<IScadaIntegrityUpdateContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaModelProviderService] is string scadaModelProviderServiceName)
            {
                serviceUri = new Uri(scadaModelProviderServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaIntegrityUpdateContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaIntegrityUpdateClient(factory, serviceUri);
        }

        #region IScadaIntegrityUpdateContract
        public Dictionary<Topic, SCADAPublication> GetIntegrityUpdate()
        {
            throw new NotImplementedException();
        }

        public SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
