using System;
using System.ComponentModel;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Core.Assemblies;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Curve
{
    /// <summary>
    /// Abstract class <c>Curve</c> contains common class members for Curve surfaces.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public abstract class Curve : ICurveSurface
    {
        #region Constructor

        protected Curve(Assembly parent, Data.CurvedSurface info, CurvedGeometry geometry)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SurfaceInfo = info ?? throw new ArgumentNullException(nameof(info));
            GeometryInfo = geometry ?? throw new ArgumentNullException(nameof(info));
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Assembly Parent { get; }

        [Browsable(false)]
        public Data.CurvedSurface SurfaceInfo { get; }

        [Browsable(false)]
        public Data.CurvedGeometry GeometryInfo { get; }

        #endregion

        #region Public Methods

        public float GetSurfaceHeight()
        {
            switch (SurfaceInfo.SurfaceType)
            {
                case AuxiliaryData.SurfaceType.Belt:
                    return SurfaceInfo.BeltHeight;

                case AuxiliaryData.SurfaceType.Strap:
                    return SurfaceInfo.StrapHeight;

                default:
                    return SurfaceInfo.RollerDiameter;
            }
        }

        public void Refresh()
        {
            Experior.Core.Environment.InvokeIfRequired(ExecuteRefresh);
        }

        public void UpdateColor()
        {
            Experior.Core.Environment.InvokeIfRequired(ExecuteColorUpdate);
        }

        public abstract void Dispose();

        #endregion

        #region Protected Methods

        protected abstract void ExecuteRefresh();

        protected abstract void ExecuteColorUpdate();

        #endregion
    }
}
