﻿using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts.ModelProvider
{
    [ServiceContract]
    public interface IScadaModelUpdateAccessContract : IService
    {
        [OperationContract]
        Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData);

        [OperationContract]
        Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData);

        [OperationContract]
        Task UpdateCommandDescription(long gid, CommandDescription commandDescription);

        //[OperationContract]
        //Task MakeAnalogEntryToMeasurementCache();

        //[OperationContract]
        //Task MakeDiscreteEntryToMeasurementCache();

        //[OperationContract]
        //Task UpdateCommandDescription();
    }
}
