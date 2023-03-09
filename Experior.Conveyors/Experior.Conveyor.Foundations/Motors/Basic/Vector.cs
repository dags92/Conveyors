using System;
using System.ComponentModel;
using System.Numerics;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Motors.Collections;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Core.Properties.TypeConverter;
using Experior.Core.Properties;
using Experior.Core.Communication.PLC;
using Experior.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Experior.Conveyor.Foundations.Motors.Basic
{
    public class Vector : Base, IElectricVectorMotor
    {
        #region Fields

        private readonly VectorInfo _info;

        private float _delta;

        #endregion

        #region Constructor

        public Vector(VectorInfo info) : base(info)
        {
            _info = info;

            if (_info.Limits == null)
            {
                _info.Limits = new VectorLimits();
            }
            _info.Limits.LimitChanged += OnLimitChanged;

            if (_info.OutputMaxLimit == null)
            {
                _info.OutputMaxLimit = new Output() {DataSize = DataSize.BOOL, Symbol = "Max. Limit"};
            }

            if (_info.OutputMidLimit == null)
            {
                _info.OutputMidLimit = new Output() { DataSize = DataSize.BOOL, Symbol = "Mid. Limit" };
            }

            if (_info.OutputMinLimit == null)
            {
                _info.OutputMinLimit = new Output() { DataSize = DataSize.BOOL, Symbol = "Min. Limit" };
            }

            Add(_info.OutputMaxLimit);
            Add(_info.OutputMidLimit);
            Add(_info.OutputMinLimit);

            Experior.Core.Environment.Scene.OnLoaded += SceneOnLoaded;
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public float DistanceTraveled
        {
            get => _info.DistanceTraveled;
            private set => _info.DistanceTraveled = value;
        }

        [Browsable(false)]
        public Vector3 TranslationDirection
        {
            get => _info.TranslationDirection;
            set
            {
                _info.TranslationDirection = value;
                Reset();
            }
        }

        [Category("Movement")]
        [DisplayName("Automatic Limit")]
        [PropertyOrder(0)]
        public AuxiliaryData.TranslationAutomaticLimits AutomaticLimitType
        {
            get => _info.AutomaticLimitType;
            set
            {
                if (_info.AutomaticLimitType == value)
                {
                    return;
                }

                _info.AutomaticLimitType = value;
                Reset();
            }
        }

        [Category("Movement")]
        [DisplayName("Tolerance")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public float Tolerance
        {
            get => _info. Limits. Tolerance;
            set => _info.Limits.Tolerance = value;
        }

        [Category("Movement")]
        [DisplayName("Max.")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public float MaxLimit
        {
            get => _info.Limits.Max;
            set => _info.Limits.Max = value;
        }

        [Category("Movement")]
        [DisplayName("Mid.")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(3)]
        public float MidLimit
        {
            get => _info.Limits.Mid;
            set => _info.Limits.Mid = value;
        }

        [Category("Movement")]
        [DisplayName("Min.")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(4)]
        public float MinLimit
        {
            get => _info.Limits.Min;
            set => _info.Limits.Min = value;
        }

        [Category("Movement")]
        [DisplayName("Reset Position")]
        [PropertyOrder(5)]
        public AuxiliaryData.TranslationPositions ResetPosition
        {
            get => _info.ResetPosition;
            set
            {
                _info.ResetPosition = value;
                Reset();
            }
        }

        [Category("PLC Input")]
        [DisplayName("Max. Limit")]
        [PropertyOrder(4)]
        public Output OutputMaxLimit
        {
            get => _info.OutputMaxLimit;
            set => _info.OutputMaxLimit = value;
        }

        [Category("PLC Input")]
        [DisplayName("Mid. Limit")]
        [PropertyOrder(5)]
        public Output OutputMidLimit
        {
            get => _info.OutputMidLimit;
            set => _info.OutputMidLimit = value;
        }

        [Category("PLC Input")]
        [DisplayName("Min. Limit")]
        [PropertyOrder(6)]
        public Output OutputMinLimit
        {
            get => _info.OutputMinLimit;
            set => _info.OutputMinLimit = value;
        }

        [Browsable(false)]
        public VectorPartCollection Parts { get; } = new VectorPartCollection();

        [Browsable(false)]
        public VectorAssemblyCollection Assemblies { get; } = new VectorAssemblyCollection();

        #endregion

        #region Public Methods

        public override void Step(float deltatime)
        {
            base.Step(deltatime);

            if (!Running)
            {
                return;
            }

            _delta = CurrentSpeed * deltatime;
            DistanceTraveled += _delta;

            Displace(_delta);
            LimitSignals();

            if (AutomaticLimitType == AuxiliaryData.TranslationAutomaticLimits.Stop)
            {
                StopLimitHandler();
            }
            else
            {
                
            }
        }

        public override void Reset()
        {
            base.Reset();

            DistanceTraveled = 0f;
            Calibrate();
        }

        public void Calibrate()
        {
            Calibrate(ResetPosition);
            LimitSignals();
        }

        public override void Dispose()
        {
            _info.Limits.LimitChanged -= OnLimitChanged;

            base.Dispose();
        }

        public static Vector Create()
        {
            return Create(string.Empty);
        }

        public static Vector Create(string motorName)
        {
            var oldMotor = Items.Get(motorName);
            if (oldMotor is Vector surface)
                return surface;

            var motorInfo = new VectorInfo { name = GetValidName("Basic Vector Motor") };

            if (motorName != string.Empty)
                motorInfo.name = motorName;

            var motor = new Vector(motorInfo);
            Items.Add(motor);
            return motor;
        }

        public static Vector Create(VectorInfo motorInfo)
        {
            if (motorInfo == null)
            {
                return Create();
            }

            var oldMotor = Items.Get(motorInfo.name);
            if (oldMotor is Vector surface)
                return surface;

            var motor = new Vector(motorInfo);

            if (NameUsed(motor.Name) || motor.Name == string.Empty)
                motor.Name = IncrementName(motor.Name);

            Items.Add(motor);
            return motor;
        }

        #endregion

        #region Protected Methods



        #endregion

        #region Private Methods

        private void Displace(float distance)
        {
            foreach (var part in Parts.Items)
            {
                part.LocalPosition += TranslationDirection * distance * Parts.Gears[part];
            }

            foreach (var assembly in Assemblies.Items)
            {
                assembly.LocalPosition += TranslationDirection * distance * Assemblies.Gears[assembly];
            }
        }

        private void LimitSignals()
        {
            if (DistanceTraveled >= MaxLimit - Tolerance && !OutputMaxLimit.Active)
            {
                OutputMaxLimit.On();
            }
            else if (DistanceTraveled < MaxLimit - Tolerance && OutputMaxLimit.Active)
            {
                OutputMaxLimit.Off();
            }

            if (DistanceTraveled >= MidLimit - Tolerance && DistanceTraveled <= MidLimit + Tolerance && !OutputMidLimit.Active)
            {
                OutputMidLimit.On();
            }
            else if ((DistanceTraveled < MidLimit - Tolerance || DistanceTraveled > MidLimit + Tolerance) && OutputMidLimit.Active)
            {
                OutputMidLimit.Off();
            }

            if (DistanceTraveled <= MinLimit + Tolerance && !OutputMinLimit.Active)
            {
                OutputMinLimit.On();
            }
            else if(DistanceTraveled > MinLimit + Tolerance && OutputMinLimit.Active)
            {
                OutputMinLimit.Off();
            }
        }

        private void Calibrate(AuxiliaryData.TranslationPositions position)
        {
            if (Running)
            {
                StopBreak();
            }

            var delta = DistanceTraveled;
            switch (position)
            {
                case AuxiliaryData.TranslationPositions.Up:
                    delta -= MaxLimit;
                    DistanceTraveled = MaxLimit;
                    break;

                case AuxiliaryData.TranslationPositions.Middle:
                    delta -= MidLimit;
                    DistanceTraveled = MidLimit;
                    break;

                default:
                    delta -= MinLimit;
                    DistanceTraveled = MinLimit;
                    break;
            }

            Displace(-delta);
        }

        private void StopLimitHandler()
        {
            if (DistanceTraveled >= MaxLimit)
            {
                Calibrate(AuxiliaryData.TranslationPositions.Up);
            }
            else if (DistanceTraveled <= MinLimit)
            {
                Calibrate(AuxiliaryData.TranslationPositions.Down);
            }
        }

        private void OnLimitChanged(object sender, EventArgs e)
        {
            Reset();
        }

        private void SceneOnLoaded()
        {
            Experior.Core.Environment.Scene.OnLoaded -= SceneOnLoaded;

            Displace(DistanceTraveled);
        }

        #endregion
    }

    [TypeConverter(typeof(VectorInfo))]
    [Serializable]
    [XmlType(TypeName = "Experior.Surface.Foundations.Motors.Basic.VectorInfo")]
    public class VectorInfo : BaseInfo
    {
        public float DistanceTraveled { get; set; }

        public Vector3 TranslationDirection { get; set; } = Vector3.UnitY;

        public AuxiliaryData.TranslationPositions ResetPosition { get; set; }

        public AuxiliaryData.TranslationAutomaticLimits AutomaticLimitType { get; set; } = AuxiliaryData.TranslationAutomaticLimits.Stop;

        public VectorLimits Limits { get; set; }

        public Output OutputMaxLimit { get; set; }

        public Output OutputMidLimit { get; set; }

        public Output OutputMinLimit { get; set; }
    }
}
