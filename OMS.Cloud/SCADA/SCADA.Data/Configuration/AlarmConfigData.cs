﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.Data.Configuration
{
    public class AlarmConfigData
    {

		public float LowPowerLimit { get; set; }
		public float HighPowerLimit { get; set; }
		public float LowVoltageLimit { get; set; }
		public float HighVolageLimit { get; set; }
		public float LowCurrentLimit { get; set; }
		public float HighCurrentLimit { get; set; }

		#region Instance

		private static AlarmConfigData _instance;

		public static AlarmConfigData Instance
		{
			get {	
					if(_instance == null)
					{
						_instance = new AlarmConfigData();
					}
					return _instance;
				}
		}
		#endregion

		private AlarmConfigData()
		{
			ImportAppSettings();
		}

		private void ImportAppSettings()
		{
			if(ConfigurationManager.AppSettings["LowPowerLimit"] is string lowPowerLimitSetting)
			{
				if(float.TryParse(lowPowerLimitSetting, out float lowPowerLimit))
				{
					LowPowerLimit = lowPowerLimit;
				}

			}

			if (ConfigurationManager.AppSettings["HighPowerLimit"] is string highPowerLimitSetting)
			{
				if (float.TryParse(highPowerLimitSetting, out float highPowerLimit))
				{
					HighPowerLimit = highPowerLimit;
				}

			}

			if (ConfigurationManager.AppSettings["LowVoltageLimit"] is string lowVoltageLimitSetting)
			{
				if (float.TryParse(lowVoltageLimitSetting, out float lowVolageLimit))
				{
					LowVoltageLimit = lowVolageLimit;
				}

			}

			if(ConfigurationManager.AppSettings["HighVoltageLimit"] is string highVoltageLimitSetting)
			{
				if(float.TryParse(highVoltageLimitSetting, out float highVoltageLimit))
				{
					HighVolageLimit = highVoltageLimit;
				}
			}

			if (ConfigurationManager.AppSettings["LowCurrentLimit"] is string lowCurrentLimitSetting)
			{
				if (float.TryParse(lowCurrentLimitSetting, out float lowCurrentLimit))
				{
					LowCurrentLimit = lowCurrentLimit;
				}

			}

			if (ConfigurationManager.AppSettings["HighCurrentLimit"] is string highCurrentLimitSetting)
			{
				if (float.TryParse(highCurrentLimitSetting, out float highCurrentLimit))
				{
					HighCurrentLimit = highCurrentLimit;
				}
			}
		}
	}
}
