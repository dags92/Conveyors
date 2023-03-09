using System;
using System.Xml.Serialization;
using Experior.Core.Media;
using Experior.Core.Parts;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Data
{
    /// <summary>
    /// <c>Boundaries</c> class used to serialize information related to conveyor's boundaries.
    /// </summary>
    [Serializable, XmlInclude(typeof(Boundaries)), XmlType(TypeName = "Experior.Surface.Foundations.Data.Boundaries")]
    public class Boundaries
    {
        public Friction Friction { get; set; } = new Friction()
        {
            Coefficient = Coefficients.Sticky
        };

        public bool Splittable { get; set; } = true;

        public float SideGuideHeight { get; set; } = 0.1f;

        public float SideGuideWidth { get; set; } = 0.02f;

        public float Ramp { get; set; } = 0f; // Implemented for Straight

        public bool UseRamp { get; set; }    // Implemented for Curve

        public bool UseStartBoundary { get; set; } = false;

        public bool UseEndBoundary { get; set; } = false;

        public bool UseRightBoundary { get; set; } = false;

        public bool UseLeftBoundary { get; set; } = false;

        public System.Windows.Media.Color Color { get; set; }
    }
}
