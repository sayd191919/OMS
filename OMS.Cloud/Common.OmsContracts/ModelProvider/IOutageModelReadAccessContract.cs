﻿using Common.CE;
using Common.CloudContracts;
using Common.OMS;
using Common.OMS.OutageModel;

using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using OMS.Common.PubSub;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelProvider
{
	[ServiceContract]
    public interface IOutageModelReadAccessContract : IService, IHealthChecker
    {

        [OperationContract]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        Task<IOutageTopologyModel> GetTopologyModel();
        
        [OperationContract]
        Task<Dictionary<long, long>> GetCommandedElements();
        
        [OperationContract]
        Task<Dictionary<long, long>> GetOptimumIsolatioPoints();

        [OperationContract]
        Task<Dictionary<long, CommandOriginType>> GetPotentialOutage();

        [OperationContract]
        Task<IOutageTopologyElement> GetElementById(long gid);
    }
}
