using System;
using System.Xml.Serialization;


namespace Experior.Conveyor.Foundations.Motors
{
    [Serializable, XmlInclude(typeof(AuxiliaryData)), XmlType(TypeName = "Experior.Surface.Foundations.Motors.AuxiliaryData")]
    public static class AuxiliaryData
    {
        [XmlType(TypeName = "Experior.Surface.Foundations.Motors.AuxiliaryData.Commands")]
        public enum Commands
        {
            Forward = 1,
            Backward = -1,
            Stop = 0
        }

        [XmlType(TypeName = "Experior.Surface.Foundations.Motors.AuxiliaryData.TranslationPositions")]
        public enum TranslationPositions
        {
            Down,
            Middle,
            Up
        }

        [XmlType(TypeName = "Experior.Surface.Foundations.Motors.AuxiliaryData.TranslationAutomaticLimits")]
        public enum TranslationAutomaticLimits
        {
            Stop,
            Eccentric
        }
    }
}
