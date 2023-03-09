using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Experior.Core.Communication.PLC;
using Experior.Core.Properties;
using Experior.Interfaces;
using Experior.Interfaces.Communication;

namespace Experior.Conveyor.Foundations.Motors
{
    /// <summary>
    /// Class <c>Encoder</c> depicts the functionality of a real Encoder.
    /// </summary>
    [XmlInclude(typeof(Encoder))]
    [TypeConverter(typeof(ObjectConverter))]
    [Serializable]
    [XmlType(TypeName = "Experior.Surface.Foundations.Motors.Encoder")]
    public class Encoder
    {
        #region Fields

        private const float Interval = 0.0001f; //10 ms
        private bool _enabled;
        private Output _outputPulse;
        private float _pulsesMm = 1;
        private bool _running = true;

        #endregion

        #region Delegates

        public delegate void EncoderEvent(Encoder sender, int value);

        #endregion

        #region Events

        public event EncoderEvent OnValueChanged;

        #endregion

        #region Enums

        public enum EncoderDataSize
        {
            /// < summary>
            /// The byte
            /// </summary>
            BYTE = 1,

            /// < summary>
            /// Unsigned 16 bits
            /// </summary>
            [Description("Unsigned 16 bits")]
            WORD = 2,

            /// < summary>
            /// Unsigned 32 bits
            /// </summary>
            [Description("Unsigned 32 bits")]
            DWORD = 3,

            /// < summary>
            /// Signed 16 bits
            /// </summary>
            [Description("Signed 16 bits")]
            INT = 4,

            /// < summary>
            /// Signed 32 bits
            /// </summary>
            [Description("Signed 32 bits")]
            DINT = 5,
        }

        #endregion Enums

        #region Constructor

        public Encoder()
        {
            //Encoder
            if (OutputPulse == null)
                OutputPulse = new Output { Symbol = "Encoder Pulse Signal" };

            OutputPulse.Size = DataSize.DINT;

            if (InputReset == null)
                InputReset = new Input { Symbol = "Reset Encoder" };

            if (InputStart == null)
                InputStart = new Input { Symbol = "Start/Stop Encoder" };
        }

        #endregion

        #region Public Properties

        [XmlElement("deltatime"), Browsable(false)]
        public double DeltaTime { get; set; }

        [Browsable(false)]
        public double Distance
        {
            get => TotalDistance;
            set
            {
                TotalDistance = value;

                if (Enabled)
                {
                    if (_outputPulse != null)
                    {
                        switch (_outputPulse.Size)
                        {
                            case DataSize.WORD:
                                _outputPulse.Send((ushort)Value);
                                break;
                            case DataSize.DWORD:
                                _outputPulse.Send((uint)Value);
                                break;
                            case DataSize.INT:
                                _outputPulse.Send((short)Value);
                                break;
                            case DataSize.DINT:
                                _outputPulse.Send((int)Value);
                                break;
                            case DataSize.BYTE:
                                _outputPulse.Send((byte)Value);
                                break;
                            case DataSize.SINT:
                                _outputPulse.Send((sbyte)Value);
                                break;
                        }
                    }

                    if (OnValueChanged != null)
                        OnValueChanged(this, (int)Value);
                }
            }
        }

        [PropertyOrder(0)]
        [Category("Encoder")]
        [Description("Encoder")]
        [DisplayName(@"Enabled")]
        [RefreshProperties(RefreshProperties.All)]
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// < summary>
        /// Gets or sets the increments PRMM.
        /// </summary>
        /// <value>The increments PRMM.</value>
        [PropertyOrder(1)]
        [PropertyAttributesProvider("DynamicPropertyEncoderProperties")]
        [Category("Encoder")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("pulsesmm")]
        public float IncrementsPrDistance
        {
            get => _pulsesMm;
            set
            {
                if (value > 0)
                    _pulsesMm = value;
            }
        }

        [Category("Encoder")]
        [DisplayName("Data Type")]
        [TypeConverter(typeof(EncoderDataTypeConverterConverter))]
        [PropertyAttributesProvider("DynamicPropertyEncoderProperties")]
        [PropertyOrder(2)]
        public DataSize DataType
        {
            get => _outputPulse.DataSize;
            set
            {
                if (_outputPulse != null)
                    _outputPulse.DataSize = value;
            }
        }

        [Category("Encoder")]
        [DisplayName(@"Value (Input)")]
        [Description("Output for encoder (Signed 32 bits - DINT)")]
        [PropertyAttributesProvider("DynamicPropertyEncoderProperties")]
        [PropertyOrder(3)]
        [XmlElement("OutputPulse")]
        public Output OutputPulse
        {
            get => _outputPulse;
            set => _outputPulse = value;
        }

        [Category("Encoder")]
        [Description("Reset encoder")]
        [DisplayName(@"Reset (Output)")]
        [PropertyAttributesProvider("DynamicPropertyEncoderProperties")]
        [PropertyOrder(4)]
        [XmlElement("InputReset")]
        public Input InputReset { get; set; }

        [Category("Encoder")]
        [Description("Start/Stop Encoder")]
        [DisplayName(@"Start/Stop (Output)")]
        [PropertyAttributesProvider("DynamicPropertyEncoderProperties")]
        [NonPublicBrowseable]
        [PropertyOrder(5)]
        [XmlElement("InputStart")]
        public Input InputStart { get; set; }

        [XmlElement("Running"), Browsable(false)]
        public bool Running
        {
            get => _running;
            set => _running = value;
        }

        [XmlElement("TotalDistance"), Browsable(false)]
        public double TotalDistance { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public double Value
        {
            get
            {
                if (Experior.Core.Environment.MeasurementSystem == MeasurementSystems.Metric)
                    return TotalDistance * _pulsesMm;

                return TotalDistance * _pulsesMm * Core.Standard.INCHS / Core.Standard.MILLIMETER;
            }
        }

        #endregion

        #region Public Methods

        public Encoder Copy()
        {
            var encoder = new Encoder();

            if (InputReset != null)
                encoder.InputReset = InputReset.Copy();

            if (InputStart != null)
                encoder.InputStart = InputStart.Copy();

            if (_outputPulse != null)
                encoder.OutputPulse = _outputPulse.Copy();

            encoder.Enabled = Enabled;
            encoder.IncrementsPrDistance = IncrementsPrDistance;
            encoder.TotalDistance = TotalDistance;

            return encoder;
        }

        public virtual string Description()
        {
            return Experior.Core.Environment.MeasurementSystem == MeasurementSystems.Metric ? "Increments/mm" : "Increments/inchs";
        }

        public void Dispose()
        {
            if (InputReset != null)
                InputReset.On -= Reset;

            if (InputStart != null)
                InputStart.On -= StartOn;

            if (InputStart != null)
                InputStart.Off -= StartOff;

            if (_outputPulse != null)
                _outputPulse.Dispose();

            if (InputReset != null)
                InputReset.Dispose();

            if (InputStart != null)
                InputStart.Dispose();

            Running = false;
        }

        public void InitCommunication()
        {
            if (InputReset != null)
                InputReset.On += Reset;

            if (InputStart != null)
                InputStart.On += StartOn;

            if (InputStart != null)
                InputStart.Off += StartOff;
        }

        public void Reset()
        {
            TotalDistance = 0;

            if (Enabled)
            {
                if (_outputPulse.Size == DataSize.WORD)
                    _outputPulse.Send((ushort)0);
                else if (_outputPulse.Size == DataSize.DWORD)
                    _outputPulse.Send((uint)0);
                else if (_outputPulse.Size == DataSize.INT)
                    _outputPulse.Send((short)0);
                else if (_outputPulse.Size == DataSize.DINT)
                    _outputPulse.Send((int)0);
                else if (_outputPulse.Size == DataSize.BYTE)
                    _outputPulse.Send((byte)0);
                else if (_outputPulse.Size == DataSize.SINT)
                    _outputPulse.Send((sbyte)0);
            }

            DeltaTime = 0;
        }

        public void Step()
        {
            if (!Running)
                return;

            if (!Enabled)
                return;

            if (_outputPulse == null)
                return;

            switch (_outputPulse.Size)
            {
                case DataSize.WORD:
                    _outputPulse.Send((ushort)Value);
                    break;
                case DataSize.DWORD:
                    _outputPulse.Send((uint)Value);
                    break;
                case DataSize.INT:
                    _outputPulse.Send((short)Value);
                    break;
                case DataSize.DINT:
                    _outputPulse.Send((int)Value);
                    break;
                case DataSize.BYTE:
                    _outputPulse.Send((byte)Value);
                    break;
                case DataSize.SINT:
                    _outputPulse.Send((sbyte)Value);
                    break;
            }
        }

        public bool Step(double deltatime, float speed)
        {
            if (!Running)
                return false;

            if (!Enabled)
                return false;

            DeltaDistance(deltatime * speed * Core.Standard.MILLIMETER);

            DeltaTime += deltatime;

            if (!(DeltaTime > Interval))
                return false;

            if (_outputPulse != null)
            {
                switch (_outputPulse.Size)
                {
                    case DataSize.WORD:
                        _outputPulse.Send((ushort)Value);
                        break;
                    case DataSize.DWORD:
                        _outputPulse.Send((uint)Value);
                        break;
                    case DataSize.INT:
                        _outputPulse.Send((short)Value);
                        break;
                    case DataSize.DINT:
                        _outputPulse.Send((int)Value);
                        break;
                    case DataSize.BYTE:
                        _outputPulse.Send((byte)Value);
                        break;
                    case DataSize.SINT:
                        _outputPulse.Send((sbyte)Value);
                        break;
                }
            }

            if (OnValueChanged != null)
                OnValueChanged(this, (int)Value);

            DeltaTime = 0;

            return true;
        }

        public override string ToString()
        {
            return "Encoder";
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyEncoderProperties(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = Enabled && attributes.IsBrowsable;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyEncoderPulsesProperties(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = !Enabled && attributes.IsBrowsable;

            if (Experior.Core.Environment.MeasurementSystem == Experior.Interfaces.MeasurementSystems.Metric)
            {
                attributes.Description = "Increments signals (increments/mm)";
                attributes.DisplayName = "Increments/mm";
            }
            else
            {
                attributes.Description = "Increments signals (increments/inchs)";
                attributes.DisplayName = "Increments/inchs";
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyEncoderDataTypeProperties(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = !Enabled && attributes.IsBrowsable;

            if (OutputPulse.Connection == null) return;
            if (OutputPulse.Connection.State == State.Connected ||
                OutputPulse.Connection.State == State.Listening ||
                OutputPulse.Connection.State == State.Connecting)
                attributes.IsReadOnly = true;
            else
                attributes.IsReadOnly = false;
        }

        #endregion

        #region Internal Methods

        internal virtual void DeltaDistance(double distance)
        {
            switch (_outputPulse.Size)
            {
                case DataSize.DINT when ((TotalDistance + distance) * _pulsesMm) > int.MaxValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm) - int.MaxValue;

                        TotalDistance = -int.MaxValue / _pulsesMm + (int)(delta / _pulsesMm);
                        break;
                    }
                case DataSize.DINT when (TotalDistance + distance) * _pulsesMm < int.MinValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm) - int.MaxValue;

                        TotalDistance = int.MaxValue / _pulsesMm - (int)(delta / _pulsesMm);
                        break;
                    }
                case DataSize.DINT:
                    TotalDistance += distance;
                    break;
                case DataSize.INT when ((TotalDistance + distance) * _pulsesMm) > short.MaxValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm) - short.MaxValue;

                        TotalDistance = -short.MaxValue / _pulsesMm + (short)(delta / _pulsesMm);
                        break;
                    }
                case DataSize.INT when (TotalDistance + distance) * _pulsesMm < short.MinValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm) - short.MaxValue;

                        TotalDistance = short.MaxValue / _pulsesMm - (short)(delta / _pulsesMm);
                        break;
                    }
                case DataSize.INT:
                    TotalDistance += distance;
                    break;
                case DataSize.BYTE when ((TotalDistance + distance) * _pulsesMm) > byte.MaxValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm) - byte.MaxValue;

                        TotalDistance = delta / _pulsesMm;
                        break;
                    }
                case DataSize.BYTE when (TotalDistance + distance) * _pulsesMm < byte.MinValue:
                    {
                        var delta = ((TotalDistance + distance) * _pulsesMm);

                        TotalDistance = byte.MaxValue / _pulsesMm - delta / _pulsesMm;
                        break;
                    }
                case DataSize.BYTE:
                    TotalDistance += distance;
                    break;
                default:
                    TotalDistance += distance;
                    break;
            }
        }

        #endregion

        #region Private Methods


        private void Reset(Input sender)
        {
            Reset();
        }

        private void Start()
        {
            Running = true;
        }

        private void StartOff(Input sender)
        {
            Stop();
        }

        private void StartOn(Input sender)
        {
            Start();
        }

        private void Stop()
        {
            Running = false;
        }

        #endregion

        #region Nested Types

        private class EncoderDataTypeConverterConverter : StringConverter
        {
            #region Methods

            /// <summary>
            /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
            /// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.</returns>
            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new string[] { Register.DataSizeConverter.BYTE, Register.DataSizeConverter.WORD, Register.DataSizeConverter.DWORD, Register.DataSizeConverter.INT, Register.DataSizeConverter.DINT });
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

            #endregion Methods
        }

        #endregion
    }
}
