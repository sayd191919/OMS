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
    
    
    /// This is a root class to provide common naming attributes for all classes needing naming attributes
    public class IdentifiedObject : IDClass {
        
        /// The description is a free human readable text describing or naming the object. It may be non unique and may not correlate to a naming hierarchy.
        private string cim_description;
        
        private const bool isDescriptionMandatory = true;
        
        private const string _descriptionPrefix = "cim";
        
        /// A Model Authority issues mRIDs. Given that each Model Authority has a unique id and this id is part of the mRID, then the mRID is globally unique.
        private string cim_mRID;
        
        private const bool isMRIDMandatory = true;
        
        private const string _mRIDPrefix = "cim";
        
        private string cim_name;
        
        private const bool isNameMandatory = true;
        
        private const string _namePrefix = "ftn";
        
        public virtual string Description {
            get {
                return this.cim_description;
            }
            set {
                this.cim_description = value;
            }
        }
        
        public virtual bool DescriptionHasValue {
            get {
                return this.cim_description != null;
            }
        }
        
        public static bool IsDescriptionMandatory {
            get {
                return isDescriptionMandatory;
            }
        }
        
        public static string DescriptionPrefix {
            get {
                return _descriptionPrefix;
            }
        }
        
        public virtual string MRID {
            get {
                return this.cim_mRID;
            }
            set {
                this.cim_mRID = value;
            }
        }
        
        public virtual bool MRIDHasValue {
            get {
                return this.cim_mRID != null;
            }
        }
        
        public static bool IsMRIDMandatory {
            get {
                return isMRIDMandatory;
            }
        }
        
        public static string MRIDPrefix {
            get {
                return _mRIDPrefix;
            }
        }
        
        public virtual string Name {
            get {
                return this.cim_name;
            }
            set {
                this.cim_name = value;
            }
        }
        
        public virtual bool NameHasValue {
            get {
                return this.cim_name != null;
            }
        }
        
        public static bool IsNameMandatory {
            get {
                return isNameMandatory;
            }
        }
        
        public static string NamePrefix {
            get {
                return _namePrefix;
            }
        }
    }
}
