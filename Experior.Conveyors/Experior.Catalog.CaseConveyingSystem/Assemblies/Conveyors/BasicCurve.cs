using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Conveyor.Foundations.Parts.Surfaces.Curve;
using Experior.Core.Mathematics;
using Experior.Core.Motors;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Colors = System.Windows.Media.Colors;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    public abstract class BasicCurve : Conveyor.Foundations.Assemblies.Curve
    {
        #region Fields

        private readonly BasicCurveInfo _info;

        #endregion

        #region Constructor

        protected BasicCurve(BasicCurveInfo info) : base(info)
        {
            _info = info;

            if (_info.SurfaceData == null)
            {
                _info.SurfaceData = new CurvedSurface();
            }

            Belt = new ConveyorCurveBelt(false)
            {
                Height = AuxiliaryData.BeltHeight,
                SurfaceSpeedMode = _info.SurfaceMode,
                SliceAngle = 10,
                Rigid = true,
                Radius = _info.GeometryData.Radius,
                Width = _info.GeometryData.Width,
                Color = Colors.DimGray,
                Friction = _info.Friction,
                Visible = false
            };
            Add(Belt);

            SideGuides = new Experior.Conveyor.Foundations.Parts.SideGuides.Curved(this);
        }

        #endregion

        #region Public Properties

        [Category("Surface")]
        [DisplayName("Friction")]
        [PropertyOrder(0)]
        public override Friction Friction
        {
            get => _info.Friction;
            set
            {
                _info.Friction = value;

                if (Belt != null)
                {
                    Belt.Friction = value;
                }
            }
        }

        [Category("Surface")]
        [Description("Surface Speed Mode")]
        [TypeConverter(typeof(ConveyorCurveSpeedSurfaceConverter))]
        [DisplayName(@"Surface Mode")]
        [PropertyAttributesProvider("DynamicPropertySurfaceMode")]
        [PropertyOrder(1)]
        public string SurfaceMode
        {
            get
            {
                if (Belt != null)
                {
                    switch (_info.SurfaceMode)
                    {
                        case ConveyorCurveBelt.SurfaceMode.EqualAngularVelocity:
                            return "Equal Angular Velocity";
                        case ConveyorCurveBelt.SurfaceMode.EqualSurfaceVelocity:
                            return "Equal Surface Speed";
                        default:
                            return string.Empty;
                    }
                }

                return string.Empty;
            }
            set
            {
                if (Belt != null)
                {
                    switch (value)
                    {
                        case "Equal Angular Velocity":
                            _info.SurfaceMode = ConveyorCurveBelt.SurfaceMode.EqualAngularVelocity;
                            break;
                        case "Equal Surface Speed":
                            _info.SurfaceMode = ConveyorCurveBelt.SurfaceMode.EqualSurfaceVelocity;
                            break;
                    }

                    Belt.SurfaceSpeedMode = _info.SurfaceMode;
                }
            }
        }

        [Category("Surface")]
        [DisplayName("Type")]
        [PropertyOrder(2)]
        public virtual AuxiliaryData.SurfaceType SurfaceType
        {
            get => _info.SurfaceData.SurfaceType;
            set
            {
                if (_info.SurfaceData.SurfaceType == value)
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
                    Log.Write("Belt Height value must be in the range of: 40 mm < X < 60 mm", Colors.Orange, LogFilter.Information);
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
                    Log.Write("Roller Diameter value must be in the range of: 40 mm < X < 60 mm", Colors.Orange, LogFilter.Information);
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
        [TypeConverter(typeof(Degrees))]
        [Description(FloatMeterToMillimeter.Description)]
        [PropertyOrder(4)]
        public virtual float RollerPitch
        {
            get => _info.SurfaceData.RollerPitch;
            set
            {
                if (value < 6)
                {
                    Log.Write("Roller Pitch value must be greater than 6°", Colors.Orange, LogFilter.Information);
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
                    Log.Write("Strap Height value must be in the range of: 40 mm < X < 60 mm", Colors.Orange, LogFilter.Information);
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
                    Log.Write("Strap Pitch value must be in the range of: 20 mm < X < 120 mm", Colors.Orange, LogFilter.Information);
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
                    Log.Write("Strap Width value must be in the range of: 20 mm < X < 40 mm", Colors.Orange, LogFilter.Information);
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

        protected ConveyorCurveBelt Belt { get; }

        protected Experior.Conveyor.Foundations.Parts.SideGuides.Curved SideGuides { get; }

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            return Conveyor.Foundations.Utilities.ContextMenu.CreateSurfaceMotorMenu(this, Motor, true);
        }

        public override void Refresh()
        {
            if (_info == null || Surface == null)
                return;

            UpdateSurface();
            SideGuides?.Refresh();

            base.Refresh();

            Belt.Radius = Radius;
            Belt.Width = Width;
            Belt.Revolution = Revolution;
            Belt.Angle = Trigonometry.Rad2Angle(Angle);
            Belt.HeightDifference = HeightDifference;
            Belt.LocalPosition = new Vector3(0, -Belt.Height / 2, -Radius);
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

                    Surface = new Rollers(this, _info.SurfaceData, _info.GeometryData);

                    break;

                case AuxiliaryData.SurfaceType.Strap:

                    Surface = new Straps(this, _info.SurfaceData, _info.GeometryData);

                    break;

                default:

                    Surface = new Belt(this, _info.SurfaceData, _info.GeometryData);

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

        #region Nested Types

        private class ConveyorCurveSpeedSurfaceConverter : StringConverter
        {
            #region Public Methods

            /// <summary>
            /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
            /// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.</returns>
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { "Equal Angular Velocity", "Equal Surface Speed" });
            }

            /// <summary>
            /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list of possible values, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <returns>true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; false if other values are possible.</returns>
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, but allow free-form entry
                return true;
            }

            /// <summary>
            /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <returns>true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.</returns>
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            #endregion Public Methods
        }

        #endregion
    }

    [TypeConverter(typeof(BasicCurveInfo))]
    [Serializable]
    [XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.BasicCurveInfo")]
    public class BasicCurveInfo : Conveyor.Foundations.Assemblies.CurveInfo
    {
        public CurvedSurface SurfaceData { get; set; }

        public SurfaceInfo Motor { get; set; }

        public ConveyorCurveBelt.SurfaceMode SurfaceMode { get; set; } = ConveyorCurveBelt.SurfaceMode.EqualAngularVelocity;
    }
}
