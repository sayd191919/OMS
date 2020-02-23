﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageSimulatorContract
    {
        [OperationContract]
        bool ResolvedOutage(long outageElementId);

        [OperationContract]
        bool IsOutageElement(long outageElementId);
    }
}