using Colors = System.Windows.Media.Colors;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Numerics;
using Experior.Conveyor.Foundations;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Core.Motors;
using Environment = Experior.Core.Environment;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Core.Parts;
using System.ComponentModel;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;

namespace Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors
{
    /// <summary>
    /// <c>BasicChain</c> class provides the implementation of a functional chain conveyor including Side Guides and Surface Motor.
    /// </summary>
    public abstract class BasicChain : Experior.Conveyor.Foundations.Assemblies.Straight
    {
        #region Fields

        private readonly BasicChainConveyorInfo _info;

        private readonly Experior.Core.Parts.Box _midProfile;
        private const float ChainWidth = 0.05f;

        #endregion

        #region Constructor

        protected BasicChain(BasicChainConveyorInfo info) : base(info)
        {
            _info = info;

            if (_info.SurfaceData == null)
            {
                _info.SurfaceData = new StraightSurface()
                {
                    Color = _info.color
                };
            }

            RightBelt = new ConveyorBelt(_info.GeometryData.Length, AuxiliaryData.BeltHeight, ChainWidth, true)
            {
                Friction = _info.Friction,
                Rigid = true,
                Color = Colors.Gray,
                Visible = false
            };
            Add(RightBelt);

            LeftBelt = new ConveyorBelt(_info.GeometryData.Length, AuxiliaryData.BeltHeight, ChainWidth, true)
            {
                Friction = _info.Friction,
                Rigid = true,
                Color = Colors.Gray,
                Visible = false
            };
            Add(LeftBelt);

            _midProfile = new Box(_info.color, 0.05f, 0.015f, _info.GeometryData.Width);
            Add(_midProfile);
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

                if (RightBelt != null)
                {
                    RightBelt.Friction = value;
                }

                if (LeftBelt != null)
                {
                    LeftBelt.Friction = value;
                }
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                if (RightBelt != null)
                {
                    RightBelt.Visible = false;
                }

                if (LeftBelt != null)
                {
                    LeftBelt.Visible = false;
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

                if (_info.SurfaceData != null)
                {
                    _info.SurfaceData.Color = Color;
                    Surface?.UpdateColor();
                }

                if (_midProfile != null)
                {
                    _midProfile.Color = value;
                }
            }
        }

        #endregion

        #region Protected Properties

        protected ConveyorBelt RightBelt { get; }

        protected ConveyorBelt LeftBelt { get; }

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

            UpdateSurface();

            base.Refresh();

            RightBelt.Ramp = LeftBelt.Ramp = 0f;
            RightBelt.Length = LeftBelt.Length = Length;
            RightBelt.LocalYaw = LeftBelt.LocalYaw = (float)Math.PI;

            RightBelt.LocalPosition = new Vector3(RightBelt.Length / 2, -RightBelt.Height / 2, -Width / 2 + RightBelt.Width / 2);
            LeftBelt.LocalPosition = new Vector3(LeftBelt.Length / 2, -LeftBelt.Height / 2, Width / 2 - LeftBelt.Width / 2);

            _midProfile.Width = Width;
            _midProfile.LocalPosition = new Vector3(Length / 2, -Surface?.GetSurfaceHeight() / 2 ?? 0f, 0);
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
            Surface = new Conveyor.Foundations.Parts.Surfaces.Straight.Chain(this, _info.GeometryData, _info.SurfaceData);
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

            RightBelt.Motor = Motor;
            LeftBelt.Motor = Motor;

            _info.Motor = (SurfaceInfo)((Electric)Motor).Info;
        }

        protected override void RemoveSurfaceMotor()
        {
            base.RemoveSurfaceMotor();

            if (Motor != null)
            {
                return;
            }

            RightBelt.Motor = null;
            LeftBelt.Motor = null;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(BasicChainConveyorInfo)), XmlType(TypeName = "Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors.BasicChainConveyorInfo")]
    public class BasicChainConveyorInfo : Experior.Conveyor.Foundations.Assemblies.StraightInfo
    {
        public Experior.Conveyor.Foundations.Data.StraightSurface SurfaceData { get; set; }

        public SurfaceInfo Motor { get; set; }
    }
}
