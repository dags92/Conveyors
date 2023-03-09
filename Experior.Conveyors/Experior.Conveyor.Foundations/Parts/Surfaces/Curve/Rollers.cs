using System.Collections.Generic;
using System.Numerics;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Curve
{
    /// <summary>
    /// Class <c>Rollers</c> provides the visualization of a curve roller conveyor surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Rollers : Parts.Surfaces.Curve.Curve
    {
        #region Fields

        private readonly List<Core.Parts.Cylinder> _rollers = new List<Cylinder>();

        #endregion

        #region Constructor

        public Rollers(Assembly parent, Data.CurvedSurface info, Data.CurvedGeometry geometry)
        : base(parent, info, geometry)
        {
            Refresh();
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            RemoveRollers();
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            RemoveRollers();

            if (!Parent.Visible)
            {
                return;
            }

            var no = (int)Arithmetic.ComputeIntervalNumbers(GeometryInfo.Angle.ToAngle(), GeometryInfo.Radius, SurfaceInfo.RollerPitch);
            var interval = GeometryInfo.Angle.ToAngle() / (no);

            for (var i = 0; i < no; i++)
            {
                var roller = new Cylinder(SurfaceInfo.Color, GeometryInfo.Width, SurfaceInfo.RollerDiameter / 2, 12) { Rigid = false };

                Parent.Add(roller);
                _rollers.Add(roller);
            }

            var nextAngleRoller = 0f;
            var heightDiff = GeometryInfo.HeightDifference / _rollers.Count;

            for (var i = 0; i < _rollers.Count; i++)
            {
                nextAngleRoller += i == 0 ? interval / 2f : interval;

                if (_rollers[i] == null)
                    continue;

                var part = _rollers[i];
                part.LocalPosition = new Vector3(0, -SurfaceInfo.RollerDiameter / 2, -GeometryInfo.Radius);

                if (heightDiff != 0)
                {
                    if (i == 0)
                        part.LocalPosition += Trigonometry.RotationPoint(Vector3.Zero, Trigonometry.Angle2Rad(nextAngleRoller), GeometryInfo.Radius, heightDiff * 0.5f, GeometryInfo.Revolution);
                    else
                        part.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, 0, -GeometryInfo.Radius), Trigonometry.Angle2Rad(nextAngleRoller), GeometryInfo.Radius, _rollers[i - 1].LocalPosition.Y + heightDiff, GeometryInfo.Revolution);
                }
                else
                    part.LocalPosition += Trigonometry.RotationPoint(Vector3.Zero, Trigonometry.Angle2Rad(nextAngleRoller), GeometryInfo.Radius, 0, GeometryInfo.Revolution);

                part.LocalYaw = GeometryInfo.Revolution == Revolution.Counterclockwise ? Trigonometry.Angle2Rad(-nextAngleRoller) : Trigonometry.Angle2Rad(nextAngleRoller);
            }
        }

        protected override void ExecuteColorUpdate()
        {
            foreach (var roller in _rollers)
            {
                roller.Color = SurfaceInfo.Color;
            }
        }

        #endregion

        #region Private Methods

        private void RemoveRollers()
        {
            foreach (var roller in _rollers)
            {
                Parent.Remove(roller);
                roller.Dispose();
            }
            _rollers.Clear();
        }

        #endregion
    }
}
