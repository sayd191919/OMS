﻿using Common.OMS;
using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.OmsContracts.OutageSimulator;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.OMS;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using OutageDatabase.Repository;
using System;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
    public class SendRepairCrewService : ISendRepairCrewContract
    {
        private IOutageTopologyModel outageModel;
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private UnitOfWork dbContext;
        private OutageMessageMapper outageMessageMapper;
        private OutageLifecycleHelper outageLifecycleHelper;
        private IOutageModelReadAccessContract outageModelReadAccessClient;
        
        public SendRepairCrewService(UnitOfWork dbContext)
        {
            this.dbContext = dbContext;
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        public async Task InitAwaitableFields()
        {
            this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.dbContext, this.outageModel);
        }
        public async Task<bool> SendRepairCrew(long outageId)
        {
            await InitAwaitableFields();
            OutageEntity outageDbEntity = null;

            try
            {
                outageDbEntity = dbContext.OutageRepository.Get(outageId);
            }
            catch (Exception e)
            {
                string message = "OutageModel::SendRepairCrew => exception in UnitOfWork.ActiveOutageRepository.Get()";
                Logger.LogError(message, e);
                throw e;
            }

            if (outageDbEntity == null)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is not found in database.");
                return false;
            }

            if (outageDbEntity.OutageState != OutageState.ISOLATED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {outageDbEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.ISOLATED})");
                return false;
            }

            await Task.Delay(10000);

            IOutageSimulatorContract outageSimulatorClient = OutageSimulatorClient.CreateClient();
            if (await outageSimulatorClient.StopOutageSimulation(outageDbEntity.OutageElementGid))
            {
                outageDbEntity.OutageState = OutageState.REPAIRED;
                outageDbEntity.RepairedTime = DateTime.UtcNow;
                dbContext.OutageRepository.Update(outageDbEntity);

                try
                {
                    dbContext.Complete();
                    await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
                }
                catch (Exception e)
                {
                    string message = "OutageModel::SendRepairCrew => exception in Complete method.";
                    Logger.LogError(message, e);
                }
            }
            else
            {
                string message = "OutageModel::SendRepairCrew => ResolvedOutage() not finished with SUCCESS";
                Logger.LogError(message);
            }

            return true;
        }
    }
}
