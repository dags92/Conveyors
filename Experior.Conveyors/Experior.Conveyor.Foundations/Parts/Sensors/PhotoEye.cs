using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Sensors
{
    /// <summary>
    /// Class <c>PhotoEye</c> provides the visualization and functionality of a Photoelectric sensor.
    /// </summary>
    /// <remarks>
    /// Only collision with loads is supported.
    /// </remarks>
    public class PhotoEye : Beam
    {
        #region Fields

        private readonly PhotoEyeInfo _info;

        private readonly Experior.Core.Parts.Model _case;
        private readonly Experior.Core.Parts.Box _plate;

        #endregion

        #region Constructor

        public PhotoEye(PhotoEyeInfo info) : base(info)
        {
            _info = info;

            _case = new Model(Resources.GetMesh("Sensor.dae"));
            Add(_case);
            Utilities.Mesh.ScaleCadFile(0.012f, _case);

            _plate = new Box(Colors.LightSlateGray, 0.03f, 0.03f, 0.008f);
            Add(_plate);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(true)]
        public override float Length
        {
            get => base.Length;
            set => base.Length = value;
        }

        [Category("Size")]
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(3)]
        public virtual float Height
        {
            get => _info.HouseHeight;
            set
            {
                if (value < 0)
                {
                    Log.Write("House Height value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.HouseHeight.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.HouseHeight = value;
                InvokeRefresh();
            }
        }

        public override ImageSource Image => Resources.GetIcon("PhotoEye");

        #endregion

        #region Protected Properties

        protected Experior.Core.Parts.Model Case => _case;

        protected Experior.Core.Parts.Box Plate => _plate;

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            base.Refresh();

            Sensor.LocalPosition = new Vector3(0, Height, 0);

            _case.LocalPosition = new Vector3(0, Sensor.LocalPosition.Y, -Length / 2);
            _plate.LocalPosition = new Vector3(0, Sensor.LocalPosition.Y, Length / 2 + _plate.Width / 2);
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(PhotoEyeInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Parts.Sensors.PhotoEyeInfo")]
    public class PhotoEyeInfo : BeamInfo
    {
        [Browsable(false)]
        public float HouseHeight { get; set; } = 0.07f;
    }
}
