using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    public class Spiral : Curve
    {
        #region Fields

        private SpiralInfo _info;

        #endregion

        #region Constructor

        public Spiral(SpiralInfo info) : base(info)
        {
            _info = info;
        }

        #endregion

        #region Public Properties

        [DisplayName("Angle")]
        [Category("Size")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(RadiansToDegrees))]
        public override float Angle
        {
            get => base.Angle;
            set
            {
                if (Trigonometry.Rad2Angle(value) < 30f)
                {
                    Log.Write("Angle value must be greater than 30°", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.Angle.IsEffectivelyEqual(value))
                    return;

                _info.GeometryData.Angle = value;
                InvokeRefresh();
            }
        }

        [Browsable(true)]
        [Category("Size")]
        [DisplayName("Height Difference")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(5)]
        public override float HeightDifference
        {
            get => base.HeightDifference; 
            set => base.HeightDifference = value;
        }

        public override ImageSource Image => Common.Icon.Get("SpiralRoller");

        #endregion

        #region Protected Methods

        protected override void UpdateArrows()
        {
            base.UpdateArrows();

            if (Arrow == null)
            {
                return;
            }

            Arrow.LocalRoll = -(HeightDifference / Angle) * 2f;
            Arrow.LocalPosition += new Vector3(0, 0.01f, 0);
        }

        #endregion
    }

    [TypeConverter(typeof(SpiralInfo))]
    [Serializable]
    [XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.SpiralInfo")]
    public class SpiralInfo : CurveInfo
    {

    }
}
