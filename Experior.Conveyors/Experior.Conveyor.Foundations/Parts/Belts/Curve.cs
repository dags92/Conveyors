using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Belts
{
    /// <summary>
    /// Class <c>Curve</c> provides the visualization of a curve conveyor belt.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Curve : Assembly
    {
        #region Fields

        private readonly CurveArea _belt;
        private readonly Cylinder _start;
        private readonly Cylinder _end;
        private float _height, _width, _radius, _angle, _heightDifference;
        private Revolution _revolution;

        #endregion

        #region Constructor

        public Curve(float radius, float height, float width, Revolution revolution, float angle) : base(new AssemblyInfo())
        {
            ListSolutionExplorer = false;

            _radius = radius;
            _height = height;
            _width = width;
            _revolution = revolution;
            _angle = angle;

            _belt = new CurveArea(Color, _height, _width, _radius, _angle, _revolution)
            {
                SliceAngle = 5,
                Rigid = false
            };

            _start = new Cylinder(Color, width, height / 2, 16) { Rigid = false, Visible = true };
            _end = new Cylinder(Color, width, height / 2, 16) { Rigid = false, Visible = true };

            Add(_belt);
            Add(_start);
            Add(_end);

            _start.LocalRoll = (float)Math.PI / 2;
            _end.LocalRoll = (float)Math.PI / 2;

            Refresh();
        }

        #endregion

        #region Properties

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
                    return;

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
                    return;

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
                    return;

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
                    return;

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
                if(_revolution == value)
                    return;

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
                    return;

                _heightDifference = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                _belt.Color = value;
                _start.Color = value;
                _end.Color = value;
            }
        }

        public override string Category => "Belts";

        public override ImageSource Image { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            var tempAngle = (float)Math.Atan(_start.Radius / Radius);
            _belt.Revolution = Revolution;
            _belt.Angle = Trigonometry.Rad2Angle(Angle - (tempAngle * 2));
            _belt.Radius = Radius;
            _belt.Height = Height;
            _belt.Width = Width;
            _belt.HeightDifference = HeightDifference;

            _end.Radius = Height / 2;
            _end.Length = Width;

            _start.Radius = Height / 2;
            _start.Length = Width;

            _belt.LocalPosition = new Vector3(0, -_belt.Height / 2, 0);
            _belt.LocalYaw = Revolution == Revolution.Clockwise ? tempAngle : -tempAngle;

            var tempPosStart = new Vector3(Revolution == Revolution.Clockwise ? _start.Radius : -_start.Radius, -_start.Radius, 0);
            _start.LocalPosition = Trigonometry.RotationPoint(tempPosStart, 0, Radius, Revolution);
            _end.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, -_end.Radius + HeightDifference, 0), Angle - tempAngle, Radius, Revolution);

            _end.LocalYaw = Revolution == Revolution.Clockwise ? Angle : Angle * -1;
        }

        #endregion Private Methods
    }
}
