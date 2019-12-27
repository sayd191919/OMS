﻿using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Common.PubSub;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Outage.SCADA.ModBus.Connection
{
    public delegate void UpdatePointDelegate(PointType type, ushort pointAddres, ushort newValue);

    public class FunctionExecutor
    {

        private ILogger logger = LoggerWrapper.Instance;
        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        private TcpConnection connection;
        private ushort tcpPort;
        private Thread connectionProcess;
        private bool threadCancellationSignal = true;
        private int numberOfTries = 0;
        private IModBusFunction currentCommand;
        private ConnectionState connectionState = ConnectionState.DISCONNECTED;
        private AutoResetEvent commandEvent;
        private ConcurrentQueue<IModBusFunction> commandQueue;

        #region Proxies
        private PublisherProxy publisherProxy = null;

        public PublisherProxy PublisherProxy
        {
            get
            {
                //TODO: diskusija statefull vs stateless

                try
                {
                    if (publisherProxy != null)
                    {
                        publisherProxy.Abort();
                        publisherProxy = null;
                    }

                    publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint);
                    publisherProxy.Open();

                }
                catch (Exception ex)
                {
                    string message = $"Exception on PublisherProxy initialization. Message: {ex.Message}";
                    logger.LogError(message, ex);
                    publisherProxy = null;
                }

                return publisherProxy;
            }
        }
        #endregion

        public event UpdatePointDelegate UpdatePointEvent;
        
        public FunctionExecutor(ushort tcpPort)
        {
            this.tcpPort = tcpPort;
            commandQueue = new ConcurrentQueue<IModBusFunction>();
            connection = new TcpConnection(this.tcpPort);
            commandEvent = new AutoResetEvent(false);
            connectionProcess = new Thread(ConnectionProcessThread)
            {
                Name = "Communication with SIM"
            };

            connectionProcess.Start();
        }

        public void EnqueueCommand(ModbusFunction modbusFunction)

        {
            if (connectionState == ConnectionState.CONNECTED)
            {
                this.commandQueue.Enqueue(modbusFunction);
                this.commandEvent.Set();
            }
        }

        private void ConnectionProcessThread()
        {
            logger.LogInfo("ConnectionProcessThread is started");


            Console.WriteLine("Establishing connection");
            logger.LogInfo("Establishing connection");
            
            while (this.threadCancellationSignal)
            {
                try
                {
                    if (this.connectionState == ConnectionState.DISCONNECTED)
                    {
                        this.numberOfTries = 0;
                        this.connection.Connect();
                        while (numberOfTries < 10)
                        {
                            if (this.connection.CheckState())
                            {
                                this.connectionState = ConnectionState.CONNECTED;
                                this.numberOfTries = 0;
                                Console.WriteLine("Connected");
                                logger.LogInfo("Connected");
                                break;
                            }
                            else
                            {
                                numberOfTries++;
                                if (this.numberOfTries == 10)
                                {
                                    this.connection.Disconnect();
                                    this.connectionState = ConnectionState.DISCONNECTED;
                                    logger.LogInfo($"Disconenting after {numberOfTries} tries.");
                                    threadCancellationSignal = true;
                                    continue;
                                }

                                logger.LogInfo($"Establishing connection... Try: {numberOfTries}");
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    else
                    {
                        logger.LogDebug("Connected and waiting for command event.");

                        this.commandEvent.WaitOne();

                        logger.LogDebug("Command event happened.");

                        while (commandQueue.TryDequeue(out this.currentCommand))
                        {
                            this.connection.SendBytes(this.currentCommand.PackRequest());

                            byte[] header = this.connection.RecvBytes(7);
                            int payLoadSize = 0;
                            
                            unchecked
                            {
                                payLoadSize = IPAddress.NetworkToHostOrder((short)BitConverter.ToInt16(header, 4));
                            }

                            byte[] payload = this.connection.RecvBytes(payLoadSize - 1);
                            byte[] message = new byte[header.Length + payload.Length];
                            
                            Buffer.BlockCopy(header, 0, message, 0, 7);
                            Buffer.BlockCopy(payload, 0, message, 7, payload.Length);
                            Dictionary<Tuple<PointType, ushort>, ushort> pointsToUpdate = this.currentCommand.ParseResponse(message);

                            foreach (Tuple<PointType, ushort> pointKey in pointsToUpdate.Keys)
                            {
                                ushort address = pointKey.Item2;
                                ushort newValue = pointsToUpdate[pointKey];

                                ConfigItem point = UpdatePoints(address, newValue);

                                ModelCode modelCode = resourcesDesc.GetModelCodeFromId(point.Gid);

                                Topic topic;

                                if (modelCode == ModelCode.ANALOG)
                                {
                                    topic = Topic.MEASUREMENT;

                                }
                                else if(modelCode == ModelCode.DISCRETE)
                                {
                                    topic = Topic.SWITCH_STATUS;
                                }
                                else
                                {
                                    string errMessage = $"Config item type is neither analog nor digital. Config item type is: {modelCode}.";
                                    logger.LogError(errMessage);
                                    throw new Exception(errMessage); 
                                }

                                SCADAMessage scadaMessage = new SCADAMessage()
                                {
                                    Gid = point.Gid,
                                    Value = point.CurrentValue,
                                };

                                SCADAPublication scadaPublication = new SCADAPublication()
                                {
                                    Topic = topic,
                                    Message = scadaMessage,
                                };

                                PublisherProxy.Publish(scadaPublication);
                            }
                        }
                    }
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != 10054)
                    {
                        throw se;
                    }
                    currentCommand = null;
                    this.connectionState = ConnectionState.DISCONNECTED;
                    this.connection.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this.connectionState = ConnectionState.DISCONNECTED;
                    this.connection.Disconnect();
                }
            }

            logger.LogInfo("ConnectionProcessThred is stopped.");
        }

        private ConfigItem UpdatePoints(ushort address, ushort newValue)
        {
            ConfigItem point = DataModelRepository.Instance.Points.Values.Where(x => x.Address == address).First();
            point.CurrentValue = newValue;

            return point;
        }
    }
}