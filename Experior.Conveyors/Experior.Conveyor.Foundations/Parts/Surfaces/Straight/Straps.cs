using System;
using System.Collections.Generic;
using System.Numerics;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Parts.Belts;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>Straps</c> provides the visualization of a straight strap conveyor surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Straps : Parts.Surfaces.Straight.Straight
    {
        #region Fields

        private readonly List<Parts.Belts.Straight> _straps = new List<Parts.Belts.Straight>();
        private StraightDrivenPulley _drivenPulley;

        #endregion

        #region Constructor

        public Straps(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface)
            : base(parent, geometry, surface)
        {
            _drivenPulley = new StraightDrivenPulley(Parent, Geometry.Length, SurfaceInfo.StrapHeight, Geometry.Width, SurfaceInfo.NoseOverDirection);

            Refresh();
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            _drivenPulley?.Dispose();
            RemoveStraps();
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            if (!Parent.Visible)
            {
                return;
            }

            RemoveStraps();

            const float widthEnd = 0.001f;

            var totalWidth = Geometry.Width - SurfaceInfo.StrapWidth;
            var no = (int)Math.Ceiling(totalWidth / (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth));
            var offset = (totalWidth - (no - 1) * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth)) / 2;

            for (var i = 0; i < no; i++)
            {
                var strap = new Parts.Belts.Straight(Geometry.Length, SurfaceInfo.StrapHeight, SurfaceInfo.StrapWidth)
                {
                    Color = SurfaceInfo.Color
                };
                Parent.Add(strap);
                _straps.Add(strap);

                strap.LocalPosition = new Vector3(0, 0, -Geometry.Width / 2 + i * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth) + SurfaceInfo.StrapWidth / 2 + offset);
            }

            _drivenPulley.Length = Geometry.Length;
            _drivenPulley.Width = Geometry.Width;
            _drivenPulley.Height = SurfaceInfo.StrapHeight;
            _drivenPulley.NoseOverDirection = SurfaceInfo.NoseOverDirection;

            var beltNoseLength = 0f;
            if (!SurfaceInfo.NoseAngle.IsEffectivelyZero())
            {
                beltNoseLength = Geometry.Width / (float)Math.Tan(SurfaceInfo.NoseAngle);
            }

            if (SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.None || _straps.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _straps.Count; i++)
            {
                var tempWidth = Math.Abs(Geometry.Width - widthEnd);
                var boundaryYaw = (float)Math.Atan(beltNoseLength / tempWidth);
                float b;

                switch (SurfaceInfo.NoseOverDirection)
                {
                    case AuxiliaryData.NoseOverDirection.Right:
                        _straps[i].LocalPosition = new Vector3(0, 0, Geometry.Width / 2 - (i * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth)) - SurfaceInfo.StrapWidth / 2 - offset);

                        b = _straps[i].LocalPosition.Z + Geometry.Width / 2;
                        _straps[i].Length = (float)Math.Tan(boundaryYaw) * b + Geometry.Length;
                        break;

                    case AuxiliaryData.NoseOverDirection.Left:
                        _straps[i].LocalPosition = new Vector3(0, 0, -Geometry.Width / 2 + (i * (SurfaceInfo.StrapPitch + SurfaceInfo.StrapWidth)) + SurfaceInfo.StrapWidth / 2 + offset);

                        b = Geometry.Width / 2 - _straps[i].LocalPosition.Z;
                        _straps[i].Length = (float)Math.Tan(boundaryYaw) * b + Geometry.Length;
                        break;
                }
            }
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
