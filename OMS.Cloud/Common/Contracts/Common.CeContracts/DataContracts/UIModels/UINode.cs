﻿using Common.CE.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
	//[Serializable]
	[DataContract]
	public class UINode : IUINode
	{
		[DataMember]
		public long Id { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string Mrid { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public bool IsActive { get; set; }
		[DataMember]
		public List<IUIMeasurement> Measurements { get; set; }
		[DataMember]
		public float NominalVoltage { get; set; }
		[DataMember]
		public string DMSType { get; set; }
		[DataMember]
		public bool IsRemote { get; set; }
		[DataMember]
		public bool NoReclosing { get; set; }
		public UINode() { }
	}
}