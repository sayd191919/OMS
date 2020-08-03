﻿using CECommon.Interfaces;
using OMS.Common.Cloud;
using System.Runtime.Serialization;

namespace CECommon.Models
{
    [DataContract]
    public class EnergyConsumer : TopologyElement
	{
		public EnergyConsumer(ITopologyElement element) : base (element.Id)
		{
            Id = element.Id;
            Description = element.Description;
            Mrid = element.Mrid;
            Name = element.Name;
            NominalVoltage = element.NominalVoltage;
            FirstEnd = element.FirstEnd;
            SecondEnd = element.SecondEnd;
            DmsType = element.DmsType;
            Measurements = element.Measurements;
            IsRemote = element.IsRemote;
            IsActive = element.IsActive;
        }

        [DataMember]
        public EnergyConsumerType Type { get; set; }
    }
}