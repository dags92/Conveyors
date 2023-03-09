using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Parts.Surfaces.Straight;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    /// <summary>
    /// <c>Alignment</c> class provides the implementation of a functional conveyor to behave as an Alignment conveyor.
    /// </summary>
    public class Alignment : Straight
    {
        #region Fields

        private readonly AlignmentInfo _info;

        private readonly Box _plate;

        #endregion

        #region Constructor

        public Alignment(AlignmentInfo info) : base(info)
        {
            _info = info;

            _plate = new Box(Colors.SlateGray, _info.length, (info.SurfaceData.RollerDiameter / 2f) * 1.2f, _info.width);
            Add(_plate);
        }

        #endregion

        #region Public Properties

        [Category("Surface")]
        [DisplayName(@"Direction Offset Angle")]
        [TypeConverter(typeof(RadiansToDegrees))]
        [PropertyOrder(3)]
        public float SurfaceDirectionOffsetAngle
        {
            get => _info.SurfaceDirectionOffsetAngle;
            set
            {
                var upperLimit = Trigonometry.Angle2Rad(20f);
                var lowerLimit = Trigonometry.Angle2Rad(5f);

                if ((value >= lowerLimit && value <= upperLimit) || (value >= -upperLimit && value <= -lowerLimit))
                {
                    if(_info.SurfaceDirectionOffsetAngle.IsEffectivelyEqual(value))
                        return;

                    _info.SurfaceDirectionOffsetAngle = value;
                    InvokeRefresh();
                }
                else
                {
                    Log.Write("Direction Offset Angle value must be in the range of: 5° ≤ X ≤ 20° || -20° ≤ X ≤ -5", Colors.Orange, LogFilter.Information);
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AuxiliaryData.SurfaceType SurfaceType { get => base.SurfaceType; set => base.SurfaceType = value; }

        [Browsable(false)]
        public override float Roll { get => base.Roll; set => base.Roll = value; }

        public override string Category => "Conveyors";

        public override ImageSource Image => Common.Icon.Get("AlignmentRoller");

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
                return;

            base.Refresh();

            _plate.Length = Length;
            _plate.Width = Width;
            _plate.Height = 0.01f; //TODO: CHECK THIS
            _plate.LocalPosition = new Vector3(Length / 2, -_plate.Height / 2 - (RollerDiameter / 2f) * 0.8f, 0);
        }

        #endregion

        #region Protected Methods

        protected override void CreateSurface()
        {
            Surface = new DiagonalRollers(this, _info.GeometryData, _info.SurfaceData)
            {
                SurfaceDirectionOffsetAngle = SurfaceDirectionOffsetAngle.ToAngle()
            };
        }

        protected override void UpdateSurface()
        {
            if (Surface is DiagonalRollers tempSurface)
            {
                tempSurface.SurfaceDirectionOffsetAngle = SurfaceDirectionOffsetAngle.ToAngle();
            }

            base.UpdateSurface();

            Belt.LocalSurfaceDirection = Trigonometry.DirectionYaw(SurfaceDirectionOffsetAngle);
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(AlignmentInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.AlignmentInfo")]
    public class AlignmentInfo : StraightInfo
    {
        public float SurfaceDirectionOffsetAngle = Trigonometry.Angle2Rad(14f);
    }
}
