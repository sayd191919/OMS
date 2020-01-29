﻿using CECommon;
using CECommon.TopologyConfiguration;
using NetworkModelServiceFunctions;
using Outage.Common;
using System.Collections.Generic;

namespace TopologyBuilder
{
	public class TopologyHelper
	{
		private ILogger logger = LoggerWrapper.Instance;
		private readonly Dictionary<TopologyStatus, List<DMSType>> elementsStatus;
		private readonly Dictionary<TopologyType, List<DMSType>> topologyTypes;

		#region Singleton
		private static object syncObj = new object();
		private static TopologyHelper instance;
		public static TopologyHelper Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new TopologyHelper();
					}
				}
				return instance;
			}

		}
		#endregion
		private TopologyHelper()
		{
			ConfigurationParse cp = new ConfigurationParse();
			elementsStatus = cp.GetAllElementStatus();
			topologyTypes = cp.GetAllTopologyTypes();
		}
		public TopologyStatus GetElementTopologyStatus(long gid)
		{
			logger.LogDebug($"Getting element topology status for GID {gid}.");
			TopologyStatus retVal;
			DMSType type = NMSManager.Instance.GetDMSTypeOfTopologyElement(gid);
			foreach (var item in elementsStatus)
			{
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return TopologyStatus.Regular;
		}
		public TopologyType GetElementTopologyType(long gid)
		{
			logger.LogDebug($"Getting element topology type for GID {gid}.");
			TopologyType retVal;
			DMSType type = NMSManager.Instance.GetDMSTypeOfTopologyElement(gid);
			foreach (var item in topologyTypes)
			{
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return TopologyType.None;
		}
	}
}