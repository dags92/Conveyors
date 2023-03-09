using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Interfaces;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    public class Transfer : BasicStraight
    {
        #region Fields

        private readonly TransferInfo _info;

        private IElectricVectorMotor _motor;
        private Arrow _arrow;

        #endregion

        #region Constructor

        public Transfer(TransferInfo info) : base(info)
        {
            _info = info;
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public virtual IElectricVectorMotor VectorMotor
        {
            get => _motor;
            protected set
            {
                if (value == null && _arrow != null)
                {
                    Remove(_arrow);

                    if (_motor != null)
                    {
                        _motor.Remove(_arrow);
                        _arrow.Dispose();
                        _motor = null;
                    }
                }

                if (value != null)
                {
                    _arrow = new Arrow(0.25f, 0.01f);
                    value.Add(_arrow);
                    Add(_arrow);

                    _arrow.OnSelected += sender => Environment.Properties.Set(Motor);

                    Add(value);
                    UpdateArrows();
                }

                _motor = value;
            }
        }

        public override string Category => "Conveyors";

        public override ImageSource Image => Common.Icon.Get("Transfer");

        #endregion

        #region Protected Properties

        public Arrow VectorMotorArrow => _arrow;

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            var menu = base.ShowContextMenu();

            if (VectorMotor != null)
            {
                menu.AddRange(Experior.Conveyor.Foundations.Utilities.ContextMenu.CreateVectorMotorMenu(VectorMotor));
            }

            return menu;
        }

        public override void InsertMotor(IElectricMotor motor)
        {
            if (Environment.Scene.Loading)
            {
                return;
            }

            if (motor is IElectricVectorMotor vector)
            {
                CreateVectorMotor(vector);
            }
            else
            {
                base.InsertMotor(motor);
            }
        }

        public override void RemoveMotor(IElectricMotor motor)
        {
            if (motor == VectorMotor)
            {
                RemoveVectorMotor();
            }

            base.RemoveMotor(motor);
        }

        public override void Refresh()
        {
            if (_info == null || VectorMotor == null)
            {
                return;
            }

            base.Refresh();

            AddMotorPartsAndAssemblies();

            if (!Environment.Scene.Loading)
            {
                VectorMotor.Reset();
            }
        }

        #endregion

        #region Protected Methods

        protected override void BuildMotors()
        {
            base.BuildMotors();

            CreateVectorMotor(_info.VectorMotor != null
                ? Vector.Create(_info.VectorMotor)
                : Vector.Create());
        }

        protected override void UpdateArrows()
        {
            base.UpdateArrows();

            if (_arrow == null)
            {
                return;
            }

            _arrow.LocalYaw = 90f.ToRadians();
            _arrow.LocalRoll = -90f.ToRadians();
            _arrow.LocalPosition = new Vector3(Length / 2, 0, -Width / 2 - SideGuideWidth - 0.05f);
        }

        #endregion

        #region Private Methods

        private void CreateVectorMotor(IElectricVectorMotor motor)
        {
            if (motor == null || VectorMotor == motor)
            {
                return;
            }

            if (VectorMotor != null)
            {
                VectorMotor.Reset();
                ClearMotorPartsAndAssemblies();

                RemoveMotor(VectorMotor);
            }

            VectorMotor = motor;
            InvokeRefresh();
        }

        private void RemoveVectorMotor()
        {
            if (Environment.Scene.Loading || Environment.Scene.Disposing || VectorMotor == null)
            {
                return;
            }

            // Remove from Global List if it is not linked to other conveyors
            if (VectorMotor.Arrows.Count == 1)
            {
                Core.Motors.Motor.Items.Remove(Motor);
                VectorMotor.Dispose();
            }

            VectorMotor = null;
        }

        private void AddMotorPartsAndAssemblies()
        {
            if (VectorMotor == null)
            {
                return;
            }

            ClearMotorPartsAndAssemblies();
            if (Parts != null)
            {
                foreach (var rigidPart in Parts)
                { 
                    VectorMotor.Parts.Add(rigidPart);
                }
            }

            if (Assemblies != null)
            {
                foreach (var assembly in Assemblies)
                {
                    VectorMotor.Assemblies.Add(assembly);
                }
            }
        }

        private void ClearMotorPartsAndAssemblies()
        {
            VectorMotor?.Parts?.Clear();
            VectorMotor?.Assemblies?.Clear();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(TransferInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.TransferInfo")]
    public class TransferInfo : BasicStraightInfo
    {
        public VectorInfo VectorMotor { get; set; }
    }
}
