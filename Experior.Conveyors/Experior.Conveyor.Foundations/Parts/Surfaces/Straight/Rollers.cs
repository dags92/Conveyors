using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>Rollers</c> provides the visualization of a straight roller conveyor surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Rollers : Parts.Surfaces.Straight.Straight
    {
        #region Fields

        private readonly List<Cylinder> _rollers = new List<Cylinder>();

        #endregion

        #region Constructor

        public Rollers(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface) 
            : base(parent, geometry, surface)
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
            if (!Parent.Visible)
                return;

            RemoveRollers();

            // Rollers for straight section:
            var no = (int)(Geometry.Length / SurfaceInfo.RollerPitch);
            var firstPitch = Geometry.Length - (no - 1) * SurfaceInfo.RollerPitch;

            for (var i = 0; i < no; i++)
            {
                var roller = new Cylinder(SurfaceInfo.Color, Geometry.Width, SurfaceInfo.RollerDiameter / 2, 12) { Rigid = false };
                _rollers.Add(roller);
                Parent.Add(roller, new Vector3(firstPitch / 2 + i * SurfaceInfo.RollerPitch, -roller.Radius, 0));
            }

            if (SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.None)
            {
                return;
            }

            // Rollers for angled section:
            var beltNoseLength = 0f;
            if (!SurfaceInfo.NoseAngle.IsEffectivelyZero())
            {
                beltNoseLength = Geometry.Width / (float)Math.Tan(SurfaceInfo.NoseAngle);
            }

            no = (int)(beltNoseLength / SurfaceInfo.RollerPitch);
            firstPitch = beltNoseLength - (no - 1) * SurfaceInfo.RollerPitch;

            for (var i = 0; i < no; i++)
            {
                var hypotenuse = (float)Math.Sqrt(Math.Pow(beltNoseLength, 2) + Math.Pow(Geometry.Width, 2));
                var angle = (float)Math.Acos(beltNoseLength / hypotenuse);

                var newLength = beltNoseLength - firstPitch / 2 - i * SurfaceInfo.RollerPitch;
                var diagonal = newLength / (float)Math.Cos(angle);
                var newWidth = diagonal * (float)Math.Sin(angle);

                var roller = new Cylinder(Colors.Silver, newWidth, SurfaceInfo.RollerDiameter / 2f, 12) { Rigid = false, };
                _rollers.Add(roller);
                Parent.Add(roller);

                var zCoordinate = SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Right
                    ? Geometry.Width / 2 - roller.Length / 2
                    : -Geometry.Width / 2 + roller.Length / 2;
                roller.LocalPosition = new Vector3(Geometry.Length + firstPitch / 3 + i * SurfaceInfo.RollerPitch, -roller.Radius, zCoordinate);
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
