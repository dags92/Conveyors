using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Data;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Surfaces.Straight
{
    /// <summary>
    /// Class <c>Sorter</c> provides the visualization of a roller sorter surface.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Sorter : Parts.Surfaces.Straight.Straight
    {
        #region Fields

        private readonly List<Cylinder> _rollers = new List<Cylinder>();
        private int _numberOfRollers = 8;

        #endregion

        #region Constructor

        public Sorter(Assembly parent, BoxGeometry geometry, Data.StraightSurface surface)
            : base(parent, geometry, surface)
        {
            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Rollers Per Line")]
        [PropertyOrder(5)]
        public int Rollers
        {
            get => _numberOfRollers;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Number of Rollers must be greater than 0", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_numberOfRollers.Equals(value))
                {
                    return;
                }

                _numberOfRollers = value;
            }
        }

        #endregion

        #region Public Methods

        public void TurnRollers(float angle)
        {
            if (_rollers == null || _rollers.Count == 0)
            {
                return;
            }

            foreach (var roller in _rollers)
            {
                roller.LocalYaw = Trigonometry.Angle2Rad(angle);
            }
        }

        public override void Dispose()
        {
            RemoveRollers();
        }

        #endregion

        #region Protected Methods

        protected override void ExecuteRefresh()
        {
            RemoveRollers();

            var rollerSpace = Geometry.Width / Rollers;
            var rollerLength = rollerSpace / 2;

            var no = (int)(Geometry.Length / SurfaceInfo.RollerPitch);
            var firstPitch = Geometry.Length - (no - 1) * SurfaceInfo.RollerPitch;

            for (var i = 0; i < no; i++)
            {
                for (var j = 0; j < Rollers; j++)
                {
                    var roller = new Cylinder(SurfaceInfo.Color, 0.025f, SurfaceInfo.RollerDiameter / 2, 12);
                    _rollers.Add(roller);
                    Parent.Add(roller);

                    roller.LocalPosition = new Vector3(Geometry.Length / 2 + firstPitch / 2 - Geometry.Length / 2 + i * SurfaceInfo.RollerPitch, -roller.Radius, -Geometry.Width / 2 + (j + 1) * rollerSpace - rollerLength);
                }
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
