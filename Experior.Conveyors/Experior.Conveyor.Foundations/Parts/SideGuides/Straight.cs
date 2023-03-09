using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Environment = Experior.Core.Environment;

namespace Experior.Conveyor.Foundations.Parts.SideGuides
{
    /// <summary>
    /// Class <c>Straight</c> provides the visualization of Side Guides of a straight conveyor.
    /// </summary>
    public class Straight
    {
        #region Fields

        private readonly Box _rightGuide;
        private readonly Box _leftGuide;

        private const float BoxWidth = 0.02f;
        private Color _color;

        #endregion

        #region Constructor

        public Straight(Assemblies.Straight parent)
        {
            Parent = parent ?? throw new NullReferenceException();
            _color = Parent.Color;

            _rightGuide = new Box(_color, Parent.Length, Parent.Surface?.GetSurfaceHeight() ?? 0.05f, BoxWidth)
            {
                Rigid = false
            };
            _leftGuide = new Box(_color, Parent.Length, Parent.Surface?.GetSurfaceHeight() ?? 0.05f, BoxWidth)
            {
                Rigid = false
            };

            Parent.Add(_rightGuide);
            Parent.Add(_leftGuide);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Assemblies.Straight Parent { get; }

        [Browsable(false)]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;

                if (_rightGuide != null)
                {
                    _rightGuide.Color = value;
                }

                if (_leftGuide != null)
                {
                    _leftGuide.Color = value;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            Environment.InvokeIfRequired(() =>
            {
                if (Parent.Surface != null)
                {
                    var noseLength = !Parent.Surface.SurfaceInfo.NoseAngle.IsEffectivelyZero()
                        ? Parent.Width / (float)Math.Tan(Parent.Surface.SurfaceInfo.NoseAngle)
                        : 0f;

                    _rightGuide.Length = Parent.Surface.SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Left
                        ? Parent.Length + noseLength
                        : Parent.Length;

                    _leftGuide.Length = Parent.Surface.SurfaceInfo.NoseOverDirection == AuxiliaryData.NoseOverDirection.Right
                        ? Parent.Length + noseLength
                        : Parent.Length;
                }
                else
                {
                    _rightGuide.Length = _leftGuide.Length = Parent.Length;
                }

                _rightGuide.Height = _leftGuide.Height = Parent.Surface?.GetSurfaceHeight() ?? 0.05f;

                _rightGuide.LocalPosition = new Vector3(_rightGuide.Length / 2, -_rightGuide.Height / 2, -Parent.Width / 2 - _rightGuide.Width / 2);
                _leftGuide.LocalPosition = new Vector3(_leftGuide.Length / 2, -_leftGuide.Height / 2, Parent.Width / 2 + _leftGuide.Width / 2);
            });
        }

        public void Remove()
        {
            Parent.Remove(_rightGuide);
            Parent.Remove(_leftGuide);

            _rightGuide.Dispose();
            _leftGuide.Dispose();
        }

        #endregion
    }
}
