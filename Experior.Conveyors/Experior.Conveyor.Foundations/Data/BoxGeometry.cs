using System;
using System.Xml.Serialization;

namespace Experior.Conveyor.Foundations.Data
{
    /// <summary>
    /// <c>Straight</c> class used to serialize information related to the surface parts dimensions.
    /// </summary>
    [Serializable, XmlInclude(typeof(BoxGeometry)), XmlType(TypeName = "Experior.Surface.Foundations.Data.BoxGeometry")]
    public class BoxGeometry
    {
        public float Length { get; set; } = 1f;

        public float Width { get; set; } = 0.45f;

        public float Height { get; set; } = 0.025f;
    }
}
