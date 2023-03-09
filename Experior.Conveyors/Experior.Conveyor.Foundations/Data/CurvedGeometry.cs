using System;
using System.Xml.Serialization;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Data
{
    /// <summary>
    /// <c>CurvedSurface</c> class used to serialize information related to the surface parts dimensions.
    /// </summary>
    [Serializable, XmlInclude(typeof(CurvedGeometry)), XmlType(TypeName = "Experior.Surface.Foundations.Data.CurvedGeometry")]
    public class CurvedGeometry
    {
        public float Radius { get; set; } = 0.6f;

        public float Width { get; set; } = 0.45f;

        public float Angle { get; set; } = (float)Math.PI / 2;

        public float HeightDifference { get; set; } = 0f;

        public Revolution Revolution { get; set; }
    }
}
