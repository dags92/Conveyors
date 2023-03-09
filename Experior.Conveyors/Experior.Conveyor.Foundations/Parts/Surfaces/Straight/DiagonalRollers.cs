using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Experior.Conveyor.Foundations.Data;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>DiagonalRollers</c> provides the visualization of a straight alignment roller conveyor surface.
    /// No interaction with dynamic objects (Loads) is provided..
    /// </summary>
    public class DiagonalRollers : Parts.Surfaces.Straight.Straight
    {
        #region Fields

        private readonly List<Cylinder> _rollers = new List<Cylinder>();
        private float _surfaceDirectionOffsetAngle = 15f;

        #endregion

        #region Constructor

        public DiagonalRollers(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface) : base(parent, geometry, surface)
        {
            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Angle")]
        [PropertyOrder(5)]
        public float SurfaceDirectionOffsetAngle
        {
            get => _surfaceDirectionOffsetAngle;
            set
            {
                if (_surfaceDirectionOffsetAngle.IsEffectivelyEqual(value))
                {
                    return;
                }

                _surfaceDirectionOffsetAngle = value;
                Refresh();
            }
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

            var radSurfacedDirectionOffsetAngle = Trigonometry.Angle2Rad(SurfaceDirectionOffsetAngle);
            var tangent = SurfaceInfo.RollerPitch;
            var directionOffsetLength = 0f;
            var triangleNo = 0f;
            var rollerLength = Geometry.Width / (float)Math.Cos(radSurfacedDirectionOffsetAngle);
            var triangleHeight = Math.Abs((float)Math.Sin(radSurfacedDirectionOffsetAngle) * rollerLength);

            if (Math.Abs(SurfaceDirectionOffsetAngle) > 1)
            {
                tangent = SurfaceInfo.RollerPitch / (float)Math.Sin((float)Math.PI / 2 - radSurfacedDirectionOffsetAngle);
                directionOffsetLength = Math.Abs((float)Math.Tan(radSurfacedDirectionOffsetAngle)) * Geometry.Width / 2;
                triangleNo = triangleHeight / tangent;

                var triangleRest = triangleNo - (int)triangleNo;
                if (triangleRest < 0.5f)
                {
                    triangleNo = triangleNo - 1 - triangleRest;
                }
                else
                {
                    triangleNo -= triangleRest;
                }
            }

            var numberOfRollers = (Geometry.Length - directionOffsetLength * 2) / tangent;

            var rest = numberOfRollers - (int)numberOfRollers;
            if (rest < 0.5f)
            {
                numberOfRollers = numberOfRollers - 1 - rest;
            }
            else
            {
                numberOfRollers -= rest;
            }

            var offset = Geometry.Length - tangent * numberOfRollers;
            var start = new Vector3(Geometry.Length / 2 - offset / 2 + triangleNo * tangent, 0, 0);

            numberOfRollers += 1;
            numberOfRollers += triangleNo * 2;

            for (var i = 0; i < numberOfRollers; i++)
            {
                var roller = new Cylinder(SurfaceInfo.Color, Geometry.Width, SurfaceInfo.RollerDiameter / 2, 12) { Rigid = false };

                _rollers.Add(roller);
                Parent.Add(roller);

                roller.LocalPosition = new Vector3(start.X - i * tangent + Geometry.Length / 2, -(SurfaceInfo.RollerDiameter / 2f), 0);
                roller.Length = rollerLength;

                roller.LocalYaw = radSurfacedDirectionOffsetAngle;
            }

            if (_rollers.Count >= triangleNo)
            {
                for (var i = 0; i < triangleNo; i++)
                {
                    var length = Math.Abs((triangleHeight - tangent * (i + 1)) / (float)Math.Sin(radSurfacedDirectionOffsetAngle));
                    var tangentZ = SurfaceInfo.RollerPitch / (float)Math.Sin(radSurfacedDirectionOffsetAngle);

                    var idx = (int)triangleNo - (i + 1);
                    _rollers[idx].LocalPosition = _rollers[(int)triangleNo].LocalPosition + new Vector3(tangent / 2 * (i + 1), 0, -tangentZ / 2 * (i + 1));
                    _rollers[idx].Length = length;

                    idx = _rollers.Count - (int)triangleNo + i;
                    _rollers[idx].LocalPosition = _rollers[_rollers.Count - (int)triangleNo - 1].LocalPosition - new Vector3(tangent / 2 * (i + 1), 0, -tangentZ / 2 * (i + 1));
                    _rollers[idx].Length = length;
                }
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
