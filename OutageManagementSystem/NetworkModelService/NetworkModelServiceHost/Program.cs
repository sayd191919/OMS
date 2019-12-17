﻿using Outage.Common;
using Outage.DataImporter.CIMAdapter;
using System;

namespace Outage.NetworkModelServiceHost
{
	public class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

            try
            {
                CIMAdapterClass cim = new CIMAdapterClass();
                
                string message = "Starting Network Model Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (NetworkModelService.NetworkModelService nms = new NetworkModelService.NetworkModelService())
                {
                    nms.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("NetworkModelService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "NetworkModelService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                logger.LogError($"NetworkModelService failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
