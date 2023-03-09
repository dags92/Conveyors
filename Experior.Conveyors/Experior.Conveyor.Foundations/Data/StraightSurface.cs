using System;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;

namespace Experior.Conveyor.Foundations.Data
{
    /// <summary>
    /// <c>Straight</c> class used to serialize information related to the surface parts dimensions.
    /// </summary>
    [Serializable, XmlInclude(typeof(StraightSurface)), XmlType(TypeName = "Experior.Surface.Foundations.Data.StraightSurface")]
    public class StraightSurface
    {
        public AuxiliaryData.SurfaceType SurfaceType { get; set; }

        public float BeltHeight { get; set; } = 0.06f;

        public float RollerDiameter { get; set; } = 0.06f;

        public float RollerPitch { get; set; } = 0.07f;

        public float StrapHeight { get; set; } = 0.05f;

        public float StrapPitch { get; set; } = 0.03f;

        public float StrapWidth { get; set; } = 0.04f;

        public AuxiliaryData.NoseOverDirection NoseOverDirection { get; set; }

        public float NoseAngle { get; set; }

        public Color Color { get; set; } = Colors.Black;
    }
}
