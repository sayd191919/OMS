﻿using OMS.Common.SCADA;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OMS.Common.ScadaContracts.DataContracts.ModbusFunctions;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.Commanding;
using OMS.Common.WcfClient.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using System.ServiceModel;

namespace SCADA.CommandingImplementation
{
    public class CommandingProvider : IScadaCommandingContract
    {
        private readonly string baseLoggString;

        private WriteCommandEnqueuerClient commandEnqueuerClient;
        private ScadaModelReadAccessClient scadaModelReadAccessClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public CommandingProvider()
        {
            this.baseLoggString = $"{typeof(CommandingProvider)} [{this.GetHashCode()}] =>";

            this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
            this.scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
        }

        #region IScadaCommandingContract
        public async Task SendSingleAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await this.scadaModelReadAccessClient.GetGidToPointItemMap();

            if (gidToPointItemMap == null)
            {
                string message = $"SendSingleAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!gidToPointItemMap.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = gidToPointItemMap[gid];

            if (pointItem is IAnalogPointItem analogPointItem && pointItem.RegisterType == PointType.ANALOG_OUTPUT)
            {
                try
                {
                    int modbusValue = analogPointItem.EguToRawValueConversion(commandingValue);
                    await SendSingleCommand(pointItem, modbusValue, commandOriginType);
                }
                catch (Exception e)
                {
                    string message = $"Exception in SendAnalogCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
            else
            {
                string message = $"Either RegistarType of entity with gid: 0x{gid:X16} is not ANALOG_OUTPUT or entity does not implement IAnalogPointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }
        }

        public async Task SendMultipleAnalogCommand(Dictionary<long, float> commandingValues, CommandOriginType commandOriginType)
        {
            ushort startAddress = 1; //EasyModbus spec
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await this.scadaModelReadAccessClient.GetGidToPointItemMap();
            Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await this.scadaModelReadAccessClient.GetAddressToGidMap();

            if (gidToPointItemMap == null)
            {
                string message = $"SendMultipleAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            int analogOutputCount = addressToGidMap[(short)PointType.ANALOG_OUTPUT].Count;
            int[] multipleCommandingValues = new int[addressToGidMap[(short)PointType.ANALOG_OUTPUT].Count];

            //for (ushort address = 1; address <= analogOutputCount; address++)
            //{
            foreach(ushort address in addressToGidMap[(short)PointType.ANALOG_OUTPUT].Keys)
            { 
                long gid = addressToGidMap[(short)PointType.ANALOG_OUTPUT][address];

                if (!gidToPointItemMap.ContainsKey(gid))
                {
                    string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }
                else if(!(gidToPointItemMap[gid] is IAnalogPointItem analogPointItem))
                {
                    string message = $"Entity with gid: 0x{gid:X16} does not implement IAnalogPointItem interface.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }
                else
                {
                    int commandingValue;

                    if (commandingValues.ContainsKey(gid))
                    {
                        commandingValue = analogPointItem.EguToRawValueConversion(commandingValues[gid]);
                    }
                    else
                    {
                        commandingValue = analogPointItem.CurrentRawValue;
                    }

                    if(address <= analogOutputCount)
                    {
                        multipleCommandingValues[address - 1] = commandingValue;
                    }
                    else
                    {
                        throw new Exception("PointItem addresses of ANALOG entities are not successive. This can happen due to cim/xml being invalid.");
                    }
                }
            }

            try
            {
                await SendMultipleCommand(ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS, startAddress, multipleCommandingValues, commandOriginType);
            }
            catch (Exception e)
            {
                string message = $"Exception in SendMultipleAnalogCommand() method.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }

        public async Task SendSingleDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await this.scadaModelReadAccessClient.GetGidToPointItemMap();

            if (gidToPointItemMap == null)
            {
                string message = $"SendSingleDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!gidToPointItemMap.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = gidToPointItemMap[gid];

            if (pointItem is IDiscretePointItem && pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
            {
                try
                {
                    await SendSingleCommand(pointItem, commandingValue, commandOriginType);
                }
                catch (Exception e)
                {
                    string message = $"Exception in SendDiscreteCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
            else
            {
                string message = $"RegistarType of entity with gid: 0x{gid:X16} is not DIGITAL_OUTPUT or entity does not implement IDiscretePointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

        }

        public async Task SendMultipleDiscreteCommand(Dictionary<long, ushort> commandingValues, CommandOriginType commandOriginType)
        {
            ushort startAddress = 1; //EasyModbus spec
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await this.scadaModelReadAccessClient.GetGidToPointItemMap();
            Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await this.scadaModelReadAccessClient.GetAddressToGidMap();

            if (gidToPointItemMap == null)
            {
                string message = $"SendMultipleDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            int digitalOutputCount = addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Count;
            int[] multipleCommandingValues = new int[addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Count];

            //for (ushort address = 1; address <= digitalOutputCount; address++)
            foreach (ushort address in addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Keys)
            {
                long gid = addressToGidMap[(short)PointType.DIGITAL_OUTPUT][address];

                if (!gidToPointItemMap.ContainsKey(gid))
                {
                    string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }
                else if (!(gidToPointItemMap[gid] is IDiscretePointItem discretePointItem))
                {
                    string message = $"Entity with gid: 0x{gid:X16} does not implement IDiscretePointItem interface.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }
                else
                {
                    int commandingValue;

                    if (commandingValues.ContainsKey(gid))
                    {
                        commandingValue = commandingValues[gid];
                    }
                    else
                    {
                        commandingValue = discretePointItem.CurrentValue;
                    }

                    if (address <= digitalOutputCount)
                    {
                        multipleCommandingValues[address - 1] = commandingValue;
                    }
                    else
                    {
                        throw new Exception("PointItem addresses of DISCRETE entities are not successive. This can happen due to cim/xml being invalid.");
                    }
                }
            }

            try
            {
                await SendMultipleCommand(ModbusFunctionCode.WRITE_MULTIPLE_COILS, startAddress, multipleCommandingValues, commandOriginType);
            }
            catch (Exception e)
            {
                string message = $"Exception in SendMultipleDiscreteCommand() method.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }
        #endregion IScadaCommandingContract

        private async Task SendSingleCommand(IScadaModelPointItem pointItem, int commandingValue, CommandOriginType commandOriginType, bool isRetry = false)
        { 
            try
            {
                ModbusFunctionCode functionCode;

                if (pointItem.RegisterType == PointType.ANALOG_OUTPUT)
                {
                    functionCode = ModbusFunctionCode.WRITE_SINGLE_REGISTER;
                }
                else if (pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
                {
                    functionCode = ModbusFunctionCode.WRITE_SINGLE_COIL;
                }
                else
                {
                    string errorMessage = $"{baseLoggString} SendSingleCommand => Commanding arguments are not valid. Registry type: {pointItem.RegisterType}, expected: {PointType.ANALOG_OUTPUT}, {PointType.DIGITAL_OUTPUT}";
                    Logger.LogError(errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                IWriteModbusFunction modbusFunction = new WriteSingleFunction(functionCode, pointItem.Address, commandingValue, commandOriginType);
                await this.commandEnqueuerClient.EnqueueWriteCommand(modbusFunction);

                string message = $"{baseLoggString} SendSingleCommand => Command SUCCESSFULLY enqueued. Function code: {modbusFunction.FunctionCode}, Origin: {modbusFunction.CommandOrigin}";
                Logger.LogInformation(message);
            }
            catch (CommunicationObjectFaultedException e)
            {
                string message = $"{baseLoggString} SendSingleCommand => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                await Task.Delay(2000);

                this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                this.scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
                await SendSingleCommand(pointItem, commandingValue, commandOriginType, true);
                //todo: different logic on multiple rety?
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    await Task.Delay(2000);

                    this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                    this.scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
                    await SendSingleCommand(pointItem, commandingValue, commandOriginType, true);
                }
                else
                {
                    string message = $"{baseLoggString} SendSingleCommand => Exception in SendCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
        }

        private async Task SendMultipleCommand(ModbusFunctionCode functionCode, ushort startAddress, int[] commandingValues, CommandOriginType commandOriginType, bool isRetry = false)
        {
            try
            { 
                IWriteModbusFunction modbusFunction = new WriteMultipleFunction(functionCode, startAddress, commandingValues, commandOriginType);
                await this.commandEnqueuerClient.EnqueueWriteCommand(modbusFunction);

                string message = $"Command SUCCESSFULLY enqueued. Function code: {modbusFunction.FunctionCode}, Origin: {modbusFunction.CommandOrigin}";
                Logger.LogInformation(message);
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    await Task.Delay(2000);

                    this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                    await SendMultipleCommand(functionCode, startAddress, commandingValues, commandOriginType, true);
                }
                else
                {
                    string message = $"Exception in SendCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
        }
    }
}
