using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Belts
{
    /// <summary>
    /// Class <c>CurveDrivenPulley</c> provides the visualization of drive and tail pulleys in a straight belt.
    /// </summary>
    public class StraightDrivenPulley
    {
        #region Fields

        private float _length, _width, _height;
        private AuxiliaryData.NoseOverDirection _noseOverDirection;
        private readonly Core.Assemblies.Assembly _parent;
        private readonly Experior.Core.Parts.Cylinder _startCylinder;
        private readonly Experior.Core.Parts.Cylinder _endCylinder;

        #endregion

        #region Constructor

        public StraightDrivenPulley(Core.Assemblies.Assembly parent, float length, float height, float width, AuxiliaryData.NoseOverDirection noseDirection)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _length = length;
            _width = width;
            _height = height;
            _noseOverDirection = noseDirection;

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
                    return;

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
                    return;

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
                    return;

                _width = value;
                InvokeRefresh();
            }
        }

        [Category("Size")]
        [DisplayName("Nose Over Direction")]
        [PropertyOrder(4)]
        public AuxiliaryData.NoseOverDirection NoseOverDirection
        {
            get => _noseOverDirection;
            set
            {
                if (NoseOverDirection == value)
                    return;

                _noseOverDirection = value;
                InvokeRefresh();
            }
        }

        #endregion

        #region Public Methods

        public void InvokeRefresh()
        {
            Experior.Core.Environment.InvokeIfRequired(Refresh);
        }

        public void Dispose()
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

            _startCylinder.LocalPosition = new Vector3(_startCylinder.Radius * 1.5f, -_startCylinder.Radius * 1.5f, 0);
            _endCylinder.LocalPosition =NoseOverDirection == AuxiliaryData.NoseOverDirection.None ? new Vector3(-_endCylinder.Radius * 1.5f + Length, -_endCylinder.Radius * 1.5f, 0) : _startCylinder.LocalPosition;
        }

        #endregion
    }
}
