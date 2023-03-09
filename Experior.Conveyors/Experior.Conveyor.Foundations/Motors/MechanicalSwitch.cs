using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Experior.Core.Communication.PLC;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Motors
{
    [TypeConverter(typeof(ObjectConverter))]
    [Serializable, XmlInclude(typeof(MechanicalSwitch)), XmlType(TypeName = "Experior.Surface.Foundations.Motors.MechanicalSwitch")]
    public class MechanicalSwitch
    {
        public bool Enabled = false;
        public Output State;

        [XmlIgnore]
        public bool Warning => State.Warning;

        public override string ToString()
        {
            return "Mechanical Switch";
        }
    }
}
