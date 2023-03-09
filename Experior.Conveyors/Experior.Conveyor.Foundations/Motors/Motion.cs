using System;
using System.ComponentModel;
using Experior.Core.Mathematics;

namespace Experior.Conveyor.Foundations.Motors
{
    /// <summary>
    /// Class <c>Motion</c> performs the calculations related to the motion based on target speed, acceleration and deceleration parameters.
    /// </summary>
    public class Motion
    {
        #region Fields

        private float _currentSpeed, _targetSpeed, _rampTime, _setPoint, _slope, _deltaSpeed;
        private float _rampUp, _rampDown;
        private bool _switchDomain;

        #endregion

        #region Public Properties

        [Browsable(false)]
        public float CurrentSpeed
        {
            get => _currentSpeed;
            private set => _currentSpeed = value;
        }

        [Browsable(false)]
        public float RampTime
        {
            get => _rampTime;
            private set => _rampTime = value;
        }

        [Browsable(false)]
        public float Slope
        {
            get => _slope;
            private set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    _slope = 0f;
                else
                    _slope = value;
            }
        }

        [Browsable(false)]
        public bool EnableAcceleration { get; set; } = true;

        #endregion

        #region Protected Properties

        protected float SpeedTolerance => Math.Abs(_deltaSpeed);

        #endregion

        #region Public Methods

        /// <summary>
        /// This method steps the motor using <c>deltaTime</c> to calculate the new <c>CurrentSpeed</c>.
        /// </summary>
        public float Step(float deltaTime)
        {
            if (EnableAcceleration && _rampTime != 0f)
            {
                if (_switchDomain && (_currentSpeed + SpeedTolerance >= 0) && _currentSpeed - SpeedTolerance <= 0)
                {
                    SwitchRamp();
                }

                _deltaSpeed = _slope * deltaTime;
                CurrentSpeed += _deltaSpeed;

                if ((_currentSpeed + SpeedTolerance >= _targetSpeed) && (_currentSpeed - SpeedTolerance <= _targetSpeed))
                {
                    CurrentSpeed = _targetSpeed;
                }
            }
            else
            {
                CurrentSpeed = _targetSpeed;
            }

            return CurrentSpeed;
        }

        /// <summary>
        /// This method calculates the acceleration value to use in <c>Step</c>.
        /// </summary>
        public void SetTargetSpeed(float targetSpeed, float rampUp, float rampDown)
        {
            _targetSpeed = targetSpeed;
            _rampUp = rampUp;
            _rampDown = rampDown;

            _switchDomain = false;
            // Forward Domain :
            if (CurrentSpeed.IsEffectivelyZero() && _targetSpeed > 0f || CurrentSpeed > 0f && _targetSpeed >= 0f)
            {
                RampTime = _targetSpeed < CurrentSpeed ? -_rampDown : _rampUp;
            }
            // Backward Domain :
            else if (CurrentSpeed.IsEffectivelyZero() && _targetSpeed < 0f || CurrentSpeed < 0f && _targetSpeed <= 0f)
            {
                RampTime = _targetSpeed < CurrentSpeed ? -_rampUp : _rampDown;
            }
            // Change Direction :
            else if (CurrentSpeed > 0 && _targetSpeed < 0 || CurrentSpeed < 0 && _targetSpeed > 0)
            {
                _switchDomain = true;
                RampTime = _targetSpeed < CurrentSpeed ? -_rampDown : _rampDown;
            }

            _setPoint = _switchDomain ? 0f : _targetSpeed;
            Slope = GetSlope(_setPoint);
        }

        /// <summary>
        /// This method resets the <c>CurrentSpeed</c>.
        /// </summary>
        public void Reset()
        {
            CurrentSpeed = 0f;
            Slope = 0f;
            _targetSpeed = 0f;
        }

        public void Translate()
        {

        }

        #endregion

        #region Private Methods

        private float GetSlope(float target)
        {
            return Math.Abs(target - CurrentSpeed) / RampTime;
        }

        private void SwitchRamp()
        {
            _switchDomain = false;
            RampTime = _targetSpeed < CurrentSpeed ? -_rampUp : _rampUp;
            Slope = GetSlope(_targetSpeed);
        }

        #endregion
    }
}
