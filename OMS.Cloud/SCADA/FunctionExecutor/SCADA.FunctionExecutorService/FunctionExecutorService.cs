﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.SCADA;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using Outage.Common;
using SCADA.FunctionExecutorImplementation;
using SCADA.FunctionExecutorImplementation.CommandEnqueuers;

namespace SCADA.FunctionExecutorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class FunctionExecutorService : StatelessService
    {
        private readonly ReadCommandEnqueuer readCommandEnqueuer;
        private readonly WriteCommandEnqueuer writeCommandEnqueuer;
        private readonly ModelUpdateCommandEnqueuer modelUpdateCommandEnqueuer;

        public FunctionExecutorService(StatelessServiceContext context)
            : base(context)
        {
            this.readCommandEnqueuer = new ReadCommandEnqueuer();
            this.writeCommandEnqueuer = new WriteCommandEnqueuer();
            this.modelUpdateCommandEnqueuer = new ModelUpdateCommandEnqueuer();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //return new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>()
            {
                //ScadaReadCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IReadCommandEnqueuer>(context,
                                                                              this.readCommandEnqueuer,
                                                                              WcfUtility.CreateTcpListenerBinding(),
                                                                              EndpointNames.ScadaReadCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaReadCommandEnqueuerEndpoint),

                //ScadaWriteCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IWriteCommandEnqueuer>(context,
                                                                               this.writeCommandEnqueuer,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.ScadaWriteCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaWriteCommandEnqueuerEndpoint),

                //ScadaModelUpdateCommandEnqueueurEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IModelUpdateCommandEnqueuer>(context,
                                                                                     this.modelUpdateCommandEnqueuer,
                                                                                     WcfUtility.CreateTcpListenerBinding(),
                                                                                     EndpointNames.ScadaModelUpdateCommandEnqueueurEndpoint);
                }, EndpointNames.ScadaModelUpdateCommandEnqueueurEndpoint)

            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            FunctionExecutorCycle functionExecutorCycle = new FunctionExecutorCycle();
            ScadaModelReadAccessClient readAccessClient = ScadaModelReadAccessClient.CreateClient();
            IScadaConfigData configData = await readAccessClient.GetScadaConfigData();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await functionExecutorCycle.Start();
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService] FunctionExecutorCycle executed.");
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService] Error: {e.Message}]");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.FunctionExecutionInterval), cancellationToken);
            }
        }
    }
}
