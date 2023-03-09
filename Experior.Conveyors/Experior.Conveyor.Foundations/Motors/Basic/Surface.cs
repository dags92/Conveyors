using System;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Motors.Interfaces;

namespace Experior.Conveyor.Foundations.Motors.Basic
{
    /// <summary>
    /// Class <c>Surface</c> designed to be used with conveyor belts.
    /// </summary>
    public class Surface : Base, IElectricSurfaceMotor
    {
        #region Fields

        private readonly SurfaceInfo _info;

        #endregion

        #region Constructor

        public Surface(SurfaceInfo info) : base(info)
        {
            _info = info;

            if (_info.name == string.Empty)
                _info.name = GetValidName("Basic Surface Motor");

            OnNameChanged += (sender, args) => info.name = Name;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a Surface motor. Name is automatically assigned.
        /// Note : The motor is added to the global list automatically.
        /// </summary>
        public static Surface Create()
        {
            return Create(string.Empty);
        }

        /// <summary>
        /// Create a Surface Motor.
        /// Note : The motor is added to the global list automatically.
        /// </summary>
        public static Surface Create(string motorName)
        {
            var oldMotor = Items.Get(motorName);
            if (oldMotor is Surface surface)
                return surface;

            var motorInfo = new SurfaceInfo { name = GetValidName("Basic Surface Motor") };

            if (motorName != string.Empty)
                motorInfo.name = motorName;

            var motor = new Surface(motorInfo);
            Items.Add(motor);
            return motor;
        }

        /// <summary>
        /// Create a Surface Motor.
        /// Note : The motor is added to the global list automatically.
        /// </summary>
        public static Surface Create(SurfaceInfo motorInfo)
        {
            var oldMotor = Items.Get(motorInfo.name);
            if (oldMotor is Surface surface)
                return surface;

            var motor = new Surface(motorInfo);

            if (NameUsed(motor.Name) || motor.Name == string.Empty)
                motor.Name = IncrementName(motor.Name);

            Items.Add(motor);
            return motor;
        }

        #endregion

        #region Public Properties

        public override ImageSource Image => Resources.GetIcon("SurfaceMotor_Icon");

        #endregion
    }

    [Serializable, XmlInclude(typeof(SurfaceInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Motors.Basic.Surface")]
    public class SurfaceInfo : BaseInfo
    {

    }
}
