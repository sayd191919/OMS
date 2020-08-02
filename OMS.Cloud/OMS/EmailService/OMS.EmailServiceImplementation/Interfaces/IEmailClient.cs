﻿using OMS.CallTrackingServiceImplementation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Interfaces
{
	public interface IEmailClient
	{
		bool Connect();
		IEnumerable<OutageMailMessage> GetUnreadMessages();
	}
}
