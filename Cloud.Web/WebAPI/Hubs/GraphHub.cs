﻿using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class GraphHub : Hub
    {
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public GraphHub()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        //public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        public async Task NotifyGraphUpdate(string omsGraphJson)
        {
            try
            {
                //Clients.All.SendAsync("updateGraph", new OmsGraphViewModel { Nodes = nodes, Relations = relations });

                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => About to call Clients.All.SendAsync().");
                await Clients.All.SendAsync("updateGraph", omsGraphJson);
                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => oms graph data [json format] sent to front-end: {omsGraphJson}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyGraphUpdate => Exception: {e.Message}", e);
            }
        }

        public void NotifyGraphOutageCall(long gid)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyGraphOutageCall => About to call Clients.All.SendAsync().");
                Clients.All.SendAsync("reportOutageCall", gid);
                Logger.LogDebug($"{baseLogString} NotifyGraphOutageCall => graph outage call [gid] sent to front-end: 0x{gid:X16}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyGraphOutageCall => Exception: {e.Message}", e);
            } 
        }

        public void Join()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Users");
        }

        public override Task OnConnectedAsync()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Users");
            return base.OnConnectedAsync();
        }
    }
}