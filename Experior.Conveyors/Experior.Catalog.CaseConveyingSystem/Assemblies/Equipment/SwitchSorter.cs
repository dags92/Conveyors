using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Parts.Surfaces.Straight;
using Experior.Core.Communication.PLC;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Environment = Experior.Core.Environment;
using Straight = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Straight;
using StraightInfo = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.StraightInfo;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment
{
    /// <summary>
    /// <c>SwitchSorter</c> class provides the implementation of a functional Switch Sorter component.
    /// </summary>
    public class SwitchSorter : Straight
    {
        #region Fields

        private readonly SwitchSorterInfo _info;

        private readonly Box _plate;
        private readonly FixPoint _rightFixPoint;
        private readonly FixPoint _leftFixPoint;

        #endregion

        #region Constructor

        public SwitchSorter(SwitchSorterInfo info) : base(info)
        {
            _info = info;

            SetPlcSignals();

            _plate = new Box(Colors.DimGray, 2f, 0.035f / 2, 0.65f) { Rigid = false };
            Add(_plate);

            _rightFixPoint = new FixPoint(Colors.Blue, FixPoint.Types.Start, FixPoint.Shapes.TriangularPrism, this);
            _leftFixPoint = new FixPoint(Colors.Blue, FixPoint.Types.Start, FixPoint.Shapes.TriangularPrism, this);

            Add(_rightFixPoint);
            Add(_leftFixPoint);

            _rightFixPoint.OnSnapped += RightFpOnSnapped;
            _leftFixPoint.OnSnapped += LeftFpOnSnapped;
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AuxiliaryData.SurfaceType SurfaceType { get => base.SurfaceType; set => base.SurfaceType = value; }

        [Category("Surface")]
        [DisplayName(@"Rollers Per Line")]
        [PropertyOrder(5)]
        public int Rollers
        {
            get => _info.RollersPerRow;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Rollers value must be greater than 0", Colors.Orange, LogFilter.Information);
                    return;
                }

                if(_info.RollersPerRow == value)
                    return;
                
                _info.RollersPerRow = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName(@"Right Switch Angle")]
        [TypeConverter(typeof(Degrees))]
        [PropertyOrder(6)]
        public float RightAngle
        {
            get => _info.RightAngle;
            set
            {
                if (value <= 0 || value > 90)
                {
                    Log.Write("Right Switch Angle value must be in the range of: 0° < X ≤ 90°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if(_info.RightAngle.IsEffectivelyEqual(value))
                    return;

                _info.RightAngle = value;
                InvokeRefresh();
            }
        }

        [Category("Surface")]
        [DisplayName(@"Left Switch Angle")]
        [TypeConverter(typeof(Degrees))]
        [PropertyOrder(7)]
        public float LeftAngle
        {
            get => _info.LeftAngle;
            set
            {
                if (value >= 0 || value < -90)
                {
                    Log.Write("Left Switch Angle value must be in the range of: -90° ≤ X < 0°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.LeftAngle.IsEffectivelyEqual(value))
                    return;

                _info.LeftAngle = value;
                InvokeRefresh();
            }
        }

        [Category("PLC Input")]
        [DisplayName(@"Right Position")]
        [PropertyOrder(1)]
        public Output OutputRightPosition
        {
            get => _info.OutputRightPosition;
            set => _info.OutputRightPosition = value;
        }

        [Category("PLC Input")]
        [DisplayName(@"Center Position")]
        [PropertyOrder(2)]
        public Output OutputCenterPosition
        {
            get => _info.OutputCenterPosition;
            set => _info.OutputCenterPosition = value;
        }

        [Category("PLC Input")]
        [DisplayName(@"Left Position")]
        [PropertyOrder(3)]
        public Output OutputLeftPosition
        {
            get => _info.OutputLeftPosition;
            set => _info.OutputLeftPosition = value;
        }

        [Category("PLC Output")]
        [DisplayName(@"Activate Right Diverting")]
        [PropertyOrder(1)]
        public Input InputDivertRight
        {
            get => _info.InputDivertRight;
            set => _info.InputDivertRight = value;
        }

        [Category("PLC Output")]
        [DisplayName(@"Activate Left Diverting")]
        [PropertyOrder(2)]
        public Input InputDivertLeft
        {
            get => _info.InputDivertLeft;
            set => _info.InputDivertLeft = value;
        }

        [Browsable(false)]
        public override float Roll { get => base.Roll; set => base.Roll = value; }

        [Browsable(false)]
        public override float Yaw { get => base.Yaw; set => base.Yaw = value; }

        public override string Category => "Equipment";

        public override ImageSource Image => Common.Icon.Get("SwitchSorter");

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            return Experior.Conveyor.Foundations.Utilities.ContextMenu.CreateSurfaceMotorMenu(this, Motor, true);
        }

        public override void Refresh()
        {
            if (_info == null)
                return;

            base.Refresh();

            _plate.Length = Length;
            _plate.Width = Belt.Width;
            _plate.Height = Surface.GetSurfaceHeight() / 2;
            _plate.LocalPosition = new Vector3(Length / 2, -Surface.GetSurfaceHeight() / 2, 0);

            _rightFixPoint.LocalPosition = new Vector3(Length / 2, 0, -Width / 2);
            _leftFixPoint.LocalPosition = new Vector3(Length / 2, 0, Width / 2);

            _rightFixPoint.LocalYaw = -(float)Math.PI / 2;
            _leftFixPoint.LocalYaw = (float)Math.PI / 2;

            //Collector?.UpdateBoundaries(); //TODO: NOT SURE ABOUT IT
        }

        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);

            if (e.Key == Key.A)
            {
                _rightFixPoint.Visible = !_rightFixPoint.Visible;
                _leftFixPoint.Visible = !_leftFixPoint.Visible;
            }
        }

        public override void Reset()
        {
            DeActivate();
            base.Reset();
        }

        public override void Activate()
        {
            var tempAngle = InputDivertRight.Active ? RightAngle : LeftAngle;

            if (Surface is Sorter sorter)
                sorter.TurnRollers(tempAngle);

            Belt.LocalSurfaceDirection = Trigonometry.DirectionYaw(Trigonometry.Angle2Rad(tempAngle));
            Arrow.LocalYaw = Trigonometry.Angle2Rad(tempAngle) + (float)Math.PI;

            OutputCenterPosition.Off();

            if (InputDivertRight.Active)
                OutputRightPosition.On();
            else if (InputDivertLeft.Active)
                OutputLeftPosition.On();
        }

        public override void DeActivate()
        {
            if (Surface is Sorter sorter)
                sorter.TurnRollers(0f);

            Belt.LocalSurfaceDirection = Trigonometry.DirectionYaw(Trigonometry.Angle2Rad(0));
            Arrow.LocalYaw = Trigonometry.Angle2Rad(0) + (float)Math.PI;

            OutputRightPosition.Off();
            OutputLeftPosition.Off();
            OutputCenterPosition.On();
        }

        public override void Dispose()
        {
            _info.InputDivertRight.On -= InputDivertRightOn;
            _info.InputDivertRight.Off -= InputDivertRightOff;

            _info.InputDivertLeft.On -= InputDivertLeftOn;
            _info.InputDivertLeft.Off -= InputDivertLeftOff;

            _rightFixPoint.OnSnapped -= RightFpOnSnapped;
            _leftFixPoint.OnSnapped -= LeftFpOnSnapped;

            base.Dispose();
        }

        #endregion

        #region Protected Methods

        protected override void CreateSurface()
        {
            Surface = new Sorter(this, _info.GeometryData, _info.SurfaceData);
        }

        protected override void UpdateSurface()
        {
            if (Surface is Sorter tempSurface)
            {
                tempSurface.Rollers = Rollers;
            }

            base.UpdateSurface();
        }

        #endregion

        #region Private Methods

        private void SetPlcSignals()
        {
            if (_info.OutputRightPosition == null)
            {
                _info.OutputRightPosition = new Output { DataSize = DataSize.BOOL, Symbol = "Right Position" };
            }

            if (_info.OutputLeftPosition == null)
            {
                _info.OutputLeftPosition = new Output { DataSize = DataSize.BOOL, Symbol = "Left Position" };
            }

            if (_info.OutputCenterPosition == null)
            {
                _info.OutputCenterPosition = new Output { DataSize = DataSize.BOOL, Symbol = "Center Position" };
            }

            Add(_info.OutputRightPosition);
            Add(_info.OutputLeftPosition);
            Add(_info.OutputCenterPosition);

            if (_info.InputDivertRight == null)
            {
                _info.InputDivertRight = new Input { DataSize = DataSize.BOOL, Symbol = "Divert Right" };
            }

            if (_info.InputDivertLeft == null)
            {
                _info.InputDivertLeft = new Input { DataSize = DataSize.BOOL, Symbol = "Divert Left" };
            }

            Add(_info.InputDivertRight);
            Add(_info.InputDivertLeft);

            _info.InputDivertRight.On += InputDivertRightOn;
            _info.InputDivertRight.Off += InputDivertRightOff;

            _info.InputDivertLeft.On += InputDivertLeftOn;
            _info.InputDivertLeft.Off += InputDivertLeftOff;

            OutputCenterPosition.On();
        }

        private void RightFpOnSnapped(FixPoint stranger, FixPoint.SnapEventArgs e)
        {
            if (stranger.Type == FixPoint.Types.Start)
                return;

            UnSnap(_rightFixPoint);
            Log.Write("Only Start Fix Point type is admissible", Colors.Orange, LogFilter.Information);
        }

        private void LeftFpOnSnapped(FixPoint stranger, FixPoint.SnapEventArgs e)
        {
            if (stranger.Type == FixPoint.Types.Start)
                return;

            UnSnap(_leftFixPoint);
            Log.Write("Only Start Fix Point type is admissible", Colors.Orange, LogFilter.Information);
        }

        private void InputDivertRightOn(Input sender)
        {
            if (InputDivertLeft.Active)
                return;

            Activate();
        }

        private void InputDivertRightOff(Input sender)
        {
            if (InputDivertLeft.Active)
                return;

            DeActivate();
        }

        private void InputDivertLeftOn(Input sender)
        {
            if (InputDivertRight.Active)
                return;

            Activate();
        }

        private void InputDivertLeftOff(Input sender)
        {
            if (InputDivertRight.Active)
                return;

            DeActivate();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(SwitchSorterInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment.SwitchSorterInfo")]
    public class SwitchSorterInfo : StraightInfo
    {
        public float RightAngle { get; set; } = 45f;

        public float LeftAngle { get; set; } = -45f;

        public int RollersPerRow { get; set; } = 8;

        public Input InputDivertRight { get; set; }

        public Input InputDivertLeft { get; set; }

        public Output OutputRightPosition { get; set; }

        public Output OutputLeftPosition { get; set; }

        public Output OutputCenterPosition { get; set; }
    }
}
