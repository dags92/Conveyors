using System;
using System.ComponentModel;
using System.Numerics;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;

namespace Experior.Conveyor.Foundations.Parts.Boundaries
{
    /// <summary>
    /// Class <c>StraightBoundaries</c> implements <c>SplittableStraight</c> class to create boundaries in a straight conveyor (right, left, front, and back).
    /// </summary>
    public class StraightBoundaries
    {
        #region Fields

        private readonly Data.Boundaries _info;
        private readonly Intersectable.Pair _pairStraight = new Intersectable.Pair();

        private SplittableStraight _rightBoundary, _leftBoundary;

        #endregion

        #region Constructor

        public StraightBoundaries(Straight parent, Data.Boundaries info)
        {
            Parent = parent ?? throw new NullReferenceException();
            _info = info ?? throw  new NullReferenceException();

            CreateBoundaries();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Straight Parent { get; }

        #endregion

        #region Protected Properties

        [Browsable(false)]
        protected SplittableStraight RightBoundary
        {
            get => _rightBoundary;
            private set
            {
                RemoveLateralBoundary(_rightBoundary);
                _rightBoundary = value;
            }
        }

        [Browsable(false)]
        protected SplittableStraight LeftBoundary
        {
            get => _leftBoundary;
            private set
            {
                RemoveLateralBoundary(_leftBoundary);
                _leftBoundary = value;
            }
        }

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
                    if (RightBoundary == null)
                    {
                        RightBoundary = new SplittableStraight(Parent.Length, _info, AuxiliaryData.SideGuidePositions.Right, Parent.Color);

                        Parent.Add(RightBoundary);
                        _pairStraight.I2 = RightBoundary;
                    }
                }
                else
                {
                    if (RightBoundary != null)
                    {
                        Parent.Remove(RightBoundary);
                        RightBoundary.Dispose();
                        _pairStraight.I2.Dispose();

                        RightBoundary = null;
                    }
                }

                if (_info.UseLeftBoundary)
                {
                    if (LeftBoundary == null)
                    {
                        LeftBoundary = new SplittableStraight(Parent.Length, _info, AuxiliaryData.SideGuidePositions.Left, Parent.Color);

                        Parent.Add(LeftBoundary);
                        _pairStraight.I1 = LeftBoundary;
                    }
                }
                else
                {
                    if (LeftBoundary != null)
                    {
                        Parent.Remove(LeftBoundary);
                        LeftBoundary.Dispose();
                        _pairStraight.I1.Dispose();

                        LeftBoundary = null;
                    }
                }

                if (!Intersectable.Pairs.Contains(_pairStraight))
                {
                    Intersectable.Pairs.Add(_pairStraight);
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
            var rightSideLength = Parent.Length;
            var leftSideLength = Parent.Length;
            var noseLength = 0f;

            if (Parent.Surface != null)
            {
                noseLength = !Parent.Surface.SurfaceInfo.NoseAngle.IsEffectivelyZero()
                    ? Parent.Width / (float)Math.Tan(Parent.Surface.SurfaceInfo.NoseAngle)
                    : 0f;

                rightSideLength += Parent.Surface.SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Left
                    ? noseLength
                    : 0f;

                leftSideLength += Parent.Surface.SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Right
                    ? noseLength
                    : 0f;
            }

            if (RightBoundary != null)
            {
                RightBoundary.Length = rightSideLength;
                RightBoundary.Width = _info.SideGuideWidth;
                RightBoundary.Height = _info.UseRightBoundary ? _info.SideGuideHeight : 0.03f;
                RightBoundary.Ramp = _info.Ramp;
                RightBoundary.LocalPosition = new Vector3(RightBoundary.Length / 2, 0f, -Parent.Width / 2 - RightBoundary.Width / 2);
            }

            if (LeftBoundary != null)
            {
                LeftBoundary.Length = leftSideLength;
                LeftBoundary.Width = _info.SideGuideWidth;
                LeftBoundary.Height = _info.UseLeftBoundary ? _info.SideGuideHeight : 0.03f;
                LeftBoundary.Ramp = _info.Ramp;
                LeftBoundary.LocalPosition = new Vector3(LeftBoundary.Length / 2, 0f, Parent.Width / 2 + LeftBoundary.Width / 2);
            }

            if (StartBoundary != null)
            {
                UpdateBoundaryDimensions(StartBoundary);
                StartBoundary.LocalPosition = new Vector3(0, StartBoundary.Height / 2, 0);
            }

            if (EndBoundary != null)
            {
                UpdateBoundaryDimensions(EndBoundary);

                if (Parent.Surface != null && Parent.Surface.SurfaceInfo.NoseOverDirection != AuxiliaryData.NoseOverDirection.None)
                {
                    var tempAngle = (float)Math.Atan(noseLength / Parent.Width);
                    EndBoundary.Width = noseLength / (float)Math.Cos(Parent.Surface.SurfaceInfo.NoseAngle);
                    EndBoundary.LocalYaw = Parent.Surface.SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Left ? -tempAngle : tempAngle;
                    EndBoundary.LocalPosition = new Vector3(Parent.Length + noseLength / 2, EndBoundary.Height / 2, 0);
                }
                else
                {
                    EndBoundary.LocalPosition = new Vector3(Parent.Length, EndBoundary.Height / 2, 0);
                }
            }
        }

        public void UpdateColor()
        {
            if (StartBoundary != null)
            {
                StartBoundary.Color = Parent.Color;
            }

            if (EndBoundary != null)
            {
                EndBoundary.Color = Parent.Color;
            }

            if (RightBoundary != null)
            {
                RightBoundary.Color = Parent.Color;
            }

            if (LeftBoundary != null)
            {
                LeftBoundary.Color = Parent.Color;
            }
        }

        public void Splitting()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (Experior.Core.Environment.Scene.Loading)
                {
                    return;
                }

                if (_pairStraight != null)
                {
                    Experior.Core.Assemblies.Intersectable.Splitting(_pairStraight); // continue moving splittable to straight
                }
            });
        }

        public void ClearSplitting()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (Experior.Core.Environment.Scene.Loading)
                {
                    return;
                }

                LeftBoundary?.ClearIntersections();

                RightBoundary?.ClearIntersections();

                if (_pairStraight != null)
                {
                    Experior.Core.Assemblies.Intersectable.ClearIntersections(_pairStraight);
                }
            });
        }

        public void DisposeAssembly()
        {
            if (RightBoundary != null)
            {
                Parent.Remove(RightBoundary);
                RightBoundary.Dispose();
            }

            if (LeftBoundary != null)
            {
                Parent.Remove(LeftBoundary);
                LeftBoundary.Dispose();
            }

            if (StartBoundary != null)
            {
                Parent.Remove(StartBoundary);
                StartBoundary.Dispose();
            }

            if (EndBoundary != null)
            {
                Parent.Remove(EndBoundary);
                EndBoundary.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private void UpdateBoundaryDimensions(Box boundary)
        {
            boundary.Length = _info.SideGuideWidth;
            boundary.Height = _info.SideGuideHeight;
            boundary.Width = Parent.Width + _info.SideGuideWidth * 2;
        }

        private void RemoveLateralBoundary(Intersectable guide)
        {
            if (guide == null)
                return;

            Parent.Remove(guide);
            guide.Dispose();
        }

        #endregion
    }
}
