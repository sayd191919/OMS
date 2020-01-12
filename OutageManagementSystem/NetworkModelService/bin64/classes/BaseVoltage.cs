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
    
    
    /// Defines a nominal base voltage which is referenced in the system.
    public class BaseVoltage : IdentifiedObject {
        
        /// The PowerSystemResource's base voltage.
        private System.Single? cim_nominalVoltage;
        
        private const bool isNominalVoltageMandatory = true;
        
        private const string _nominalVoltagePrefix = "cim";
        
        public virtual float NominalVoltage {
            get {
                return this.cim_nominalVoltage.GetValueOrDefault();
            }
            set {
                this.cim_nominalVoltage = value;
            }
        }
        
        public virtual bool NominalVoltageHasValue {
            get {
                return this.cim_nominalVoltage != null;
            }
        }
        
        public static bool IsNominalVoltageMandatory {
            get {
                return isNominalVoltageMandatory;
            }
        }
        
        public static string NominalVoltagePrefix {
            get {
                return _nominalVoltagePrefix;
            }
        }
    }
}