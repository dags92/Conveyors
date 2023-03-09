using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;
using Standard = Experior.Conveyor.Foundations.Parts.Supports.Standard;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    /// <summary>
    /// <c>Merge</c> class provides the implementation of a functional conveyor to behave as a Merge or Divert type.
    /// </summary>
    public class Merge : Straight
    {
        #region Fields

        private readonly MergeInfo _info;

        private readonly ConveyorBelt _beltNose;

        #endregion

        #region Constructor

        public Merge(MergeInfo info) : base(info)
        {
            _info = info;

            _beltNose = new ConveyorBelt(BeltNoseLength, AuxiliaryData.BeltHeight, _info.width, true, 0.025f)
            {
                Color = Colors.Gray,
                RampSide = RampSides.None,
                RampEnd = RampEnds.Front,
                Friction = Belt.Friction,
                Motor = Belt.Motor,
                Visible = false
            };
            Add(_beltNose);
        }

        #endregion

        #region Enums

        [XmlType(TypeName = "CaseConveyingSystem.FlowDirection")]
        public enum FlowDirection
        {
            Infeed,
            Outfeed
        }

        #endregion Public Enums

        #region Public Properties

        [Browsable(false)]
        public override IElectricSurfaceMotor Motor
        {
            get => base.Motor;
            protected set
            {
                base.Motor = value;

                if (_beltNose != null)
                {
                    _beltNose.Motor = value;
                }
            }
        }

        [Browsable(true)]
        public override Friction Friction
        {
            get => base.Friction;
            set
            {
                base.Friction = value;

                if (_beltNose != null)
                {
                    _beltNose.Friction = value;
                }
            }
        }

        [Category("Size")]
        [DisplayName("Angle")]
        [TypeConverter(typeof(RadiansToDegrees))]
        public float NoseAngle
        {
            get => _info.SurfaceData.NoseAngle;
            set
            {
                if (value > 60f.ToRadians() || value < 30f.ToRadians())
                {
                    Log.Write("Angle value must be in the range of: 30° ≤ X ≤ 60°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.NoseAngle.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.SurfaceData.NoseAngle = value;
                InvokeRefresh();

                Collector?.UpdateBoundaries();
            }
        }

        [Category("Orientation")]
        [DisplayName("Direction")]
        public AuxiliaryData.NoseOverDirection Direction
        {
            get => _info.SurfaceData.NoseOverDirection;
            set
            {
                if (_info.SurfaceData.NoseOverDirection == value || value == AuxiliaryData.NoseOverDirection.None)
                {
                    return;
                }

                _info.SurfaceData.NoseOverDirection = value;
                InvokeRefresh();
            }
        }

        [Category("Orientation")]
        [DisplayName("Flow Direction")]
        public FlowDirection Flow
        {
            get => _info.Flow;
            set
            {
                if(_info.Flow == value)
                    return;

                _info.Flow = value;
                Motor?.Reset();

                if (value == FlowDirection.Outfeed)
                {
                    Feeder.RemoveFeeder();
                }

                InvokeRefresh();
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                if (_beltNose != null)
                {
                    _beltNose.Visible = false;
                }
            }
        }

        [Browsable(false)]
        public override float Roll { get => base.Roll; set => base.Roll = value; }

        public override string Category => "Conveyors";

        public override ImageSource Image => Flow == FlowDirection.Infeed ? Common.Icon.Get("MergeRoller") : Common.Icon.Get("DivertRoller");

        #endregion

        #region Protected Properties

        protected float BeltNoseLength
        {
            get
            {
                if (!NoseAngle.IsEffectivelyZero())
                {
                    return Width / (float)Math.Tan(NoseAngle);
                }

                return 0f;
            }
        }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            base.Refresh();

            _beltNose.Length = BeltNoseLength;
            _beltNose.Width = Width;
            _beltNose.LocalPosition = new Vector3(Length + _beltNose.Length / 2, -_beltNose.Height / 2 - 0.001f, 0);
            _beltNose.LocalYaw = (float)Math.PI;

            switch (Direction)
            {
                case AuxiliaryData.NoseOverDirection.Right:
                    _beltNose.EndOffsetLeft = 1;
                    _beltNose.EndOffsetRight = 0;
                    break;

                case AuxiliaryData.NoseOverDirection.Left:
                    _beltNose.EndOffsetRight = 1;
                    _beltNose.EndOffsetLeft = 0;
                    break;

                default:
                    _beltNose.EndOffsetRight = 0;
                    _beltNose.EndOffsetLeft = 0;
                    break;
            }

            SetFlowDirection();
            UpdateArrows();
        }

        #endregion

        #region Protected Methods

        protected override void UpdateSupports()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (SupportStart == null || SupportEnd == null)
                {
                    return;
                }

                if ((UseSupport && (StartFixPoint.Position.Y <= 0.2f || EndFixPoint.Position.Y <= 0.2f) || (!(UseSupport && Visible))))
                {
                    SupportStart.Visible = SupportEnd.Visible = false;
                    return;
                }

                SupportStart.Visible = SupportEnd.Visible = true;

                SupportStart.Width = Width + SideGuideWidth * 1.5f;
                SupportStart.Height = StartFixPoint.Position.Y - Section.Position.Y - (Surface.GetSurfaceHeight());
                SupportEnd.Height = EndFixPoint.Position.Y - Section.Position.Y - (Surface.GetSurfaceHeight());

                SupportStart.Width = Width;
                SupportEnd.Width = BeltNoseLength / (float)Math.Cos(NoseAngle);

                SupportStart.LocalPosition = new Vector3(Standard.FootLength, -Surface.GetSurfaceHeight() - SupportStart.Height / 2, 0);

                var tempAngle = (float)Math.Atan(BeltNoseLength / Width);
                SupportEnd.LocalPosition = new Vector3(Length + BeltNoseLength / 2 - Standard.FootLength / 2, -Surface.GetSurfaceHeight() - SupportEnd.Height / 2, 0);
                SupportEnd.LocalYaw = Direction == AuxiliaryData.NoseOverDirection.Right ? tempAngle : -tempAngle;
            });
        }

        protected override void UpdateArrows()
        {
            if (Arrow == null || _info == null)
                return;

            Arrow.LocalPosition = new Vector3(Belt.Length / 2, 0, 0);
            Arrow.LocalYaw = Flow == FlowDirection.Outfeed ? 0f : (float)Math.PI;
        }

        #endregion

        #region Private Methods

        private void SetFlowDirection()
        {
            switch (Flow)
            {
                case FlowDirection.Outfeed:
                    StartFixPoint.LocalYaw = 0f;
                    StartFixPoint.LocalPosition = new Vector3(Length + BeltNoseLength / 2, 0, 0);

                    EndFixPoint.LocalPosition = new Vector3(0, 0, 0);
                    EndFixPoint.LocalYaw = 0f;
                    EndFixPoint.Visible = true;

                    Belt.LocalSurfaceDirection = new Vector3(1, 0, 0);
                    _beltNose.LocalSurfaceDirection = new Vector3(1, 0, 0);
                    break;

                default:
                    StartFixPoint.LocalYaw = (float)Math.PI;
                    StartFixPoint.LocalPosition = new Vector3(0, 0, 0);

                    EndFixPoint.LocalPosition = new Vector3(Length + BeltNoseLength / 2, 0, 0);
                    EndFixPoint.Visible = false;

                    Belt.LocalSurfaceDirection = new Vector3(-1, 0, 0);
                    _beltNose.LocalSurfaceDirection = new Vector3(-1, 0, 0);
                    break;
            }
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(MergeInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.MergeInfo")]
    public class MergeInfo : StraightInfo
    {
        public Merge.FlowDirection Flow { get; set; } = Merge.FlowDirection.Infeed;
    }
}
