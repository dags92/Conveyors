using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Common;
using Experior.Core;
using Experior.Core.Assemblies;
using Experior.Core.Communication.PLC;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Colors = System.Windows.Media.Colors;

namespace Experior.Conveyor.Foundations.Pneumatics
{
    /// <summary>
    /// Abstract class <c>SplittableStraight</c> provides common behaviors and class members required to develop Pneumatic components.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public abstract class Base : IEntity
    {
        #region Fields

        private readonly BaseInfo _info;

        private States _state;
        private readonly Timer _injectTimer;
        private readonly Timer _ejectTimer;

        private readonly Assembly _parent;
        private bool _enabled = true;
        private bool _selected;
        private bool _solutionExplorer = true;

        #endregion

        #region Constructor

        protected Base(Assembly parent, BaseInfo info)
        {
            _info = info;

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _parent = parent;

            if (string.IsNullOrEmpty(_info.Name) || _info.Name == "ASSEMBLY")
            {
                _info.Name = Assembly.GetValidName("Pneumatics");
            }

            _injectTimer = new Timer(Stroke / Speed);
            _ejectTimer = new Timer(Stroke / Speed);

            _injectTimer.OnElapsed += InjectTimerOnElapsed;
            _ejectTimer.OnElapsed += EjectTimerOnElapsed;

            SetPlcSignals();
        }

        #endregion

        #region Events

        public event EventHandler<NameChangedEventArgs> OnNameChanged;

        public EventHandler StrokeChanged;

        #endregion

        #region Enums

        [XmlType(TypeName = "PneumaticValves")]
        public enum Valves
        {
            Monostable,
            Bistable
        }

        [XmlType(TypeName = "PneumaticStates")]
        public enum States
        {
            Injected,
            Ejected,
            Injecting,
            Ejecting
        }

        [XmlType(TypeName = "PneumaticMotionAxes")]
        public enum Axes
        {
            X,
            Y,
            Z
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the speed. 
        /// </summary>
        [Category("Motion")]
        [DisplayName("Speed")]
        [TypeConverter(typeof(MeterPerSeconds))]
        [PropertyOrder(0)]
        public float Speed
        {
            get => _info.Speed;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Speed value must be greater than 0 m/s", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Speed.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Speed = value;
                ResetIfRequired();
            }
        }

        /// <summary>
        /// Gets or sets the stroke. 
        /// </summary>
        [Category("Motion")]
        [DisplayName("Stroke")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public float Stroke
        {
            get => _info.Stroke;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Stroke value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Stroke.IsEffectivelyEqual(value))
                {
                    return;
                }

                _info.Stroke = value;  //TODO: CONSTRAINT IS MISSING
                ResetIfRequired();

                StrokeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the valve type. 
        /// </summary>
        [Category("Motion")]
        [DisplayName("Valve")]
        [PropertyOrder(2)]
        public Valves Valve
        {
            get => _info.Valve;
            set
            {
                if (_info.Valve == value)
                {
                    return;
                }

                _info.Valve = value;
                ResetIfRequired();
            }
        }

        /// <summary>
        /// Gets or sets the axis of motion. 
        /// </summary>
        [Browsable(false)]
        public Axes Axis
        {
            get => _info.Axis;
            set
            {
                if (_info.Axis == value)
                {
                    return;
                }

                _info.Axis = value;
                ResetIfRequired();
            }
        }

        /// <summary>
        /// Gets or sets the Injected PLC Input Signal. 
        /// </summary>
        [Category("PLC Input")]
        [DisplayName("Injected")]
        [PropertyOrder(3)]
        public Output OutputInjected
        {
            get => _info.OutputInjected;
            set => _info.OutputInjected = value;
        }

        /// <summary>
        /// Gets or sets the Ejected PLC Input Signal. 
        /// </summary>
        [Category("PLC Input")]
        [DisplayName("Ejected")]
        [PropertyOrder(4)]
        public Output OutputEjected
        {
            get => _info.OutputEjected;
            set => _info.OutputEjected = value;
        }

        /// <summary>
        /// Gets or sets the Inject PLC Output Signal. 
        /// </summary>
        [Category("PLC Output")]
        [DisplayName("Inject")]
        [PropertyOrder(5)]
        public Input InputInject
        {
            get => _info.InputInject;
            set => _info.InputInject = value;
        }

        /// <summary>
        /// Gets or sets the Eject PLC Output Signal. 
        /// </summary>
        [Category("PLC Output")]
        [DisplayName("Eject")]
        [PropertyOrder(6)]
        public Input InputEject
        {
            get => _info.InputEject;
            set => _info.InputEject = value;
        }

        /// <summary>
        /// Gets or sets the Name of this instance. 
        /// </summary>
        [Browsable(true)]
        [Category("Identification")]
        [RefreshProperties(RefreshProperties.All)]
        public string Name
        {
            get => _info.Name;
            set
            {
                if (string.IsNullOrEmpty(value) || _info.Name == value)
                {
                    return;
                }

                if (!Assembly.NameUsed(value))
                {
                    var oldName = _info.Name;
                    _info.Name = value;
                    OnNameChanged?.Invoke(this, new NameChangedEventArgs(oldName, value));

                    RefreshSolutionExplorer();
                }
                else
                {
                    Log.Write($"{value} name is already used", Colors.Red, LogFilter.Error);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Parent of this instance. 
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public IEntity Parent { get; set; }

        /// <summary>
        /// Enables or Disables the functionality of this instance. 
        /// </summary>
        [Browsable(false)]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;

                if (!value)
                {
                    Reset();
                }

                Enable(value);
                RefreshSolutionExplorer();
            }
        }

        /// <summary>
        /// Gets or sets the Entity Id of this instance. 
        /// </summary>
        [Browsable(false)]
        public virtual ulong EntityId { get; }

        /// <summary>
        /// Allows the visualization of this instance in the Solution Explorer. 
        /// </summary>
        [Browsable(false)]
        public bool ListSolutionExplorer
        {
            get => _solutionExplorer;
            set
            {
                _solutionExplorer = value;
                RefreshSolutionExplorer();
            }
        }

        /// <summary>
        /// Gets or Sets Selected to highlight Parent's Parts and Sub-assemblies.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Select(value);
            }
        }

        /// <summary>
        /// Gets the image to be displayed in the Solution Explorer of this instance. 
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public ImageSource Image => Enabled ? Resources.GetIcon("PneumaticEnabled") : Resources.GetIcon("PneumaticDisabled");

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the State of this instance. 
        /// </summary>
        [XmlIgnore]
        protected States State
        {
            get => _state;
            set
            {
                switch (value)
                {
                    case States.Injected:

                        if (OutputEjected.Active)
                        {
                            OutputEjected.Off();
                        }

                        OutputInjected.On();

                        break;

                    case States.Ejected:

                        if (OutputInjected.Active)
                        {
                            OutputInjected.Off();
                        }

                        OutputEjected.On();

                        break;

                    case States.Injecting:
                    case States.Ejecting:

                        OutputEjected.Off();
                        OutputInjected.Off();

                        break;
                }

                _state = value;
            }
        }

        /// <summary>
        /// Gets or sets the current Position of this instance. 
        /// </summary>
        [XmlIgnore]
        protected float Position { get; set; }

        /// <summary>
        /// Gets or sets the motion Direction of this instance. 
        /// </summary>
        [XmlIgnore]
        protected Vector3 Direction
        {
            get
            {
                switch (Axis)
                {
                    case Axes.Y:
                        return  Vector3.UnitY;

                    case Axes.Z:
                        return Vector3.UnitZ;

                    default:
                        return  Vector3.UnitX;
                }
            }
        }

        #endregion

        #region Public Methods

        /// < summary>
        /// Calls <see cref="ExecuteStep"/> to step the Parts and Assemblies based on <see cref="State"/>.
        /// This method is not call automatically by the Physics Engine.
        /// </summary>
        public virtual void Step(float deltaTime)
        {
            if (!Enabled)
            {
                return;
            }

            if (State == States.Ejected || State == States.Injected)
            {
                return;
            }

            ExecuteStep(deltaTime);
        }

        /// < summary>
        /// Resets the position and PLC signals if and only if <c>State</c> is not injected.
        /// </summary>
        public void ResetIfRequired()
        {
            if (_info.Speed.IsEffectivelyZero())
            {
                return;
            }

            if (Position.IsEffectivelyZero())
            {
                return;
            }

            Reset();
        }

        /// < summary>
        /// Resets the position despite the current <c>State</c>.
        /// </summary>
        public virtual void Reset()
        {
            ResetTimers();

            State = States.Injected;
            Move(-Position);
            Position = 0;
        }

        /// < summary>
        /// Injects this instance.
        /// </summary>
        public virtual void Inject()
        {
            if (!Enabled)
            {
                return;
            }

            if (State == States.Injected || State == States.Injecting)
            {
                return;
            }

            ResetTimers();
            State = States.Injecting;

            _injectTimer.Timeout = (Position) / Speed;
            _injectTimer.Start();
        }

        /// < summary>
        /// Ejects this instance.
        /// </summary>
        public virtual void Eject()
        {
            if (!Enabled)
            {
                return;
            }

            if (State == States.Ejected || State == States.Ejecting)
            {
                return;
            }

            ResetTimers();
            State = States.Ejecting;

            _ejectTimer.Timeout = (Stroke - Position) / Speed;
            _ejectTimer.Start();
        }

        /// < summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            InputInject.OnReceived -= InputInjectOnReceived;
            InputEject.OnReceived -= InputEjectOnReceived;

            _injectTimer.OnElapsed -= InjectTimerOnElapsed;
            _ejectTimer.OnElapsed -= EjectTimerOnElapsed;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyInject(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = Valve == Valves.Bistable;
        }

        #endregion

        #region Protected Methods

        protected abstract void ExecuteStep(float deltaTime);

        protected abstract void Move(float delta);

        protected abstract void Enable(bool value);

        protected abstract void DeSelectPartsAndAssemblies();

        protected void RefreshSolutionExplorer()
        {
            if (Core.Environment.Scene.Paused)
            {
                Core.Environment.SolutionExplorer.Refresh();
            }
        }

        #endregion

        #region Private Methods

        private void Select(bool value)
        {
            if (value)
            {
                if (_parent != null)
                {
                    return;
                }

                if (!SharedStates.SelectedAssemblies.Contains(_parent))
                {
                    SharedStates.SelectedAssemblies.Add(_parent);
                }
            }
            else
            {
                DeSelectPartsAndAssemblies();
            }
        }

        private void SetPlcSignals()
        {
            if (OutputInjected == null)
            {
                OutputInjected = new Output { DataSize = DataSize.BOOL, Symbol = "Injected", ListSolutionExplorer = false};
            }

            if (OutputEjected == null)
            {
                OutputEjected = new Output { DataSize = DataSize.BOOL, Symbol = "Ejected", ListSolutionExplorer = false };
            }

            if (InputInject == null)
            {
                InputInject = new Input { DataSize = DataSize.BOOL, Symbol = "Inject", ListSolutionExplorer = false };
            }

            if (InputEject == null)
            {
                InputEject = new Input { DataSize = DataSize.BOOL, Symbol = "Eject", ListSolutionExplorer = false };
            }

            _parent.Add(OutputInjected);
            _parent.Add(OutputEjected);
            _parent.Add(InputInject);
            _parent.Add(InputEject);

            InputInject.OnReceived += InputInjectOnReceived;
            InputEject.OnReceived += InputEjectOnReceived;

            OutputInjected.On();
        }

        private void ResetTimers()
        {
            _injectTimer?.Reset();
            _ejectTimer?.Reset();
        }

        private void InputInjectOnReceived(Input sender, object value)
        {
            if (!sender.Active || Valve == Valves.Monostable)
            {
                return;
            }

            Inject();
        }

        private void InputEjectOnReceived(Input sender, object value)
        {
            if (sender.Active)
            {
                if (State != States.Injected)
                {
                    return;
                }

                Eject();
            }
            else
            {
                if (Valve == Valves.Monostable)
                {
                    Inject();
                }
            }
        }

        private void InjectTimerOnElapsed(Timer sender)
        {
            State = States.Injected;
        }

        private void EjectTimerOnElapsed(Timer sender)
        {
            State = States.Ejected;
        }

        #endregion
    }

    [TypeConverter(typeof(ObjectConverter))]
    [Serializable]
    [XmlInclude(typeof(BaseInfo))]
    [XmlType(TypeName = "Experior.Surface.Foundations.Pneumatics.BaseInfo")] 
    public abstract class BaseInfo
    {
        public string Name { get; set; }

        public Base.Valves Valve { get; set; }

        public Base.Axes Axis { get; set; }

        public float Speed { get; set; } = 0.5f; // Units: m/s

        public float Stroke { get; set; } = 0.25f;

        public Output OutputInjected { get; set; }

        public Output OutputEjected { get; set; }

        public Input InputInject { get; set; }

        public Input InputEject { get; set; }
    }
}
