//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Outage {
    using System;
    using Outage;
    
    
    /// The parts of the power system that are designed to carry current or that are conductively connected therewith. ConductingEquipment is contained within an EquipmentContainer that may be a Substation, or a VoltageLevel or a Bay within a Substation.
    public class ConductingEquipment : Equipment {
        
        /// Use association to ConductingEquipment only when there is no VoltageLevel container used.
        private BaseVoltage cim_BaseVoltage;
        
        private const bool isBaseVoltageMandatory = false;
        
        private const string _BaseVoltagePrefix = "cim";
        
        public virtual BaseVoltage BaseVoltage {
            get {
                return this.cim_BaseVoltage;
            }
            set {
                this.cim_BaseVoltage = value;
            }
        }
        
        public virtual bool BaseVoltageHasValue {
            get {
                return this.cim_BaseVoltage != null;
            }
        }
        
        public static bool IsBaseVoltageMandatory {
            get {
                return isBaseVoltageMandatory;
            }
        }
        
        public static string BaseVoltagePrefix {
            get {
                return _BaseVoltagePrefix;
            }
        }
    }
}
