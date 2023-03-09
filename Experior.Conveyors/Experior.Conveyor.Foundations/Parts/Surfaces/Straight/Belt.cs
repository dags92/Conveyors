using System;
using System.Numerics;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Parts.Belts;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>Belt</c> provides the visualization of a straight conveyor belt surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Belt : Parts.Surfaces.Straight.Straight
    {
        #region Fields

        private readonly Parts.Belts.Straight _belt;
        private ConveyorBelt _beltNose;
        private readonly StraightDrivenPulley _drivenPulley;

        #endregion

        #region Constructor

        public Belt(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface)
            : base(parent, geometry, surface)
        {
            _belt = new Parts.Belts.Straight(Geometry.Length, SurfaceInfo.BeltHeight, Geometry.Width)
            {
                Color = SurfaceInfo.Color
            };
            Parent.Add(_belt);

            _drivenPulley = new StraightDrivenPulley(Parent, Geometry.Length, SurfaceInfo.BeltHeight, Geometry.Width, SurfaceInfo.NoseOverDirection);

            Refresh();
        }

        #endregion
        
        #region Public Methods

        public override void Dispose()
        {
            _drivenPulley.Dispose();
            _belt.Dispose();

            if (_beltNose != null)
            {
                Parent.Remove(_beltNose);
                _beltNose.Dispose();
            }
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            if (!Parent.Visible)
            {
                return;
            }

            if (_belt == null)
            {
                return;
            }

            _belt.Length = Geometry.Length;
            _belt.Height = SurfaceInfo.BeltHeight;
            _belt.Width = Geometry.Width - 0.01f;

            _drivenPulley.Length = Geometry.Length;
            _drivenPulley.Width = Geometry.Width;
            _drivenPulley.Height = SurfaceInfo.BeltHeight;
            _drivenPulley.NoseOverDirection = SurfaceInfo.NoseOverDirection;

            if (SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.None)
            {
                if (_beltNose != null)
                {
                    Parent.Remove(_beltNose);
                    _beltNose.Dispose();
                    _beltNose = null;
                }

                return;
            }

            if (SurfaceInfo.NoseAngle.IsEffectivelyZero())
            {
                return;
            }

            var beltNoseLength = Geometry.Width / (float)Math.Tan(SurfaceInfo.NoseAngle);
            if (_beltNose == null)
            {
                _beltNose = new ConveyorBelt(beltNoseLength, SurfaceInfo.BeltHeight, Geometry.Width, true, 0.025f)
                {
                    Color = System.Windows.Media.Colors.Black,
                    Rigid = false,
                    RampSide = RampSides.None,
                    RampEnd = RampEnds.Front,
                };

                Parent.Add(_beltNose);
                _beltNose.LocalYaw = (float)Math.PI;
            }

            _beltNose.Length = beltNoseLength;
            _beltNose.Width = Geometry.Width - 0.01f;
            _beltNose.Height = SurfaceInfo.BeltHeight;
            _beltNose.LocalPosition = new Vector3(Geometry.Length + beltNoseLength / 2 - 0.01f, -SurfaceInfo.BeltHeight / 2, 0);

            switch (SurfaceInfo.NoseOverDirection)
            {
                case AuxiliaryData.NoseOverDirection.Right:
                    _beltNose.EndOffsetLeft = 1;
                    _beltNose.EndOffsetRight = 0;
                    break;

                case AuxiliaryData.NoseOverDirection.Left:
                    _beltNose.EndOffsetRight = 1;
                    _beltNose.EndOffsetLeft = 0;
                    break;
            }
        }

        protected override void ExecuteColorUpdate()
        {
            if (_belt != null)
            {
                _belt.Color = SurfaceInfo.Color;
            }

            if (_beltNose != null)
            {
                _beltNose.Color = SurfaceInfo.Color;
            }
        }

        #endregion
    }
}
