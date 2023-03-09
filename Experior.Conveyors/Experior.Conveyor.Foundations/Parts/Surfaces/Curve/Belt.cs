using System.Numerics;
using Experior.Conveyor.Foundations.Parts.Belts;
using Experior.Core.Assemblies;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Curve
{
    /// <summary>
    /// Class <c>Belt</c> provides the visualization of a curve conveyor belt surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Belt : Parts.Surfaces.Curve.Curve
    {
        #region Fields

        private readonly Parts.Belts.Curve _belt;
        private readonly CurveDrivenPulley _drivenPulley;

        #endregion

        #region Constructor

        public Belt(Assembly parent, Data.CurvedSurface info, Data.CurvedGeometry geometry)
            : base(parent, info, geometry)
        {
            _belt = new Parts.Belts.Curve(GeometryInfo.Radius, SurfaceInfo.BeltHeight, GeometryInfo.Width, GeometryInfo.Revolution, GeometryInfo.Angle)
            {
                Color = SurfaceInfo.Color
            };
            parent.Add(_belt);

            _drivenPulley = new CurveDrivenPulley(Parent, GeometryInfo.Radius, GeometryInfo.Width, GeometryInfo.Angle, SurfaceInfo.BeltHeight, GeometryInfo.HeightDifference, GeometryInfo.Revolution);

            Refresh();
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            _drivenPulley.Remove();

            Parent.Remove(_belt);
            _belt.Dispose();
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            if (_belt == null)
            {
                return;
            }

            _belt.Visible = Parent.Visible;

            if (!Parent.Visible)
            {
                return;
            }

            _belt.Angle = GeometryInfo.Angle;
            _belt.Radius = GeometryInfo.Radius;
            _belt.Height = SurfaceInfo.BeltHeight;
            _belt.Width = GeometryInfo.Width - 0.01f;
            _belt.HeightDifference = GeometryInfo.HeightDifference;
            _belt.Revolution = GeometryInfo.Revolution;
            _belt.LocalPosition = new Vector3(0, 0, -GeometryInfo.Radius);

            _drivenPulley.Radius = GeometryInfo.Radius;
            _drivenPulley.Width = GeometryInfo.Width;
            _drivenPulley.Angle = GeometryInfo.Angle;
            _drivenPulley.Height = SurfaceInfo.BeltHeight;
            _drivenPulley.HeightDifference = GeometryInfo.HeightDifference;
            _drivenPulley.Revolution = GeometryInfo.Revolution;
        }

        protected override void ExecuteColorUpdate()
        {
            if (_belt != null)
            {
                _belt.Color = SurfaceInfo.Color;
            }
        }

        #endregion
    }
}
