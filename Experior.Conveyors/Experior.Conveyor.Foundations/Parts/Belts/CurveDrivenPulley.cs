using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Belts
{
    /// <summary>
    /// Class <c>CurveDrivenPulley</c> provides the visualization of drive and tail pulleys in a curve belt.
    /// </summary>
    public class CurveDrivenPulley
    {
        #region Fields

        private float _radius, _height, _width, _angle, _heightDifference;
        private Revolution _revolution;
        private Experior.Core.Parts.Cylinder _startCylinder, _endCylinder;
        private Core.Assemblies.Assembly _parent;

        #endregion

        #region Constructor

        public CurveDrivenPulley(Core.Assemblies.Assembly parent, float radius, float width, float angle, float height, float heightDifference, Revolution revolution)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _radius = radius;
            _height = height;
            _width = width;
            _angle = angle;
            _heightDifference = heightDifference;
            _revolution = revolution;

            _startCylinder = new Cylinder(Colors.Silver, width, (height / 2) * 0.7f, 12);
            parent.Add(_startCylinder);

            _endCylinder = new Cylinder(Colors.Silver, width, (height / 2) * 0.7f, 12);
            parent.Add(_endCylinder);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Height")]
        [PropertyOrder(1)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Height
        {
            get => _height;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Height value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_height.IsEffectivelyEqual(value))
                {
                    return;
                }

                _height = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Width")]
        [PropertyOrder(2)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Width
        {
            get => _width;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Width value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_width.IsEffectivelyEqual(value))
                {
                    return;
                }

                _width = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Radius")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Radius
        {
            get => _radius;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Radius value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_radius.IsEffectivelyEqual(value))
                {
                    return;
                }

                _radius = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Angle")]
        [PropertyOrder(4)]
        [TypeConverter(typeof(RadiansToDegrees))]
        public float Angle
        {
            get => _angle;
            set
            {
                if (value < Trigonometry.Angle2Rad(10f))
                {
                    Log.Write("Angle value must be greater or equal than 10°", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_angle.IsEffectivelyEqual(value))
                {
                    return;
                }

                _angle = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Revolution")]
        [PropertyOrder(5)]
        public Revolution Revolution
        {
            get => _revolution;
            set
            {
                if (_revolution == value)
                {
                    return;
                }

                _revolution = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Height Difference")]
        [PropertyOrder(6)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float HeightDifference
        {
            get => _heightDifference;
            set
            {
                if (value < 0)
                {
                    Log.Write("Height Difference value must be greater or equal than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_heightDifference.IsEffectivelyEqual(value))
                {
                    return;
                }

                _heightDifference = value;
                InvokeRefresh();
            }
        }

        #endregion

        #region Public Methods

        public void InvokeRefresh()
        {
            Experior.Core.Environment.InvokeIfRequired(Refresh);
        }

        public void Remove()
        {
            _parent.Remove(_startCylinder);
            _parent.Remove(_endCylinder);

            _startCylinder.Dispose();
            _endCylinder.Dispose();
        }

        #endregion

        #region Private Methods

        private void Refresh()
        {
            _startCylinder.Length = Width;
            _startCylinder.Radius = (Height / 2) * 0.7f;

            _endCylinder.Length = Width;
            _endCylinder.Radius = (Height / 2) * 0.7f;

            var sign = Revolution == Revolution.Clockwise ? 1 : -1;
            _startCylinder.LocalPosition = new Vector3(_startCylinder.Radius * 1.5f * sign, -_startCylinder.Radius * 1.5f, 0);

            var tempAngle = (float)Math.Atan(_endCylinder.Radius * 1.5f / Radius);
            _endCylinder.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, -_endCylinder.Radius * 1.5f + HeightDifference, -Radius), Angle - tempAngle, Radius, Revolution);
            _endCylinder.LocalYaw = Revolution == Revolution.Clockwise ? Angle : Angle * -1;
        }

        #endregion
    }
}
