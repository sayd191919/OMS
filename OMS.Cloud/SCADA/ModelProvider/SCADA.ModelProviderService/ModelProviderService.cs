﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;
using SCADA.ModelProviderImplementation;
using SCADA.ModelProviderImplementation.ContractProviders;
using SCADA.ModelProviderImplementation.DistributedTransaction;

namespace SCADA.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        private readonly string baseLogString;

        private readonly IScadaModelReadAccessContract modelReadAccessProvider;
        private readonly IScadaModelUpdateAccessContract modelUpdateAccessProvider;
        private readonly IScadaIntegrityUpdateContract integrityUpdateProvider;
        private readonly INotifyNetworkModelUpdateContract scadaNotifyNetworkModelUpdate;
        private readonly ITransactionActorContract scadaTransactionActorProviders;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
            this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                //DONE THIS WAY (in this order) BECAUSE: there is a mechanism that tracks the initialization process of reliable collections, which is set in constructors of these classes
                var modelResourceDesc = new ModelResourcesDesc();
                var enumDescs = new EnumDescs();

                this.modelReadAccessProvider = new ModelReadAccessProvider(this.StateManager);
                this.modelUpdateAccessProvider = new ModelUpdateAccessProvider(this.StateManager);
                this.integrityUpdateProvider = new IntegrityUpdateProvider(this.StateManager);
                this.scadaNotifyNetworkModelUpdate = new ScadaNotifyNetworkModelUpdate(this.StateManager);
                this.scadaTransactionActorProviders = new ScadaTransactionActor(this.StateManager, modelResourceDesc, enumDescs);

                string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
                Logger.LogError(errorMessage, e);
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //return new ServiceReplicaListener[0];
            return new[]
            {
                //ScadaModelReadAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelReadAccessContract>(context,
                                                                            this.modelReadAccessProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelReadAccessEndpoint);
                }, EndpointNames.ScadaModelReadAccessEndpoint),

                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelUpdateAccessContract>(context,
                                                                            this.modelUpdateAccessProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelUpdateAccessEndpoint);
                }, EndpointNames.ScadaModelUpdateAccessEndpoint),

                //SCADAIntegrityUpdateEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaIntegrityUpdateContract>(context,
                                                                           this.integrityUpdateProvider,
                                                                           WcfUtility.CreateTcpListenerBinding(),
                                                                           EndpointNames.ScadaIntegrityUpdateEndpoint);
                }, EndpointNames.ScadaIntegrityUpdateEndpoint),

                //SCADAModelUpdateNotifierEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifyNetworkModelUpdateContract>(context,
                                                                            this.scadaNotifyNetworkModelUpdate,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.TmsNotifyNetworkModelUpdateEndpoint);
                }, EndpointNames.TmsNotifyNetworkModelUpdateEndpoint),

                //SCADATransactionActorEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                            this.scadaTransactionActorProviders,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.TmsTransactionActorEndpoint);
                }, EndpointNames.TmsTransactionActorEndpoint),
            };
        }

        protected async override Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                //InitializeReliableCollections();
                string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
                Logger.LogInformation(errorMessage, e);
            }
        }

        private void InitializeReliableCollections()
        {
            Task[] tasks = new Task[]
            {
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, ScadaModelPointItem>>(tx, ReliableDictionaryNames.GidToPointItemMap);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, ScadaModelPointItem>>(tx, ReliableDictionaryNames.IncomingGidToPointItemMap);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<ushort, long>>>(tx, ReliableDictionaryNames.AddressToGidMap);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<ushort, long>>>(tx, ReliableDictionaryNames.IncomingAddressToGidMap);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, CommandDescription>>(tx, ReliableDictionaryNames.CommandDescriptionCache);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, ModbusData>>(tx, ReliableDictionaryNames.MeasurementsCache);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>(tx, ReliableDictionaryNames.InfoCache);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<byte, List<long>>>(tx, ReliableDictionaryNames.ModelChanges);
                        await tx.CommitAsync();
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
