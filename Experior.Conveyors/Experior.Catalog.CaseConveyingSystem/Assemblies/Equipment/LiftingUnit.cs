using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Conveyor.Foundations.Pneumatics;
using Experior.Core.Mathematics;
using Experior.Core.Motors;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Base = Experior.Conveyor.Foundations.Pneumatics.Base;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment
{
    public class LiftingUnit : Conveyor.Foundations.Assemblies.Straight
    {
        #region Fields

        private readonly LiftingUnitInfo _info;
        private readonly Linear _pneumatic;

        private readonly List<ChainUnit> _chains;
        private const float ChainHeight = 0.05f;
        private const float ChainWidth = 0.03f;

        #endregion

        #region Constructor

        public LiftingUnit(LiftingUnitInfo info) : base(info)
        {
            _info = info;

            if (_info.Pneumatic == null)
            {
                _info.Pneumatic = new LinearInfo()
                {
                    Speed = 0.6f,
                    Stroke = 0.5f,
                    Valve = Base.Valves.Monostable
                };
            }

            _pneumatic = new Linear(this, _info.Pneumatic) { Axis = Base.Axes.Y };
            Add(_pneumatic);

            _chains = new List<ChainUnit>();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public override float Width { get; set; }

        [Category("Size")]
        [DisplayName("Number of Chains")]
        [PropertyOrder(0)]
        public int NumberOfChains
        {
            get => _info.NumberOfChains;
            set
            {
                if (value <= 1)
                {
                    Log.Write("Number of Chains value must be greater than 1", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if(_info.NumberOfChains.Equals(value))
                    return;

                _info.NumberOfChains = value;
                Invoke(CreateSurface);
            }
        }

        [Category("Size")]
        [DisplayName("Pitch")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(3)]
        public float ChainPitch
        {
            get => _info.ChainPitch;
            set
            {
                if (value <= 0) //TODO: DOUBLE CHECK THIS ONE
                {
                    Log.Write("Chain Pitch value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.ChainPitch.IsEffectivelyEqual(value))
                    return;

                _info.ChainPitch = value;
                Invoke(CreateSurface);
            }
        }

        [Category("Pneumatic")]
        [DisplayName("Parameters")]
        [PropertyOrder(0)]
        public Linear Pneumatic => _pneumatic;

        [Browsable(true)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                foreach (var unit in _chains)
                    unit.Chain.Color = value;
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                if(!value)
                    return;

                foreach (var unit in _chains)
                    unit.Belt.Visible = false;
            }
        }

        public override string Category => "Equipment";

        public override ImageSource Image => Common.Icon.Get("LiftingUnit");

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            var menu = new List<Environment.UI.Toolbar.BarItem>
            {
                new Environment.UI.Toolbar.Button("Reset", Common.Icon.Get("Calibrate"))
                {
                    OnClick = (sender, args) => Reset()
                }
            };

            return menu;
        }

        public override void Step(float deltatime)
        {
            Pneumatic.Step(deltatime);
        }

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            base.Refresh();
            UpdateUnitDimensions();
        }

        public override void Reset()
        {
            Pneumatic.Reset();
        }

        public override void UpdatePhotoEye(ConveyorPhotoEye photoEye)
        {
        }

        #endregion

        #region Protected Methods

        protected override void BuildAssembly()
        {
            base.BuildAssembly();

            CreateSurface();
            Refresh();
        }

        protected override void BuildMotors()
        {
            CreateSurfaceMotor(_info.Motor != null
                ? Conveyor.Foundations.Motors.Basic.Surface.Create(_info.Motor)
                : Conveyor.Foundations.Motors.Basic.Surface.Create());
        }

        protected override void CreateSurface()
        {
            if(_chains.Count > 0)
                RemoveUnits();

            for (var i = 0; i < NumberOfChains; i++)
            {
                var chain = new Experior.Conveyor.Foundations.Parts.Chains.Straight(Length, ChainHeight, ChainWidth);
                Add(chain);
                chain.Color = Color;

                var belt = new Experior.Core.Parts.ConveyorBelt(Length, AuxiliaryData.BeltHeight, ChainWidth);
                Add(belt);

                _chains.Add(new ChainUnit(chain, belt));
            }

            UpdateUnitMotor();
            UpdatePartsAndAssemblies(true);
            UpdateUnitDimensions();
        }

        protected override void CreateSurfaceMotor(IElectricSurfaceMotor motor)
        {
            base.CreateSurfaceMotor(motor);

            if (Motor != motor)
                return;

            _info.Motor = (SurfaceInfo)((Electric)Motor).Info;

            UpdateUnitMotor();
            UpdatePartsAndAssemblies(true);
        }

        protected override void RemoveSurfaceMotor()
        {
            base.RemoveSurfaceMotor();

            if (Motor != null)
                return;

            foreach (var unit in _chains)
                unit.Belt.Motor = null;

            _info.Motor = null;
        }

        #endregion

        #region Private Methods

        private void RemoveUnits()
        {
            UpdatePartsAndAssemblies(false);

            foreach (var pair in _chains)
            {
                Remove(pair.Chain);
                pair.Chain.Dispose();

                Remove(pair.Belt);
                pair.Belt.Dispose();
            }

            _chains.Clear();
        }

        private void UpdateUnitDimensions()
        {
            var count = 0;
            var width = (ChainWidth * NumberOfChains) + (Math.Abs(ChainPitch - ChainWidth) * (NumberOfChains - 1));
            foreach (var unit in _chains)
            {
                // Chain:
                unit.Chain.Length = Length;
                unit.Chain.LocalPosition = new Vector3(0, 0, -width / 2 + count * (ChainPitch + unit.Chain.Width / 2));

                // Belt:
                unit.Belt.Length = Length;
                unit.Belt.Ramp = (unit.Belt.Height / unit.Belt.Length) * 5f;
                unit.Belt.LocalYaw = (float)Math.PI;
                unit.Belt.LocalPosition = unit.Chain.LocalPosition + new Vector3(unit.Belt.Length / 2, -unit.Belt.Height / 2, 0f);
                unit.Belt.Visible = false;

                count++;
            }
        }

        private void UpdateUnitMotor()
        {
            if(Motor == null)
                return;

            foreach (var unit in _chains.Where(unit => unit.Belt.Motor != Motor))
            {
                unit.Belt.Motor = Motor;
            }
        }

        private void UpdatePartsAndAssemblies(bool update)
        {
            if (update)
            {
                foreach (var unit in _chains)
                {
                    Pneumatic.Add(unit.Chain);
                    Pneumatic.Add(unit.Belt);
                }

                if(Arrow != null)
                    Pneumatic.Add(Arrow);

                Pneumatic.Add(StartFixPoint);
                Pneumatic.Add(EndFixPoint);
            }
            else
                Pneumatic.RemoveAll();
        }

        #endregion

        #region Nested Types

        public class ChainUnit
        {
            public ChainUnit(Conveyor.Foundations.Parts.Chains.Straight chain, ConveyorBelt belt)
            {
                Chain = chain;
                Belt = belt;
            }

            public Conveyor.Foundations.Parts.Chains.Straight Chain { get; }

            public ConveyorBelt Belt { get; }
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(LiftingUnitInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment.LiftingUnitInfo")]
    public class LiftingUnitInfo : Conveyor.Foundations.Assemblies.StraightInfo
    {
        public float ChainPitch { get; set; } = 0.1f;

        public int NumberOfChains { get; set; } = 3;

        public SurfaceInfo Motor { get; set; }

        public LinearInfo Pneumatic { get; set; }
    }
}
