using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Core.Communication.PLC;
using Experior.Core.Mathematics;
using Experior.Core.Motors;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Environment = Experior.Core.Environment;

namespace Experior.Conveyor.Foundations.Motors.Basic
{
    /// <summary>
    /// Abstract class <c>SplittableStraight</c> contains common class members and behaviors to recreate a Forward/Backward motor functionality.
    /// </summary>
    public abstract class Base : Electric
    {
        #region Fields

        private readonly BaseInfo _info;

        private bool _move;
        private AuxiliaryData.Commands _command;

        #endregion

        #region Constructor

        protected Base(BaseInfo info) : base(info)
        {
            _info = info;

            // Foundations does not use inheritance from Catalog and therefore, it is necessary to manually add the Motor Type.
            if(!Experior.Common.SharedStates.MotorTypes.ContainsKey(GetType()))
                Experior.Common.SharedStates.MotorTypes.Add(GetType(), GetType().ToString());

            SetPlcSignals();
            Motion = new Motion { EnableAcceleration = info.UseRamp };

            _command = AuxiliaryData.Commands.Forward;
            info.MechanicalSwitch.State.On();
        }

        #endregion

        #region Events

        public EventHandler<float> TargetSpeedReached;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the speed. 
        /// </summary>
        [Display(Order = 1, GroupName = "Speed")]
        [DisplayName(@"Speed")]
        [TypeConverter(typeof(MeterPerSeconds))]
        [PropertyOrder(1)]
        public override float Speed
        {
            get => _info.BaseSpeed;
            set
            {
                if (value <= 0)
                    return;

                _info.BaseSpeed = value;
                if (Running)
                {
                    Controller();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Alternative speed. 
        /// </summary>
        [Display(Order = 1, GroupName = "Speed")]
        [DisplayName(@"Alternative Speed")]
        [TypeConverter(typeof(MeterPerSeconds))]
        [PropertyOrder(2)]
        public float AlternativeSpeed
        {
            get => _info.AlternativeSpeed;
            set
            {
                if (value <= 0)
                {
                    return;
                }

                _info.AlternativeSpeed = value;
                if (Running)
                {
                    Controller();
                }
            }
        }

        /// <summary>
        /// Gets or sets the use of ramp for acceleration and deceleration states.
        /// </summary>
        [Display(Order = 1, GroupName = "Acceleration/Deceleration")]
        [DisplayName(@"Enabled")]
        [PropertyOrder(3)]
        public bool UseRamp
        {
            get => _info.UseRamp;
            set
            {
                _info.UseRamp = value;

                if (Motion != null)
                {
                    Motion.EnableAcceleration = value;
                }

                Environment.Properties.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the ramp value implemented in acceleration.
        /// </summary>
        [Display(Order = 1, GroupName = "Acceleration/Deceleration")]
        [DisplayName(@"Ramp Up")]
        [PropertyAttributesProvider("DynamicPropertyAcceleration")]
        [TypeConverter(typeof(MilliSeconds))]
        [PropertyOrder(4)]
        public float RampUp
        {
            get => _info.RampUp;
            set
            {
                if (RampDown.IsEffectivelyEqual(0f) && value.IsEffectivelyEqual(0f))
                {
                    UseRamp = false;
                    return;
                }

                if (value < 0)
                {
                    return;
                }

                _info.RampUp = value;
            }
        }

        /// <summary>
        /// Gets or sets the ramp value implemented in deceleration.
        /// </summary>
        [Display(Order = 1, GroupName = "Acceleration/Deceleration")]
        [DisplayName(@"Ramp Down")]
        [PropertyAttributesProvider("DynamicPropertyAcceleration")]
        [TypeConverter(typeof(MilliSeconds))]
        [PropertyOrder(5)]
        public float RampDown
        {
            get => _info.RampDown;
            set
            {
                if (RampUp.IsEffectivelyEqual(0f) && value.IsEffectivelyEqual(0f))
                {
                    UseRamp = false;
                    return;
                }

                if (value < 0)
                {
                    return;
                }

                _info.RampDown = value;
            }
        }

        /// <summary>
        /// Enables/Disables the use of Ready PLC Input signal.
        /// </summary>
        [Category("PLC Input")]
        [DisplayName(@"Mechanical Switch")]
        [PropertyOrder(1)]
        public bool MechanicalSwitchEnabled
        {
            get => _info.MechanicalSwitch.Enabled;
            set
            {
                _info.MechanicalSwitch.Enabled = value;

                Environment.Properties.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the Ready PLC Input signal.
        /// </summary>
        [Category("PLC Input")]
        [DisplayName(@"Ready")]
        [PropertyAttributesProvider("DynamicPropertyReady")]
        [PropertyOrder(2)]
        public Output OutputReady
        {
            get => _info.MechanicalSwitch.State;
            set => _info.MechanicalSwitch.State = value;
        }

        /// <summary>
        /// Gets or sets the Running PLC Input signal.
        /// This instance indicates if the <c>CurrentSpeed</c> of the motor is different from zero.
        /// </summary>
        [Category("PLC Input")]
        [DisplayName(@"Running")]
        [PropertyOrder(3)]
        public Output OutputRunning
        {
            get => _info.OutputRunning;
            set => _info.OutputRunning = value;
        }

        /// <summary>
        /// Gets or sets the Forward PLC Input signal.
        /// This instance move the motor in forward direction when its value is true.
        /// </summary>
        [Category("PLC Output")]
        [DisplayName(@"Forward")]
        [PropertyOrder(1)]
        public Input InputForward
        {
            get => _info.InputForward;
            set => _info.InputForward = value;
        }

        /// <summary>
        /// Gets or sets the Backward PLC Input signal.
        /// This instance move the motor in backward direction when its value is true.
        /// </summary>
        [Category("PLC Output")]
        [DisplayName(@"Backward")]
        [PropertyOrder(2)]
        public Input InputBackward
        {
            get => _info.InputBackward;
            set => _info.InputBackward = value;
        }

        /// <summary>
        /// Gets or sets the Alternative Speed PLC Input signal.
        /// This instance sets the Alternative Speed as the new Target Speed.
        /// </summary>
        [Category("PLC Output")]
        [DisplayName(@"Alternative Speed")]
        [PropertyOrder(3)]
        public Input InputAlternativeSpeed
        {
            get => _info.InputAlternativeSpeed;
            set => _info.InputAlternativeSpeed = value;
        }

        /// <summary>
        /// Gets or sets Encoder.
        /// </summary>
        [Category("Encoder")]
        [DisplayName(@"Properties")]
        public virtual Encoder Encoder => _info.Encoder;

        /// <summary>
        /// Gets or sets Current Speed.
        /// </summary>
        [Browsable(false)]
        public override float CurrentSpeed
        {
            get => base.CurrentSpeed;
            protected set
            {
                base.CurrentSpeed = value;

                if (!value.IsEffectivelyZero())
                {
                    LastSpeed = value;
                }
            }
        }

        /// <summary>
        /// Indicates if the motor is running.
        /// </summary>
        public override bool Running => !CurrentSpeed.IsEffectivelyZero();

        /// <summary>
        /// Gets the direction in which the motor is running.
        /// </summary>
        [Browsable(false)]
        public override MotorDirection Direction =>
            Command == AuxiliaryData.Commands.Forward || Command == AuxiliaryData.Commands.Stop
                ? MotorDirection.Forward
                : MotorDirection.Backward;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the Target Speed the motor must reach.
        /// </summary>
        protected internal float TargetSpeed { get; internal set; }

        /// <summary>
        /// Gets or sets the Last Target Speed.
        /// </summary>
        protected internal float LastSpeed { get; internal set; }

        /// <summary>
        /// Instance <c>Move</c> allows the motion of the motor and notifies the Belt when it is about to start or stop.
        /// </summary>
        protected internal bool Move
        {
            get => _move;
            internal set
            {
                _move = value;

                if (CurrentSpeed != 0f)
                    return;

                if (value)
                {
                    InvokeBeginStart();
                    InvokeStarted();
                }
                else
                    InvokeStopped();
            }
        }

        /// <summary>
        /// Instance <c>Command</c> indicates the command to execute.
        /// </summary>
        protected internal AuxiliaryData.Commands Command
        {
            get => _command;
            private set
            {
                _command = value;

                if (Running)
                    Controller();
            }
        }

        /// <summary>
        /// Instance <c>Motion</c> takes care of the calculations related to the motion of the motor taking into account acceleration and deceleration.
        /// </summary>
        protected readonly Motion Motion;

        #endregion

        #region Public Methods

        public override void Step(float deltatime)
        {
            if (Encoder.Enabled && CurrentSpeed != 0)
            {
                Encoder.Step(deltatime, CurrentSpeed);
            }

            if (!Move || !_info.MechanicalSwitch.State.Active)
            {
                return;
            }

            CurrentSpeed = Motion.Step(deltatime);

            if (CurrentSpeed.IsEffectivelyEqual(TargetSpeed))
            {
                Move = false;
                TargetSpeedReached?.Invoke(this, CurrentSpeed);

                UpdateColor(TargetSpeed.IsEffectivelyZero() ? Colors.Red : Colors.Green);
            }
            else
            {
                UpdateColor(Colors.Orange);
            }

            if (CurrentSpeed >= 0f)
            {
                SetForward();
            }
            else
            {
                SetBackward();
            }

            MotorStatus();
        }

        public override void Start()
        {
            if (!_info.MechanicalSwitch.State.Active)
            {
                return;
            }

            // Started from context menu
            if (Command == AuxiliaryData.Commands.Stop)
            {
                Command = LastSpeed >= 0f ? AuxiliaryData.Commands.Forward : AuxiliaryData.Commands.Backward;
            }

            Controller();
        }

        public override void Forward()
        {
            Command = AuxiliaryData.Commands.Forward;
        }

        public override void Backward()
        {
            Command = AuxiliaryData.Commands.Backward;
        }

        public override void Stop()
        {
            Command = AuxiliaryData.Commands.Stop;
        }

        public override void StopBreak()
        {
            Move = false;

            TargetSpeed = 0f;
            CurrentSpeed = 0f;
            Motion.Reset();

            _command = AuxiliaryData.Commands.Forward;
            UpdateColor(Colors.Red);
            SetForward();
            MotorStatus();

            Environment.Invoke(InvokeStopped);
            Script.Logic.Motor.Stopped(this);
        }

        public override void SwitchDirection()
        {
            switch (Command)
            {
                case AuxiliaryData.Commands.Forward:
                    Backward();
                    break;
                case AuxiliaryData.Commands.Backward:
                    Forward();
                    break;
            }
        }

        public override void Reset()
        {
            StopBreak();

            Encoder?.Reset();
        }

        public override void Dispose()
        {
            _info.InputForward.On -= InputForwardOn;
            _info.InputForward.Off -= InputForwardOff;

            _info.InputBackward.On -= InputBackwardOn;
            _info.InputBackward.Off -= InputBackwardOff;

            _info.InputAlternativeSpeed.On -= InputAlternativeSpeedReceived;
            _info.InputAlternativeSpeed.Off -= InputAlternativeSpeedReceived;

            base.Dispose();
        }

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            var menu = new List<Environment.UI.Toolbar.BarItem>();
            if (_info.MechanicalSwitch.Enabled && _info.MechanicalSwitch.State.Active)
                menu.Add(new Environment.UI.Toolbar.Button("Off", Resources.GetIcon("MechanicalSwitch_Off"))
                {
                    OnClick = (sender, args) =>
                    {
                        _info.MechanicalSwitch.State.Off();
                        StopBreak();
                    }
                });
            else if (_info.MechanicalSwitch.Enabled && !_info.MechanicalSwitch.State.Active)
                menu.Add(new Environment.UI.Toolbar.Button("On", Resources.GetIcon("MechanicalSwitch_On"))
                {
                    OnClick = (sender, args) => _info.MechanicalSwitch.State.On()
                });

            return menu;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyReady(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = _info.MechanicalSwitch.Enabled;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyAcceleration(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = _info.UseRamp;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyMotorType(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = this is Surface;
        }

        #endregion

        #region Protected Methods

        /// < summary>
        /// This method defines the Target Speed based on the I/Os and activates the motion of the motor.
        /// </summary>
        protected virtual void Controller()
        {
            TargetSpeed = InputAlternativeSpeed.Active ? AlternativeSpeed : Speed;
            TargetSpeed *= (int)Command;

            Motion.SetTargetSpeed(TargetSpeed, RampUp / 1000f, RampDown / 1000f);
            Move = true;
        }

        /// < summary>
        /// This method takes care of feedback signals in relation to the status of the motor.
        /// </summary>
        protected virtual void MotorStatus()
        {
            switch (Running)
            {
                case true when !OutputRunning.Active:
                    OutputRunning.On();
                    break;
                case false when OutputRunning.Active:
                    OutputRunning.Off();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void SetPlcSignals()
        {
            if (_info.Encoder == null)
            {
                _info.Encoder = new Encoder { IncrementsPrDistance = 0.1f };
            }

            if (_info.Encoder.OutputPulse == null)
            {
                _info.Encoder.OutputPulse = new Output { DataSize = DataSize.DINT, Description = "Encoder Pule Signal", Symbol = "Encoder Pulse" };
            }

            if (_info.Encoder.InputReset == null)
            {
                _info.Encoder.InputReset = new Input { DataSize = DataSize.BOOL, Description = "Encoder Reset", Symbol = "Encoder Reset" };
            }

            if (_info.Encoder.InputStart == null)
            {
                _info.Encoder.InputStart = new Input { DataSize = DataSize.BOOL, Description = "Start/Stop Encoder", Symbol = "Start/Stop Encoder", ListSolutionExplorer = false };
            }

            Add(_info.Encoder.InputReset);
            Add(_info.Encoder.OutputPulse);
            Add(_info.Encoder.InputStart);

            _info.Encoder.InputReset.On += sender => _info.Encoder.Reset();

            if (_info.MechanicalSwitch == null)
            {
                _info.MechanicalSwitch = new MechanicalSwitch();
            }

            if (_info.MechanicalSwitch.State == null)
            {
                _info.MechanicalSwitch.State = new Output { DataSize = DataSize.BOOL, Description = "Ready Signal", Symbol = "Ready" };
            }

            if (_info.OutputRunning == null)
            {
                _info.OutputRunning = new Output { DataSize = DataSize.BOOL, Description = "Motor moving", Symbol = "Running" };
            }

            Add(_info.MechanicalSwitch.State);
            Add(_info.OutputRunning);

            if (_info.InputForward == null)
            {
                _info.InputForward = new Input { DataSize = DataSize.BOOL, Description = "Move Forward", Symbol = "Forward" };
            }

            if (_info.InputBackward == null)
            {
                _info.InputBackward = new Input { DataSize = DataSize.BOOL, Description = "Move Backward", Symbol = "Backward" };
            }

            if (_info.InputAlternativeSpeed == null)
            {
                _info.InputAlternativeSpeed = new Input { DataSize = DataSize.BOOL, Description = "Use Alternative Speed", Symbol = "Alternative Speed" };
            }

            Add(_info.InputForward);
            Add(_info.InputBackward);
            Add(_info.InputAlternativeSpeed);

            _info.InputForward.On += InputForwardOn;
            _info.InputForward.Off += InputForwardOff;

            _info.InputBackward.On += InputBackwardOn;
            _info.InputBackward.Off += InputBackwardOff;

            _info.InputAlternativeSpeed.On += InputAlternativeSpeedReceived;
            _info.InputAlternativeSpeed.Off += InputAlternativeSpeedReceived;
        }

        private void InputForwardOn(Input sender)
        {
            if (Command == AuxiliaryData.Commands.Forward && Move)
            {
                return;
            }

            Forward();

            if (!Running)
            {
                Start();
            }
        }

        private void InputForwardOff(Input sender)
        {
            if (!InputForward.Active && !InputBackward.Active)
            {
                Stop();
            }
        }

        private void InputBackwardOn(Input sender)
        {
            if (Command == AuxiliaryData.Commands.Backward && Move)
            {
                return;
            }

            Backward();

            if (!Running)
            {
                Start();
            }
        }

        private void InputBackwardOff(Input sender)
        {
            if (!InputForward.Active && !InputBackward.Active)
            {
                Stop();
            }
        }

        private void InputAlternativeSpeedReceived(Input sender)
        {
            if (InputForward.Active)
            {
                Forward();
            }
            else if (InputBackward.Active)
            {
                Backward();
            }
        }

        #endregion
    }

    [TypeConverter(typeof(BaseInfo))]
    [Serializable]
    [XmlType(TypeName = "Experior.Surface.Foundations.Motors.Basic.BaseInfo")]
    public class BaseInfo : ElectricInfo
    {
        public float BaseSpeed { get; set; } = 0.3f;

        public float AlternativeSpeed { get; set; } = 0.1f;

        public float RampUp { get; set; } = 300f;

        public float RampDown { get; set; } = 300f;

        public bool UseRamp { get; set; } = true;

        public MechanicalSwitch MechanicalSwitch { get; set; }

        public Encoder Encoder { get; set; }

        public Output OutputRunning { get; set; }

        public Input InputForward { get; set; }

        public Input InputBackward { get; set; }

        public Input InputAlternativeSpeed { get; set; }
    }
}
