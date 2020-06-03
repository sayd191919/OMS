﻿using Outage.Common;
using System;
using System.Collections.Generic;
using OMS.Common.SCADA;
using OMS.Common.NmsContracts.GDA;
using System.Runtime.Serialization;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;

namespace SCADA.ModelProviderImplementation.Data
{
    internal class ScadaModelPointItemHelper
    {
        //private readonly EnumDescs enumDescs;

        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        //public ScadaModelPointItemHelper()
        //{
        //    enumDescs = new EnumDescs();
        //}

        public void InitializeScadaModelPointItem(ScadaModelPointItem pointItem, List<Property> props, ModelCode type, EnumDescs enumDescs)
        {
            pointItem.Alarm = AlarmType.NO_ALARM;

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        pointItem.Gid = item.AsLong();
                        break;

                    case ModelCode.IDOBJ_NAME:
                        pointItem.Name = item.AsString();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        if (ushort.TryParse(item.AsString(), out ushort address))
                        {
                            pointItem.Address = address;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => Address is either not defined or is invalid.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        if (type == ModelCode.ANALOG)
                        {
                            pointItem.RegisterType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else if (type == ModelCode.DISCRETE)
                        {
                            pointItem.RegisterType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => ModelCode type is neither ANALOG nor DISCRETE.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void InitializeDiscretePointItem(DiscretePointItem pointItem, List<Property> props, ModelCode type, EnumDescs enumDescs)
        {
            InitializeScadaModelPointItem(pointItem as ScadaModelPointItem, props, type, enumDescs);

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.DISCRETE_CURRENTOPEN:
                        pointItem.CurrentValue = (ushort)((item.AsBool() == true) ? 1 : 0);
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        pointItem.MaxValue = (ushort)item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        pointItem.MinValue = (ushort)item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        pointItem.NormalValue = (ushort)item.AsInt();
                        break;
                    case ModelCode.DISCRETE_MEASUREMENTTYPE:
                        pointItem.DiscreteType = (DiscreteMeasurementType)(enumDescs.GetEnumValueFromString(ModelCode.ANALOG_SIGNALTYPE, item.AsEnum().ToString()));
                        break;

                    default:
                        break;
                }
            }

            pointItem.Initialized = true;
            pointItem.SetAlarms();
        }

        public void InitializeAnalogPointItem(AnalogPointItem pointItem, List<Property> props, ModelCode type, EnumDescs enumDescs)
        {
            InitializeScadaModelPointItem(pointItem as ScadaModelPointItem, props, type, enumDescs);

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.ANALOG_CURRENTVALUE:
                        pointItem.CurrentEguValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        pointItem.EGU_Max = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        pointItem.EGU_Min = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        pointItem.NormalValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_SCALINGFACTOR:
                        pointItem.ScalingFactor = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_DEVIATION:
                        pointItem.Deviation = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_SIGNALTYPE:
                        pointItem.AnalogType = (AnalogMeasurementType)(enumDescs.GetEnumValueFromString(ModelCode.ANALOG_SIGNALTYPE, item.AsEnum().ToString()));
                        break;

                    default:
                        break;
                }
            }

            pointItem.Initialized = true;
            pointItem.SetAlarms();
        }
    }
}