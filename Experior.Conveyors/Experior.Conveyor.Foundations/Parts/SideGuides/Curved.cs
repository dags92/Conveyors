using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.SideGuides
{
    /// <summary>
    /// Class <c>Curved</c> provides the visualization of Side Guides of a curved conveyor.
    /// </summary>
    public class Curved
    {
        #region Fields

        private readonly CurveArea _innerBoundary;
        private readonly CurveArea _outerBoundary;

        private const float CurveWidth = 0.02f;
        private Color _color;

        #endregion

        #region Constructor

        public Curved(Curve parent)
        {
            Parent = parent ?? throw new NullReferenceException();
            _color = Parent.Color;

            _innerBoundary = new CurveArea(_color, Parent.Surface?.GetSurfaceHeight() ?? 0.05f, CurveWidth,
                Parent.Radius, Parent.Angle, Parent.Revolution);

            _outerBoundary = new CurveArea(_color, Parent.Surface?.GetSurfaceHeight() ?? 0.05f, CurveWidth,
                Parent.Radius, Parent.Angle, Parent.Revolution);

            Parent.Add(_innerBoundary);
            Parent.Add(_outerBoundary);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Assemblies.Curve Parent { get; }

        [Browsable(false)]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;

                if (_innerBoundary != null)
                {
                    _innerBoundary.Color = value;
                }

                if (_outerBoundary != null)
                {
                    _outerBoundary.Color = value;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                _innerBoundary.RampType = CurveRampTypes.Inner;
                UpdateLateralBoundary(_innerBoundary);

                _outerBoundary.RampType = CurveRampTypes.Outer;
                UpdateLateralBoundary(_outerBoundary);
            });
        }

        public void Remove()
        {
            Parent.Remove(_innerBoundary);
            Parent.Remove(_outerBoundary);

            _innerBoundary.Dispose();
            _outerBoundary.Dispose();
        }

        #endregion

        #region Private Methods

        private void UpdateLateralBoundary(CurveArea boundary)
        {
            var radius = boundary.RampType == CurveRampTypes.Inner
                ? Parent.Radius - Parent.Width / 2 - _innerBoundary.Width / 2
                : Parent.Radius + Parent.Width / 2 + _outerBoundary.Width / 2;

            boundary.Radius = radius;
            boundary.Revolution = Parent.Revolution;
            boundary.Width = CurveWidth;
            boundary.Height = Parent.Surface?.GetSurfaceHeight() ?? 0.05f;
            boundary.HeightDifference = Parent.HeightDifference;
            boundary.Angle = Parent.Angle.ToAngle();
            boundary.Ramp = false;

            var position = boundary.RampType == CurveRampTypes.Inner
                ? new Vector3(0, -_innerBoundary.Height / 2, -Parent.Radius)
                : new Vector3(0, -_outerBoundary.Height / 2, -Parent.Radius);

            boundary.LocalPosition = position;
        }

        #endregion
    }
}
