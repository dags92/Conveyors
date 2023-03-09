using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Conveyor.Foundations.Parts.Surfaces.Straight;
using Experior.Core.Mathematics;
using Experior.Core.Motors;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Colors = System.Windows.Media.Colors;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors
{
    /// <summary>
    /// <c>BasicRoller</c> class provides the implementation of a functional roller conveyor including Side Guides and Surface Motor.
    /// </summary>
    public abstract class BasicRoller : Experior.Conveyor.Foundations.Assemblies.Straight
    {
        #region Fields

        private readonly BasicRollerInfo _info;

        #endregion

        #region Constructor

        protected BasicRoller(BasicRollerInfo info) : base(info)
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
                {
                    Belt.Visible = false;
                }
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

            Belt.Length = Length;
            Belt.Width = Width;
            Belt.Ramp = 0f;

            Belt.LocalYaw = (float)Math.PI;
            Belt.LocalPosition = new Vector3(Belt.Length / 2, -Belt.Height / 2, 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            SideGuides?.Remove();
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
            Surface = new Rollers(this, _info.GeometryData, _info.SurfaceData);
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
                return;

            Belt.Motor = null;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(BasicRollerInfo)), XmlType(TypeName = "Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors.BasicRollerInfo")]
    public class BasicRollerInfo : Experior.Conveyor.Foundations.Assemblies.StraightInfo
    {
        public Experior.Conveyor.Foundations.Data.StraightSurface SurfaceData { get; set; }

        public SurfaceInfo Motor { get; set; }
    }
}
