using System;
using System.ComponentModel;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Core.Assemblies;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Abstract class <c>Straight</c> contains common class members for Straight surfaces.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public abstract class Straight : IStraightSurface
    {
        #region Constructor

        protected Straight(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SurfaceInfo = surface ?? throw new ArgumentNullException(nameof(surface));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Assembly Parent { get; }

        [Browsable(false)]
        public BoxGeometry Geometry { get; }

        [Browsable(false)]
        public Data.StraightSurface SurfaceInfo { get; }

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
