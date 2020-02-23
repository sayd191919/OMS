﻿using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageAccessProxy : BaseProxy<IOutageAccessContract>, IOutageAccessContract, IOutageService
    {
        public OutageAccessProxy(string endpointName)
            : base(endpointName)
        {
        }

        public List<ActiveOutage> GetActiveOutages()
        {
            List<ActiveOutage> outageModels;
            try
            {
                outageModels = Channel.GetActiveOutages();
            }
            catch (Exception e)
            {
                string message = "Exception in GetActiveOutages() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return outageModels;
        }

        public List<ArchivedOutage> GetArchivedOutages()
        {
            List<ArchivedOutage> outageModels;
            try
            {
                outageModels = Channel.GetArchivedOutages();
            }
            catch (Exception e)
            {
                string message = "Exception in GetArchivedOutages() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return outageModels;
        }
    }
}