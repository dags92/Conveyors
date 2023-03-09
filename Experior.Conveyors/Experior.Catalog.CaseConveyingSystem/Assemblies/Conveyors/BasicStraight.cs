using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Conveyor.Foundations.Parts.Surfaces.Straight;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Motors;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Interfaces.Collections;
using Colors = System.Windows.Media.Colors;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    /// <summary>
    /// <c>BasicStraight</c> class provides the implementation of a functional conveyor including Surface, Side Guides and Surface Motor.
    /// </summary>
    public abstract class BasicStraight : Experior.Conveyor.Foundations.Assemblies.Straight
    {
        #region Fields

        private readonly BasicStraightInfo _info;

        #endregion

        #region Constructor

        protected BasicStraight(BasicStraightInfo info)
            : base(info)
        {
            _info = info;

            if (_info.SurfaceData == null)
            {
                _info.SurfaceData = new StraightSurface();
            }

            Belt = new ConveyorBelt(_info.length, AuxiliaryData.BeltHeight, _info.width, true)
            {
                Friction = _info.Friction,
                Rigid = true,
                Color = Colors.Gray,
                Visible = false
            };
            Add(Belt);

            SideGuides = new Experior.Conveyor.Foundations.Parts.SideGuides.Straight(this);
        }

        #endregion

        #region Public Properties

        [Browsable(true)]
        public override Friction Friction
        {
            get => base.Friction;
            set
            {
                base.Friction = value;

                if (Belt != null)
                {
                    Belt.Friction = value;
                }
            }
        }

        [Category("Surface")]
        [DisplayName("Type")]
        [PropertyOrder(1)]
        public virtual AuxiliaryData.SurfaceType SurfaceType
        {
            get => _info.SurfaceData.SurfaceType;
            set
            {
                if(_info.SurfaceData.SurfaceType ==  value)
                    return;

                _info.SurfaceData.SurfaceType = value;
                CreateSurface();
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyAttributesProvider("DynamicPropertyBelt")]
        [DisplayName("Belt Height")]
        [PropertyOrder(2)]
        public float BeltHeight
        {
            get => _info.SurfaceData.BeltHeight;
            set
            {
                if (value < 0.04f || value > 0.06f)
                {
                    Log.Write("Belt Height value must be in the range of: 40 mm ≤ X ≤ 60 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.BeltHeight.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.BeltHeight = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName(@"Roller Diameter")]
        [PropertyAttributesProvider("DynamicPropertyRollers")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [Description(FloatMeterToMillimeter.Description)]
        [PropertyOrder(3)]
        public virtual float RollerDiameter
        {
            get => _info.SurfaceData.RollerDiameter;
            set
            {
                if (value < 0.04f || value > 0.06f)
                {
                    Log.Write("Roller Diameter value must be in the range of: 40 mm ≤ X ≤ 60 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.RollerDiameter.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.RollerDiameter = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName(@"Roller Pitch")]
        [PropertyAttributesProvider("DynamicPropertyRollers")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [Description(FloatMeterToMillimeter.Description)]
        [PropertyOrder(4)]
        public virtual float RollerPitch
        {
            get => _info.SurfaceData.RollerPitch;
            set
            {
                if (value < 0.06f || value > 0.12f)
                {
                    Log.Write("Roller Pitch value must be in the range of: 60 mm ≤ X ≤ 120 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.RollerPitch.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.RollerPitch = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName("Strap Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyAttributesProvider("DynamicPropertyStraps")]
        [PropertyOrder(5)]
        public float StrapHeight
        {
            get => _info.SurfaceData.StrapHeight;
            set
            {
                if (value < 0.04f || value > 0.06f)
                {
                    Log.Write("Strap Height value must be in the range of: 40 mm ≤ X ≤ 60 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.StrapHeight.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.StrapHeight = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName(@"Strap Pitch")]
        [PropertyAttributesProvider("DynamicPropertyStraps")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [Description(FloatMeterToMillimeter.Description)]
        [PropertyOrder(6)]
        public virtual float StrapPitch
        {
            get => _info.SurfaceData.StrapPitch;
            set
            {
                if (value < 0.02f || value > 0.12f)
                {
                    Log.Write("Strap Pitch value must be in the range of: 20 mm ≤ X ≤ 120 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.StrapPitch.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.StrapPitch = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName("Strap Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyAttributesProvider("DynamicPropertyStraps")]
        [PropertyOrder(7)]
        public float StrapWidth
        {
            get => _info.SurfaceData.StrapWidth;
            set
            {
                if (value < 0.02f || value > 0.04f)
                {
                    Log.Write("Strap Width value must be in the range of: 20 mm ≤ X ≤ 40 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SurfaceData.StrapWidth.IsEffectivelyEqual(value))
                    return;

                _info.SurfaceData.StrapWidth = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName("Color")]
        [PropertyOrder(8)]
        public Color SurfaceColor
        {
            get => _info.SurfaceData.Color;
            set
            {
                _info.SurfaceData.Color = value;
                Surface?.UpdateColor();
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                if (Belt != null)
                    Belt.Visible = false;
            }
        }

        [Browsable(true)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                if (SideGuides != null)
                {
                    SideGuides.Color = Color;
                }
            }
        }

        #endregion

        #region Protected Properties

        protected ConveyorBelt Belt { get; }

        protected Experior.Conveyor.Foundations.Parts.SideGuides.Straight SideGuides { get; }

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            return Experior.Conveyor.Foundations.Utilities.ContextMenu.CreateSurfaceMotorMenu(this, Motor, true);
        }

        public override void Refresh()
        {
            if (_info == null || Surface == null)
            {
                return;
            }

            base.Refresh();

            UpdateSurface();
            SideGuides?.Refresh();

            Belt.Ramp = 0f;
            Belt.Length = Length;
            Belt.Width = Width;

            Belt.LocalYaw = (float)Math.PI;
            Belt.LocalPosition = new Vector3(Belt.Length / 2, -Belt.Height / 2, 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            SideGuides?.Remove();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                StartFixPoint.Visible = !StartFixPoint.Visible;
                EndFixPoint.Visible = !EndFixPoint.Visible;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyBelt(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = SurfaceType == AuxiliaryData.SurfaceType.Belt;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyRollers(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = SurfaceType == AuxiliaryData.SurfaceType.Roller;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyStraps(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = SurfaceType == AuxiliaryData.SurfaceType.Strap;
        }

        #endregion

        #region Protected Methods

        protected override void BuildAssembly()
        {
            base.BuildAssembly();

            CreateSurface();
        }

        protected override void BuildMotors()
        {
            CreateSurfaceMotor(_info.Motor != null
                ? Conveyor.Foundations.Motors.Basic.Surface.Create(_info.Motor)
                : Conveyor.Foundations.Motors.Basic.Surface.Create());
        }

        protected override void CreateSurface()
        {
            switch (SurfaceType)
            {
                case AuxiliaryData.SurfaceType.Roller:

                    Surface = new Rollers(this, _info.GeometryData, _info.SurfaceData);

                    break;

                case AuxiliaryData.SurfaceType.Strap:

                    Surface = new Straps(this, _info.GeometryData, _info.SurfaceData);

                    break;

                default:

                    Surface = new Belt(this, _info.GeometryData, _info.SurfaceData);

                    break;
            }
        }

        protected virtual void UpdateSurface()
        {
            Surface.Refresh();
            Surface.UpdateColor();
        }

        protected override void CreateSurfaceMotor(IElectricSurfaceMotor motor)
        {
            base.CreateSurfaceMotor(motor);

            if (Motor != motor)
            {
                return;
            }

            Belt.Motor = Motor;
            _info.Motor = (SurfaceInfo)((Electric)Motor).Info;
        }

        protected override void RemoveSurfaceMotor()
        {
            base.RemoveSurfaceMotor();

            if (Motor != null)
            {
                return;
            }

            Belt.Motor = null;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(BasicStraightInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.BasicStraightInfo")]
    public class BasicStraightInfo : Experior.Conveyor.Foundations.Assemblies.StraightInfo
    {
        public Experior.Conveyor.Foundations.Data.StraightSurface SurfaceData { get; set; }

        public SurfaceInfo Motor { get; set; }
    }
}