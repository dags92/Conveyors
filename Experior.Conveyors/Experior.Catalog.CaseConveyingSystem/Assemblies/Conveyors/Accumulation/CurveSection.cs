using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Rendering.Interfaces;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation
{
    /// <summary>
    /// <c>CurveSection</c> class provides a functional curve conveyor which has a <c>Beam</c> integrated.
    /// </summary>
    public class CurveSection : Curve, ISection
    {
        #region Fields

        private readonly CurveSectionInfo _info;

        #endregion

        #region Constructor

        public CurveSection(SectionHandler handler, CurveSectionInfo info) : base(info)
        {
            _info = info;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));

            if (_info.Beam == null)
                _info.Beam = new BeamInfo();

            Sensor = new Beam(_info.Beam);
            Add(Sensor);
            Sensor.Movable = false;

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public int Zone { get; set; }

        [Browsable(false)]
        public Beam Sensor { get; }

        [Browsable(false)]
        public float SensorDistance
        {
            get => _info.SensorDistance;
            set
            {
                if (value < 0)
                    return;

                if (_info.SensorDistance.IsEffectivelyEqual(value))
                    return;

                _info.SensorDistance = value;
                UpdateSensor();
            }
        }

        [Browsable(false)]
        public SectionHandler Handler { get; }

        [Browsable(false)]
        public override AuxiliaryData.SurfaceType SurfaceType { get => base.SurfaceType; set => base.SurfaceType = value; }

        [Browsable(false)]
        public float Length { get; set; }

        [Browsable(false)]
        public override float Width { get => base.Width; set => base.Width = value; }

        [Browsable(false)]
        public override string Name { get => base.Name; set => base.Name = value; }

        [Browsable(false)]
        public override string SectionName { get => base.SectionName; set => base.SectionName = value; }

        [Browsable(false)]
        public override bool Enabled { get => base.Enabled; set => base.Enabled = value; }

        [Browsable(false)]
        public override EventCollection Events { get => base.Events; set => base.Events = value; }

        [Browsable(false)]
        public override bool Visible { get => base.Visible; set => base.Visible = value; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
                return;

            base.Refresh();

            UpdateSensor();
        }

        /// < summary>
        /// Enables or Disables the visualization of a specific Fix Point.
        /// Use <c>FixPoint.Types.Start</c> or <c>FixPoint.Types.End</c>.
        /// </summary>
        public void SetFixPointVisualization(FixPoint.Types fixPoint, bool value)
        {
            switch (fixPoint)
            {
                case FixPoint.Types.Start:

                    if (StartFixPoint.Visible != value)
                        StartFixPoint.Visible = value;

                    break;

                case FixPoint.Types.End:

                    if (EndFixPoint.Visible != value)
                        EndFixPoint.Visible = value;

                    break;
            }
        }

        /// < summary>
        /// Returns End fix point Global Position and Global Orientation.
        /// </summary>
        public (Vector3, Matrix4x4) GetEndFixPointPose()
        {
            return (EndFixPoint.Position, EndFixPoint.Orientation);
        }

        public override string ToString()
        {
            return string.Empty;
        }

        #endregion

        #region Private Methods

        private void UpdateSensor()
        {
            Sensor.Length = Width;
            var sensorCenter = new Vector3(0, 0, Radius);

            var circumference = (float)(2 * Math.PI * Radius);
            var ratio = SensorDistance / circumference;
            var yaw = (float)(ratio * 2 * Math.PI);

            var sign = Revolution == Revolution.Clockwise ? 1 : -1;
            sensorCenter = Vector3.Transform(sensorCenter, Matrix4x4.CreateFromYawPitchRoll(yaw * sign, 0, 0));
            Sensor.LocalPosition = sensorCenter + new Vector3(0, Sensor.Radius + 0.01f, -Radius);
            Sensor.LocalYaw = yaw * sign;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(CurveSectionInfo)), XmlType(TypeName ="Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.CurveSectionInfo")]
    public class CurveSectionInfo : CurveInfo
    {
        public BeamInfo Beam;
        public float SensorDistance = 0.4f;
    }
}
