using System;
using System.Xml.Serialization;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;

namespace Experior.Conveyor.Foundations.Data
{
    /// <summary>
    /// <c>CurvedSurface</c> class used to serialize information related to the surface parts dimensions.
    /// </summary>
    [Serializable, XmlInclude(typeof(CurvedSurface)), XmlType(TypeName = "Experior.Surface.Foundations.Data.CurvedSurface")]
    public class CurvedSurface
    {
        public AuxiliaryData.SurfaceType SurfaceType { get; set; }

        public float BeltHeight { get; set; } = 0.06f;

        public float RollerDiameter { get; set; } = 0.06f;

        public float RollerPitch { get; set; } = 8f;

        public float StrapHeight { get; set; } = 0.05f;

        public float StrapPitch { get; set; } = 0.03f;

        public float StrapWidth { get; set; } = 0.04f;

        public Color Color { get; set; } = Colors.Black;
    }
}
