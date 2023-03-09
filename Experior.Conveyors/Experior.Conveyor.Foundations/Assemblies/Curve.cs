using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Assemblies
{
    /// <summary>
    /// Class <c>Curve</c> contains implementation of <c>Boundaries</c>.
    /// </summary>
    public abstract class Curve : Foundation, ICurvePhotoEye
    {
        #region Private Fields

        private readonly CurveInfo _info;
        private ICurveSurface _surface;

        #endregion Private Fields

        #region Public Constructors

        protected Curve(CurveInfo info) : base(info)
        {
            _info = info;

            if (_info.Boundaries == null)
            {
                _info.Boundaries = new Boundaries();
            }

            if (_info.GeometryData == null)
            {
                _info.GeometryData = new CurvedGeometry();
            }

            Boundaries = new Parts.Boundaries.Curved(this, _info.Boundaries);
        }

        #endregion Public Constructors

        #region Public Properties

        [Category("Size")]
        [DisplayName("Angle")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(RadiansToDegrees))]
        public virtual float Angle
        {
            get => _info.GeometryData.Angle;
            set
            {
                if (value.ToAngle() < 30f || value.ToAngle() > 180f)
                {
                    Log.Write("Angle value must be in the range of: 30° ≤ X ≤ 180°", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.Angle.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.GeometryData.Angle = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Size")]
        [DisplayName("Radius")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(0)]
        public virtual float Radius
        {
            get => _info.GeometryData.Radius;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Radius value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.Radius.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.GeometryData.Radius = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Orientation"), Description("Direction")]
        [DisplayName(@"Revolution")]
        [PropertyOrder(0)]
        public virtual Revolution Revolution
        {
            get => _info.GeometryData.Revolution;
            set
            {
                if (_info.GeometryData.Revolution == value)
                {
                    return;
                }

                _info.GeometryData.Revolution = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Size")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public virtual float Width
        {
            get => _info.GeometryData.Width;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Width value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.Width.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.GeometryData.Width = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public virtual float HeightDifference
        {
            get => _info.GeometryData.HeightDifference;
            set
            {
                if (value < 0)
                {
                    Log.Write("Height Difference value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.HeightDifference.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.GeometryData.HeightDifference = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(true)]
        public override float Yaw
        {
            get => base.Yaw;
            set
            {
                base.Yaw = value;
                UnSnap();
            }
        }

        [Category("Boundaries")]
        [DisplayName("Start")]
        [PropertyOrder(0)]
        public bool UseStartBoundary
        {
            get => _info.Boundaries.UseStartBoundary;
            set
            {
                if (value == _info.Boundaries.UseStartBoundary)
                {
                    return;
                }

                _info.Boundaries.UseStartBoundary = value;
                Boundaries.CreateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [DisplayName("End")]
        [PropertyOrder(1)]
        public bool UseEndBoundary
        {
            get => _info.Boundaries.UseEndBoundary;
            set
            {
                if (value == _info.Boundaries.UseEndBoundary)
                {
                    return;
                }

                _info.Boundaries.UseEndBoundary = value;
                Boundaries.CreateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [DisplayName("Right")]
        [PropertyOrder(2)]
        public bool UseRightBoundary
        {
            get => _info.Boundaries.UseRightBoundary;
            set
            {
                if (value == _info.Boundaries.UseRightBoundary)
                {
                    return;
                }

                _info.Boundaries.UseRightBoundary = value;
                Boundaries.CreateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [DisplayName("Lef")]
        [PropertyOrder(3)]
        public bool UseLeftBoundary
        {
            get => _info.Boundaries.UseLeftBoundary;
            set
            {
                if (value == _info.Boundaries.UseLeftBoundary)
                {
                    return;
                }

                _info.Boundaries.UseLeftBoundary = value;
                Boundaries.CreateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(4)]
        [DisplayName("Height")]
        public float SideGuideHeight
        {
            get => _info.Boundaries.SideGuideHeight;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Side Guide Height value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Boundaries.SideGuideHeight.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Boundaries.SideGuideHeight = value;
                Boundaries.UpdateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(5)]
        [DisplayName("Width")]
        public float SideGuideWidth
        {
            get => _info.Boundaries.SideGuideWidth;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Side Guide Width value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Boundaries.SideGuideWidth.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Boundaries.SideGuideWidth = value;
                Boundaries.UpdateBoundaries();
                UpdatePhotoEyes();
            }
        }

        [Category("Boundaries")]
        [DisplayName("Ramp")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(6)]
        public bool SideGuideRamp
        {
            get => _info.Boundaries.UseRamp;
            set
            {
                if (_info.Boundaries.UseRamp == value)
                {
                    return;
                }

                _info.Boundaries.UseRamp = value;
                Boundaries.UpdateBoundaries();
            }
        }

        [Category("Boundaries")]
        [DisplayName("Friction")]
        [PropertyOrder(7)]
        public virtual Friction SideGuideFriction
        {
            get => _info.Boundaries.Friction;
            set => _info.Boundaries.Friction = value;
        }

        [Browsable(false)]
        public ICurveSurface Surface
        {
            get => _surface;
            protected set
            {
                RemoveSurface();
                _surface = value;
            }
        }

        [Browsable(true)]
        [Category("Boundaries")]
        [DisplayName("Color")]
        [PropertyOrder(8)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                _info.Boundaries.Color = value;
                Boundaries?.UpdateColor();
            }
        }

        #endregion Public Properties

        #region Protected Properties

        private Parts.Boundaries.Curved Boundaries { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            Boundaries.UpdateBoundaries();

            base.Refresh();
        }

        public override void UpdatePhotoEye(ConveyorPhotoEye photoEye)
        {
            if (photoEye == null || Surface == null)
            {
                return;
            }

            photoEye.Length = Width;
            photoEye.Revolution = Revolution;

            if (!photoEye.AutomaticHeight)
            {
                return;
            }

            var tempHeight = UseRightBoundary || UseLeftBoundary ? SideGuideHeight: 0f;
            photoEye.Height = tempHeight;
        }

        public override bool Snap(FixPoint stranger, FixPoint local)
        {
            //Only rotate yaw - otherwise the roll will be changed
            return SnapRotateOnlyYaw(stranger, local);
        }

        #endregion

        #region Protected Methods

        protected override void BuildAssembly()
        {
            Boundaries?.UpdateBoundaries();
        }

        protected override void UpdateFixPoints()
        {
            StartFixPoint.LocalPosition = Trigonometry.RotatePoint(new Vector3(0f, 0f, -Radius), 0f, 0f, Radius);

            if (Revolution == Revolution.Clockwise)
            {
                EndFixPoint.LocalPosition = Trigonometry.RotatePoint(new Vector3(0f, 0f, -Radius), Angle, Radius);
                EndFixPoint.LocalPosition = new Vector3(EndFixPoint.LocalPosition.X, HeightDifference, EndFixPoint.LocalPosition.Z);
                EndFixPoint.LocalYaw = Angle + (float)Math.PI;
                StartFixPoint.LocalYaw = (float)Math.PI;
            }
            else
            {
                EndFixPoint.LocalPosition = Trigonometry.RotatePoint(new Vector3(0f, 0f, -Radius), Angle, 0f, Radius, Revolution);
                EndFixPoint.LocalPosition = new Vector3(EndFixPoint.LocalPosition.X, HeightDifference, EndFixPoint.LocalPosition.Z);
                EndFixPoint.LocalYaw = -Angle;
                StartFixPoint.LocalYaw = 0f;
            }
        }

        protected override void UpdateArrows()
        {
            if (Arrow == null)
            {
                return;
            }

            var center = new Vector3(0, 0, -Radius);
            Arrow.LocalPosition = center + Trigonometry.RotationPoint(Vector3.Zero, Angle / 2, Radius, 0f, Revolution);
            Arrow.LocalPosition = new Vector3(Arrow.LocalPosition.X, HeightDifference / 2 + Arrow.Height / 2, Arrow.LocalPosition.Z);
            Arrow.LocalYaw = Revolution == Revolution.Clockwise ? Angle / 2 - (float)Math.PI : -Angle / 2;
        }

        protected abstract void CreateSurface();

        protected void RemoveSurface()
        {
            _surface?.Dispose();
            _surface = null;
        }

        #endregion

        #region Private Methods

        private bool SnapRotateOnlyYaw(FixPoint stranger, FixPoint local)
        {
            var parent = TopParent;

            Matrix4x4.Invert(parent.Orientation, out Matrix4x4 inverseWorld);
            Matrix4x4.Invert(local.Orientation * inverseWorld, out Matrix4x4 localInvert);

            var orientation = localInvert * stranger.Orientation;
            parent.Yaw = Trigonometry.Yaw(orientation);

            var diff = local.Position - parent.Position;
            parent.Position = Trigonometry.TranslatePoint(stranger.Position, -diff);
            return true;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(CurveInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Assemblies.CurveInfo")]
    public class CurveInfo : FoundationInfo
    {
        public Data.Boundaries Boundaries { get; set; }

        public Data.CurvedGeometry GeometryData { get; set; }
    }
}
