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

namespace Experior.Conveyor.Foundations.Parts.Chains
{
    /// <summary>
    /// Class <c>Straight</c> provides the visualization of a single straight chain conveyor.
    /// </summary>
    /// <remarks>
    /// No interaction with dynamic objects (Loads) is provided.
    /// </remarks>
    public class Straight : Assembly
    {
        #region Fields

        private readonly Box _chain, _chainCase;
        private readonly Cylinder _start, _startCase;
        private readonly Cylinder _end, _endCase;
        private float _length, _width, _height;

        private const float Scale = 0.8f;

        #endregion

        #region Constructor

        public Straight(float length, float height, float width) : base(new AssemblyInfo())
        {
            ListSolutionExplorer = false;

            _height = height;
            _length = length;
            _width = width;

            _chain = new Box(_length, 0.03f, _width)
            {
                Rigid = false, 
                Color = Colors.DimGray
            };
            _start = new Cylinder(Colors.DimGray, _width, _height / 2, 14) { Rigid = false };
            _end = new Cylinder(Colors.DimGray, _width, _height / 2, 14) { Rigid = false };

            Add(_start);
            Add(_end);
            Add(_chain);

            _start.LocalRoll = (float)Math.PI / 2;
            _end.LocalRoll = (float)Math.PI / 2;

            _chainCase = new Box(_length, 0.03f, _width)
            {
                Rigid = false, 
                Color = Color
            };
            _startCase = new Cylinder(Color, _width, _height / 2, 14) { Rigid = false };
            _endCase = new Cylinder(Color, _width, _height / 2, 14) { Rigid = false };

            Add(_startCase);
            Add(_endCase);
            Add(_chainCase);

            _startCase.LocalRoll = (float)Math.PI / 2;
            _endCase.LocalRoll = (float)Math.PI / 2;

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

                _chainCase.Color = value;
                _startCase.Color = value;
                _endCase.Color = value;
            }
        }

        public override string Category => "Chains";

        public override ImageSource Image { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            // Case:
            _chainCase.Length = (Length - Height);
            _chainCase.Height = Height * Scale;
            _chainCase.Width = Width * 1.2f;

            _endCase.Radius = (Height / 2) * Scale;
            _endCase.Length = Width * 1.2f;

            _startCase.Radius = (Height / 2) * Scale;
            _startCase.Length = Width * 1.2f;

            _chainCase.LocalPosition = new Vector3(Length / 2, -Height / 2, 0);
            _startCase.LocalPosition = new Vector3(_start.Radius, -_start.Radius, 0);
            _endCase.LocalPosition = new Vector3(Length - _end.Radius, -_end.Radius, 0);

            // Chain:
            _chain.Length = Length - Height;
            _chain.Height = Height;
            _chain.Width = Width * 0.75f;

            _end.Radius = Height / 2;
            _end.Length = Width * 0.75f;

            _start.Radius = Height / 2;
            _start.Length = Width * 0.75f;

            _chain.LocalPosition = new Vector3(Length / 2, -Height / 2, 0);
            _start.LocalPosition = new Vector3(_start.Radius, -_start.Radius, 0);
            _end.LocalPosition = new Vector3(Length - _end.Radius, -_end.Radius, 0);
        }

        #endregion
    }
}
