using System;
using System.ComponentModel;
using System.Numerics;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Core.Assemblies;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>Chain</c> provides the visualization of a straight chain conveyor surface.
    /// No interaction with dynamic objects (Loads) is provided..
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public class Chain : IStraightSurface
    {
        #region Fields

        private readonly Parts.Chains.Straight _rightBelt;
        private readonly Parts.Chains.Straight _leftBelt;

        private const float CaseHeight = 0.1f;
        private const float CaseWidth = 0.035f;

        #endregion

        #region Constructor

        public Chain(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SurfaceInfo = surface ?? throw new ArgumentNullException(nameof(surface));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));

            _rightBelt = new Chains.Straight(Geometry.Length, CaseHeight, CaseWidth);
            _leftBelt = new Chains.Straight(Geometry.Length, CaseHeight, CaseWidth);
            
            Parent.Add(_rightBelt);
            Parent.Add(_leftBelt);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public virtual Assembly Parent { get; }

        [Browsable(false)]
        public BoxGeometry Geometry { get; }

        [Browsable(false)]
        public Data.StraightSurface SurfaceInfo { get; }

        #endregion

        #region Public Methods

        public float GetSurfaceHeight() => CaseHeight;

        public void Refresh()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                _rightBelt.Length = _leftBelt.Length = Geometry.Length;
                _rightBelt.LocalPosition = new Vector3(0, 0f, -Geometry.Width / 2 + _rightBelt.Width / 2);
                _leftBelt.LocalPosition = new Vector3(0, 0f, Geometry.Width / 2 - _leftBelt.Width / 2);
            });
        }

        public void UpdateColor()
        {
            if (_rightBelt != null)
            {
                _rightBelt.Color = SurfaceInfo.Color;
            }

            if (_leftBelt != null)
            {
                _leftBelt.Color = SurfaceInfo.Color;
            }
        }

        public void Dispose()
        {
            Parent.Remove(_rightBelt);
            Parent.Remove(_leftBelt);

            _rightBelt.Dispose();
            _leftBelt.Dispose();
        }

        #endregion
    }
}
