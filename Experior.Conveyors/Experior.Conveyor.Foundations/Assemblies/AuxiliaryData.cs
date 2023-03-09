using System.Xml.Serialization;

namespace Experior.Conveyor.Foundations.Assemblies
{
    /// <summary>
    /// Class <c>AuxiliaryData</c> container of general data.
    /// </summary>
    public static class AuxiliaryData
    {
        /// <summary>
        /// Constant <c>BeltHeight</c> used to define the height of Straight and Curve belts.
        /// </summary>
        public const float BeltHeight = 0.025f;

        /// <summary>
        /// Enum <c>SurfaceType</c> indicates the types of surfaces.
        /// </summary>
        [XmlType(TypeName = "Experior.Conveyor.Foundations.Assemblies.SurfaceType")]
        public enum SurfaceType
        {
            Belt,
            Roller,
            Strap
        }

        /// <summary>
        /// Enum <c>NoseOverDirection</c> indicates the nose direction of a straight surface.
        /// </summary>
        [XmlType(TypeName = "Experior.Conveyor.Foundations.Assemblies.NoseOverDirection")]
        public enum NoseOverDirection
        {
            None,
            Right,
            Left
        }

        /// <summary>
        /// Enum <c>SideGuidePositions</c> indicates the position of a side guide.
        /// </summary>
        [XmlType(TypeName = "Experior.Conveyor.Foundations.Assemblies.SideGuidePositions")]
        public enum SideGuidePositions
        {
            Right,
            Left
        }

        /// <summary>
        /// Enum <c>SystemType</c> indicates the application system.
        /// </summary>
        [XmlType(TypeName = "Experior.Conveyor.Foundations.Assemblies.SystemType")]
        public enum SystemType
        {
            CaseConveying,
            PalletConveying
        }
    }
}
