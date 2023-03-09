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

namespace Experior.Conveyor.Foundations.Parts.Belts
{
    /// <summary>
    /// Class <c>Straight</c> provides visualization of a straight conveyor belt.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Straight : Assembly
    {
        #region Fields

        private readonly Box _belt;
        private readonly Cylinder _start;
        private readonly Cylinder _end;
        private float _length, _width, _height;

        #endregion

        #region Constructor

        public Straight(float length, float height, float width) : base(new AssemblyInfo())
        {
            ListSolutionExplorer = false;

            _height = height;
            _length = length;
            _width = width;

            _belt = new Box(_length, 0.03f, _width) { Rigid = false, Color = Color };
            _start = new Cylinder(Color, _width, _height / 2, 10) { Rigid = false };
            _end = new Cylinder(Color, _width, _height / 2, 10) { Rigid = false };

            Add(_start);
            Add(_end);
            Add(_belt);

            _start.LocalRoll = (float)Math.PI / 2;
            _end.LocalRoll = (float)Math.PI / 2;

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Length")]
        [PropertyOrder(1)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Length
        {
            get => _length;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Length value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_length.IsEffectivelyEqual(value))
                {
                    return;
                }

                _length = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Size")]
        [DisplayName("Height")]
        [PropertyOrder(2)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Height
        {
            get => _height;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Height value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
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
        [PropertyOrder(3)]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        public float Width
        {
            get => _width;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Width value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
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
            _belt.Length = Length - Height;
            _belt.Height = Height;
            _belt.Width = Width;

            _end.Radius = Height / 2;
            _end.Length = Width;

            _start.Radius = Height / 2;
            _start.Length = Width;

            _belt.LocalPosition = new Vector3(Length / 2, -Height / 2, 0);
            _start.LocalPosition = new Vector3(_start.Radius, -_start.Radius, 0);
            _end.LocalPosition = new Vector3(Length - _end.Radius, -_end.Radius, 0);
        }

        #endregion
    }
}
