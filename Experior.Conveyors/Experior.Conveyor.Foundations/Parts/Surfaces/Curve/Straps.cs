using System;
using System.Collections.Generic;
using System.Numerics;
using Experior.Conveyor.Foundations.Parts.Belts;
using Experior.Core.Assemblies;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Curve
{
    /// <summary>
    /// Class <c>Straps</c> provides the visualization of a curve strap conveyor surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Straps : Parts.Surfaces.Curve.Curve
    {
        #region Fields

        private readonly List<Parts.Belts.Curve> _straps = new List<Parts.Belts.Curve>();
        private readonly CurveDrivenPulley _drivenPulley;

        #endregion

        #region Constructor

        public Straps(Assembly parent, Data.CurvedSurface info, Data.CurvedGeometry geometry)
        : base(parent, info, geometry)
        {
            _drivenPulley = new CurveDrivenPulley(Parent, GeometryInfo.Radius, GeometryInfo.Width, GeometryInfo.Angle, SurfaceInfo.StrapHeight, GeometryInfo.HeightDifference, GeometryInfo.Revolution);

            Refresh();
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            _drivenPulley.Remove();

            RemoveStraps();
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            RemoveStraps();

            if (!Parent.Visible)
            {
                return;

            }

            var totalWidth = (GeometryInfo.Width - SurfaceInfo.StrapWidth);
            var no = (int)Math.Ceiling(totalWidth / (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth));
            var offset = ((totalWidth) - (no - 1) * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth)) / 2;

            for (var i = 0; i < no; i++)
            {
                var strap = new Parts.Belts.Curve(GeometryInfo.Radius, SurfaceInfo.StrapHeight, SurfaceInfo.StrapWidth, GeometryInfo.Revolution, GeometryInfo.Angle)
                {
                    Color = SurfaceInfo.Color
                };

                strap.Radius = GeometryInfo.Revolution == Revolution.Clockwise
                    ? GeometryInfo.Radius - (GeometryInfo.Width / 2) + i * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth) + SurfaceInfo.StrapWidth / 2 + offset
                    : GeometryInfo.Radius + (GeometryInfo.Width / 2) - i * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth) - SurfaceInfo.StrapWidth / 2 - offset;

                strap.HeightDifference = GeometryInfo.HeightDifference;

                Parent.Add(strap);
                _straps.Add(strap);

                strap.LocalPosition = new Vector3(0, 0, -GeometryInfo.Radius);
            }

            _drivenPulley.Radius = GeometryInfo.Radius;
            _drivenPulley.Width = GeometryInfo.Width;
            _drivenPulley.Angle = GeometryInfo.Angle;
            _drivenPulley.Height = SurfaceInfo.StrapHeight;
            _drivenPulley.HeightDifference = GeometryInfo.HeightDifference;
            _drivenPulley.Revolution = GeometryInfo.Revolution;
        }

        protected override void ExecuteColorUpdate()
        {
            foreach (var strap in _straps)
            {
                strap.Color = SurfaceInfo.Color;
            }
        }

        #endregion

        #region Private Methods

        private void RemoveStraps()
        {
            foreach (var strap in _straps)
            {
                Parent.Remove(strap);
                strap.Dispose();
            }
            _straps.Clear();
        }

        #endregion
    }
}
