﻿using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommanding
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class SCADASubscriber : ISubscriberCallback
	{
		private ILogger Logger = LoggerWrapper.Instance;
		public string GetSubscriberName() => "Calculation Engine";

		public void Notify(IPublishableMessage message)
		{
			Logger.LogDebug($"Message recived from PubSub with type {message.GetType().ToString()}.");

			if (message is SingleAnalogValueSCADAMessage singleAnalog)
			{
				SCADACommandingCache.Instance.UpdateAnalogMeasurement(singleAnalog.Gid, singleAnalog.Value);
			}
			else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
			{
				SCADACommandingCache.Instance.UpdateAnalogMeasurement(multipleAnalog.Data);
			}
			else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
			{
				SCADACommandingCache.Instance.UpdateDiscreteMeasurement(singleDiscrete.Gid, singleDiscrete.Value);
			}
			else if (message is MultipleDiscreteValueSCADAMessage multipleDiscrete)
			{
				SCADACommandingCache.Instance.UpdateDiscreteMeasurement(multipleDiscrete.Data);
			}
			else
			{
				Logger.LogError($"Message has unsupported type [{message.GetType().ToString()}].");
			}
		}
	}
}