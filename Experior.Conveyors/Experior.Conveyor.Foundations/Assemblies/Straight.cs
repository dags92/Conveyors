using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Conveyor.Foundations.Parts.Boundaries;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Assemblies
{
    /// <summary>
    /// Class <c>Straight</c> contains implementation of <c>Boundaries</c>.
    /// </summary>
    public abstract class Straight : Foundation, IStraightPhotoEye
    {
        #region Fields

        private readonly StraightInfo _info;
        private IStraightSurface _surface;

        #endregion

        #region Constructor

        protected Straight(StraightInfo info) 
            :base(info)
        {
            _info = info;

            if (_info.Boundaries == null)
            {
                _info.Boundaries = new Boundaries();
            }

            if (_info.GeometryData == null)
            {
                _info.GeometryData = new BoxGeometry();
            }

            Boundaries = new StraightBoundaries(this, _info.Boundaries);
        }

        #endregion

        #region Public Properties

        [Category("Size")]
        [DisplayName("Length")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public virtual float Length
        {
            get => _info.GeometryData.Length;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Length value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.GeometryData.Length.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.GeometryData.Length = value;
                InvokeRefresh();

                DimensionChanged?.Invoke(this, EventArgs.Empty);
                //Collector?.UpdateBoundaries();
            }
        }

        [Category("Size")]
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
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
                //Collector?.UpdateBoundaries();
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
        public virtual bool UseRightBoundary
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
        [DisplayName("Left")]
        [PropertyOrder(3)]
        public virtual bool UseLeftBoundary
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
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(4)]
        public virtual float SideGuideHeight
        {
            get => _info.Boundaries.SideGuideHeight;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Side Guide Height value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
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
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(5)]
        public virtual float SideGuideWidth
        {
            get => _info.Boundaries.SideGuideWidth;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Side Guide Height value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Boundaries.SideGuideHeight.IsEffectivelyEqual(value))
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
        public float SideGuideRamp
        {
            get => _info.Boundaries.Ramp;
            set
            {
                if (value < 0)
                {
                    Log.Write("Side Guide Ramp value must be greater or equal than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Boundaries.Ramp.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Boundaries.Ramp = value;
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
        public IStraightSurface Surface
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
                Boundaries?.UpdateColor();
            }
        }

        #endregion

        #region Protected Properties

        protected StraightBoundaries Boundaries { get; }

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
            if (photoEye == null)
            {
                return;
            }

            photoEye.Length = Width;

            if (!photoEye.AutomaticHeight)
            {
                return;
            }

            var tempHeight = UseRightBoundary || UseLeftBoundary ? SideGuideHeight: 0f;
            photoEye.Height = tempHeight;
        }

        public override void AfterMoving()
        {
            base.AfterMoving();
            Boundaries?.Splitting();
        }

        public override void BeforeMoving()
        {
            Boundaries?.ClearSplitting();
        }

        public void Snapping(FixPoint stranger, FixPoint.SnapEventArgs e)
        {
            stranger.Attached.Visible = false;
            Boundaries?.Splitting();
        }

        public override void Dispose()
        {
            Boundaries?.DisposeAssembly();

            base.Dispose();
        }

        #endregion

        #region Protected Methods

        protected override void UpdateFixPoints()
        {
            StartFixPoint.LocalYaw = (float)Math.PI;
            EndFixPoint.LocalYaw = (float)Math.PI;

            StartFixPoint.LocalPosition = Vector3.Zero;
            EndFixPoint.LocalPosition = new Vector3(Length, 0, 0);
        }

        protected override void UpdateArrows()
        {
            if (Arrow == null)
            {
                return;
            }

            Arrow.LocalYaw = (float)Math.PI;
            Arrow.LocalPosition = new Vector3(Length / 2, Arrow.Height / 2, 0);
        }

        protected abstract void CreateSurface();

        protected void RemoveSurface()
        {
            _surface?.Dispose();
            _surface = null;
        }

        protected override void BuildAssembly()
        {
            Boundaries?.UpdateBoundaries();
        }

        protected override void ExecuteSceneOnLoaded()
        {
            base.ExecuteSceneOnLoaded();
            Boundaries?.Splitting();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(StraightInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Assemblies.StraightInfo")]
    public class StraightInfo : FoundationInfo
    {
        public Data.Boundaries Boundaries { get; set; }

        public Data.BoxGeometry GeometryData { get; set; }
    }
}
