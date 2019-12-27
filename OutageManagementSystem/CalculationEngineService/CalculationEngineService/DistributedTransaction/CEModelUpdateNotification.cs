﻿using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;

namespace CalculationEngineService.DistributedTransaction
{
    public class CEModelUpdateNotification : ModelUpdateNotification
    {
        //public static CalculationEngineService calculationEngineService = null;

        public CEModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.CalculationEngineService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            //TODO: CE notification logic

            using (TransactionEnlistmentProxy transactionEnlistmentProxy = TransactionEnlistmentProxy)
            {
                if(transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Enlist(ActorName);
                }
                else
                {
                    string message = "TransactionEnlistmentProxy is null";
                    logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
            }

                
            logger.LogInfo("Calculation Engine SUCCESSFULLY notified about network model update.");
            return true;
        }
    }
}
