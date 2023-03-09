using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Core.Communication.PLC;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Parts.Sensors;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Sensors
{
    /// <summary>
    /// Class <c>Beam</c> provides a <c>Experior.Core.Parts.Sensors.Box</c> sensor with its corresponding PLC Input.
    /// </summary>
    /// <remarks>
    /// Only collision with loads is supported.
    /// </remarks>
    public class Beam : Experior.Core.Assemblies.Assembly
    {
        #region Fields

        private readonly BeamInfo _info;

        #endregion

        #region Constructor

        public Beam(BeamInfo info) : base(info)
        {
            _info = info;

            if (_info.OutputBlocked == null)
            {
                _info.OutputBlocked = new Output() { DataSize = DataSize.BOOL, Symbol = "Blocked" };
            }

            Add(_info.OutputBlocked);

            Sensor = new Experior.Core.Parts.Sensors.Box(Colors.DodgerBlue, 0.04f, 0.04f, 1f)
            {
                Collision = Collisions.Loads,
                Visible = true
            };
            Add(Sensor);
            
            Sensor.OnEnter += SensorOnEnter;
            Sensor.OnLeave += SensorOnLeave;
            Sensor.OnSelected += SensorOnSelected;

            Refresh();
        }

        #endregion

        #region Events

        public EventHandler<object> Entering;

        public EventHandler<object> Leaving;

        #endregion

        #region Public Properties

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Length")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public virtual float Length
        {
            get => _info.length;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Length value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.length.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.length = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Radius")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public virtual float Radius
        {
            get => _info.Radius;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Radius value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Radius.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Radius = value;
                InvokeRefresh();
            }
        }

        [Category("PLC Input")]
        [DisplayName("Blocked")]
        [PropertyOrder(1)]
        public Output OutputBlocked
        {
            get => _info.OutputBlocked;
            set => _info.OutputBlocked = value;
        }

        [Browsable(true)] 
        public override float Yaw { get => base.Yaw; set => base.Yaw = value; }

        [Browsable(true)] 
        public override float Roll { get => base.Roll; set => base.Roll = value; }

        [Browsable(true)] 
        public override float Pitch { get => base.Pitch; set => base.Pitch = value; }

        [Browsable(false)]
        public override bool Active => Sensor.Active;

        [Browsable(false)] 
        public List<Core.Loads.Load> Loads => Sensor.Loads;

        public override string Category => "Sensors";

        public override ImageSource Image => Resources.GetIcon("Beam");

        #endregion

        #region Protected Properties

        protected Experior.Core.Parts.Sensors.Box Sensor { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            Sensor.Width = Length;
            Sensor.Length = Sensor.Height = Radius * 2;
        }

        public override void Dispose()
        {
            Sensor.OnEnter -= SensorOnEnter;
            Sensor.OnLeave -= SensorOnLeave;
            Sensor.OnSelected -= SensorOnSelected;

            base.Dispose();
        }

        public void AttachLoad(Core.Loads.Load load)
        {
            Sensor.Attach(load);
        }

        public void UnAttachLoad(Core.Loads.Load load)
        {
            Sensor.UnAttach(load);
        }

        public void UnAttachLoads()
        {
            Sensor.UnAttach();
        }

        #endregion

        #region Private Methods

        private void SensorOnEnter(Sensor sensor, object trigger)
        {
            OutputBlocked.On();

            Entering?.Invoke(this, trigger);
        }

        private void SensorOnLeave(Sensor sensor, object trigger)
        {
            if (sensor.Loads.Count == 0)
            {
                OutputBlocked.Off();
            }

            Leaving?.Invoke(this, trigger);
        }

        private void SensorOnSelected(RigidPart sender)
        {
            Experior.Core.Environment.Properties.Set(this);
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(BeamInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Parts.Sensors.BeamInfo")]
    public class BeamInfo : Experior.Core.Assemblies.AssemblyInfo
    {
        [Browsable(false)]
        public float Radius { get; set; } = 0.003f;

        [Browsable(false)]
        public Output OutputBlocked { get; set; }
    }
}
