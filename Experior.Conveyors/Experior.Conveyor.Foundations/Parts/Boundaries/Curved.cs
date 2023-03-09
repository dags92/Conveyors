using System;
using System.ComponentModel;
using System.Numerics;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Boundaries
{
    /// <summary>
    /// Class <c>Curved</c> to create boundaries in a curved conveyor (right, left, front, and back).
    /// </summary>
    public class Curved
    {
        #region Fields

        private readonly Data.Boundaries _info;

        #endregion

        #region Constructor

        public Curved(Curve parent, Data.Boundaries info)
        {
            Parent = parent ?? throw new NullReferenceException();
            _info = info ?? throw new NullReferenceException();

            CreateBoundaries();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Curve Parent { get; }

        #endregion

        #region Protected Properties

        [Browsable(false)]
        protected CurveArea InnerBoundary { get; private set; }

        [Browsable(false)]
        protected CurveArea OuterBoundary { get; private set; }

        [Browsable(false)]
        protected Box StartBoundary { get; private set; }

        [Browsable(false)]
        protected Box EndBoundary { get; private set; }

        #endregion

        #region Public Methods

        public void CreateBoundaries()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                // Lateral boundaries

                if (_info.UseRightBoundary)
                {
                    if (InnerBoundary == null)
                    {
                        InnerBoundary = new CurveArea(_info.Color, _info.SideGuideHeight, _info.SideGuideWidth, Parent.Radius, Parent.Angle, Parent.Revolution)
                        {
                            Rigid = true
                        };

                        Parent.Add(InnerBoundary);
                    }
                }
                else
                {
                    if (InnerBoundary != null)
                    {
                        Parent.Remove(InnerBoundary);
                        InnerBoundary.Dispose();
                        InnerBoundary = null;
                    }
                }

                if (_info.UseLeftBoundary)
                {
                    if (OuterBoundary == null)
                    {
                        OuterBoundary = new CurveArea(_info.Color, _info.SideGuideHeight, _info.SideGuideWidth, Parent.Radius, Parent.Angle, Parent.Revolution)
                        {
                            Rigid = true
                        };

                        Parent.Add(OuterBoundary);
                    }
                }
                else
                {
                    if (OuterBoundary != null)
                    {
                        Parent.Remove(OuterBoundary);
                        OuterBoundary.Dispose();
                        OuterBoundary = null;
                    }
                }

                // Front and back boundaries

                if (_info.UseStartBoundary)
                {
                    if (StartBoundary == null)
                    {
                        StartBoundary = new Box(Parent.Color, _info.SideGuideWidth, _info.SideGuideHeight, Parent.Width + _info.SideGuideWidth * 2)
                        {
                            Rigid = true,
                            Color = Parent.Color,
                            Friction = _info.Friction
                        };

                        Parent.Add(StartBoundary);
                    }
                }
                else
                {
                    if (StartBoundary != null)
                    {
                        Parent.Remove(StartBoundary);
                        StartBoundary.Dispose();
                        StartBoundary = null;
                    }
                }

                if (_info.UseEndBoundary)
                {
                    if (EndBoundary == null)
                    {
                        EndBoundary = new Box(Parent.Color, _info.SideGuideWidth, _info.SideGuideHeight, Parent.Width + _info.SideGuideWidth * 2)
                        {
                            Rigid = true,
                            Color = Parent.Color,
                            Friction = _info.Friction
                        };

                        Parent.Add(EndBoundary);
                    }
                }
                else
                {
                    if (EndBoundary != null)
                    {
                        Parent.Remove(EndBoundary);
                        EndBoundary.Dispose();
                        EndBoundary = null;
                    }
                }

                UpdateBoundaries();
            });
        }

        public void UpdateBoundaries()
        {
            if (InnerBoundary != null)
            {
                InnerBoundary.RampType = CurveRampTypes.Inner;
                UpdateLateralBoundary(InnerBoundary);
            }

            if (OuterBoundary != null)
            {
                OuterBoundary.RampType = CurveRampTypes.Outer;
                UpdateLateralBoundary(OuterBoundary);
            }

            if (StartBoundary != null)
            {
                UpdateBoundaryDimensions(StartBoundary);
                StartBoundary.LocalPosition = new Vector3(0, StartBoundary.Height / 2, 0);
            }

            if (EndBoundary != null)
            {
                UpdateBoundaryDimensions(EndBoundary);
                EndBoundary.LocalYaw = Parent.Revolution == Revolution.Clockwise ? Parent.Angle : -Parent.Angle;
                EndBoundary.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, 0, -Parent.Radius), Parent.Angle, Parent.Radius, EndBoundary.Height / 2 + Parent.HeightDifference, Parent.Revolution);
            }
        }

        public void UpdateColor()
        {
            if (StartBoundary != null)
            {
                StartBoundary.Color = _info.Color;
            }

            if (EndBoundary != null)
            {
                EndBoundary.Color = _info.Color;
            }

            if (InnerBoundary != null)
            {
                InnerBoundary.Color = _info.Color;
            }

            if (OuterBoundary != null)
            {
                OuterBoundary.Color = _info.Color;
            }
        }

        #endregion

        #region Private Methods

        private void UpdateLateralBoundary(CurveArea boundary)
        {
            var radius = boundary.RampType == CurveRampTypes.Inner
                ? Parent.Radius - Parent.Width / 2 - _info.SideGuideWidth / 2
                : Parent.Radius + Parent.Width / 2 + _info.SideGuideWidth / 2;

            boundary.Radius = radius;
            boundary.Revolution = Parent.Revolution;
            boundary.Width = _info.SideGuideWidth;
            boundary.Height = _info.SideGuideHeight;
            boundary.HeightDifference = Parent.HeightDifference;
            boundary.Angle = Parent.Angle.ToAngle();
            boundary.Ramp = _info.UseRamp;

            var position = boundary.RampType == CurveRampTypes.Inner
                ? new Vector3(0, InnerBoundary.Height / 2, -Parent.Radius)
                : new Vector3(0, OuterBoundary.Height / 2, -Parent.Radius);

            boundary.LocalPosition = position;
        }

        private void UpdateBoundaryDimensions(Box boundary)
        {
            boundary.Length = _info.SideGuideWidth;
            boundary.Height = _info.SideGuideHeight;
            boundary.Width = Parent.Width + _info.SideGuideWidth * 2;
        }

        #endregion
    }
}
