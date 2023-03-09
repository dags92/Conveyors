using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Collections;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Interfaces;
using Environment = Experior.Core.Environment;

namespace Experior.Conveyor.Foundations.Assemblies
{
    /// <summary>
    /// Abstract Class <c>Foundation</c> provides the implementation of <c>FixPoints</c>, <c>PhotoEyeCollection</c> and <c>IElectricSurfaceMotor</c>.
    /// </summary>
    /// <remarks>
    /// Child classes must use either <c>IStraightPhotoEye</c> or <c>ICurvePhotoEye</c>..
    /// </remarks>
    public abstract class Foundation : CustomAssembly
    {
        #region Fields

        private readonly FoundationInfo _info;

        private IElectricSurfaceMotor _motor;
        private Arrow _arrow;

        #endregion

        #region Constructor

        protected Foundation(FoundationInfo info) : base(info)
        {
            _info = info;

            if (_info.Friction == null)
            {
                _info.Friction = new Friction() { Coefficient = Coefficients.Sticky };
            }

            if (_info.PhotoEyes == null)
            {
                _info.PhotoEyes = new PhotoEyeCollection();
            }

            _info.PhotoEyes.ItemAdded += PhotoEyeCollectionOnItemAdded;
            _info.PhotoEyes.ItemRemoved += PhotoEyeCollectionOnItemRemoved;

            StartFixPoint = new FixPoint(Colors.Red, FixPoint.Types.Start, FixPoint.Shapes.TriangularPrism, this);
            EndFixPoint = new FixPoint(Colors.Blue, FixPoint.Types.End, FixPoint.Shapes.TriangularPrism, this);

            Add(StartFixPoint);
            Add(EndFixPoint);

            StartFixPoint.OnBeforeSnapping += StartFixPointOnBeforeSnapping;
            EndFixPoint.OnBeforeSnapping += EndFixPointOnBeforeSnapping;

            foreach (var sensorInfo in PhotoEyeCollection)
            {
                CreatePhotoEye(sensorInfo);
            }
        }

        #endregion

        #region Delegates

        public delegate void CreatingPhotoEyeEventHandler(PhotoEyeInfo info);

        public delegate void RemovingPhotoEyeEventHandler(PhotoEyeInfo info);

        #endregion

        #region Events

        public event CreatingPhotoEyeEventHandler CreatingPhotoEye;

        public event RemovingPhotoEyeEventHandler RemovingPhotoEye;

        public EventHandler DimensionChanged { get; set; }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public virtual IElectricSurfaceMotor Motor
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
                    _arrow = new Arrow(0.25f, 0.001f);
                    value.Add(_arrow);
                    Add(_arrow);

                    _arrow.OnSelected += sender => Experior.Core.Environment.Properties.Set(Motor);

                    Add(value);
                    UpdateArrows();
                }

                _motor = value;
            }
        }

        [Category("Surface")]
        [DisplayName("Friction")]
        [PropertyOrder(0)]
        public virtual Friction Friction
        {
            get => _info.Friction;
            set => _info.Friction = value;
        }

        [Category("Photo Eyes")]
        [DisplayName("Items")]
        [PropertyOrder(1)]
        public PhotoEyeCollection PhotoEyeCollection => _info.PhotoEyes;

        [Browsable(true)]
        public override bool Locked
        {
            get => base.Locked;
            set
            {
                base.Locked = value;
                UnLockArrows();
            }
        }

        #endregion

        #region Protected Properties

        protected FixPoint StartFixPoint { get; }

        protected FixPoint EndFixPoint { get; }

        protected Arrow Arrow => _arrow;

        #endregion

        #region Public Methods

        public override void InsertMotor(IElectricMotor motor)
        {
            if (Experior.Core.Environment.Scene.Loading)
            {
                return;
            }

            if (motor is IElectricSurfaceMotor surfaceMotor)
            {
                CreateSurfaceMotor(surfaceMotor);
            }
            else
            {
                base.InsertMotor(motor);
            }
        }

        public override void RemoveMotor(IElectricMotor motor)
        {
            if (motor == Motor)
            {
                RemoveSurfaceMotor();
            }

            base.RemoveMotor(motor);
        }

        public override void Refresh()
        {
            UpdateFixPoints();
            UpdatePhotoEyes();
            UpdateArrows();
        }

        public override void Reset()
        {
            Motor?.Reset();
        }

        public override void Dispose()
        {
            StartFixPoint.OnBeforeSnapping -= StartFixPointOnBeforeSnapping;
            EndFixPoint.OnBeforeSnapping -= EndFixPointOnBeforeSnapping;

            _info.PhotoEyes.ItemAdded -= PhotoEyeCollectionOnItemAdded;
            _info.PhotoEyes.ItemRemoved -= PhotoEyeCollectionOnItemRemoved;

            RemoveSurfaceMotor();

            base.Dispose();
        }

        public void UpdatePhotoEyes()
        {
            foreach (var photoEyeInfo in PhotoEyeCollection)
            {
                UpdatePhotoEye(photoEyeInfo.PhotoEye);
            }
        }

        public abstract void UpdatePhotoEye(ConveyorPhotoEye photoEye);

        #endregion

        #region Protected Methods

        protected virtual void CreateSurfaceMotor(IElectricSurfaceMotor motor)
        {
            if (motor == null || Motor == motor)
            {
                return;
            }

            if (Motor != null)
            {
                RemoveMotor(Motor);
            }

            Motor = motor;
        }

        protected virtual void RemoveSurfaceMotor()
        {
            if (Core.Environment.Scene.Loading || Environment.Scene.Disposing || Motor == null)
            {
                return;
            }

            // Remove from Global List if it is not linked to other conveyors
            if (Motor.Arrows.Count == 1)
            {
                Experior.Core.Motors.Motor.Items.Remove(Motor);
                Motor.Dispose();
            }

            Motor = null;
        }

        protected abstract void UpdateFixPoints();

        protected abstract void UpdateArrows();

        protected override void ExecuteSceneOnLoaded()
        {
            UnLockArrows();
        }

        #endregion

        #region Private Methods

        private void StartFixPointOnBeforeSnapping(FixPoint sender, FixPoint stranger, FixPoint.SnapEventArgs e)
        {
            if (stranger.Type == FixPoint.Types.End)
            {
                return;
            }

            e.Cancel = true;
            Log.Write($"Surface {Name} has been un-snapped. End Fix Point (Blue) type is expected.", System.Windows.Media.Colors.Orange, LogFilter.Information);
        }

        private void EndFixPointOnBeforeSnapping(FixPoint sender, FixPoint stranger, FixPoint.SnapEventArgs e)
        {
            if (stranger.Type == FixPoint.Types.Start)
            {
                return;
            }

            e.Cancel = true;
            Log.Write($"Surface {Name} has been un-snapped. Start Fix Point (Red) type is expected", System.Windows.Media.Colors.Orange, LogFilter.Information);
        }

        private void PhotoEyeCollectionOnItemAdded(object sender, EventListArgs<ConveyorPhotoEyeInfo> e)
        {
            var max = _info.PhotoEyes.Select(sensorInfo => sensorInfo.Distance).Prepend(0).Max();
            e.Item.Distance = max + 0.2f;
            e.Item.name = e.Item.GetNewName();

            CreatePhotoEye(e.Item);
        }

        private void PhotoEyeCollectionOnItemRemoved(object sender, EventListArgs<ConveyorPhotoEyeInfo> e)
        {
            Remove(e.Item.PhotoEye);
            e.Item.PhotoEye.Dispose();
        }

        private void CreatePhotoEye(ConveyorPhotoEyeInfo photoEyeInfo)
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                var photoEye = new ConveyorPhotoEye(photoEyeInfo, this);
                Add(photoEye);

                photoEyeInfo.PhotoEye = photoEye;
                UpdatePhotoEye(photoEye);
            });
        }

        private void UnLockArrows()
        {
            if (Parts == null)
            {
                return;
            }

            foreach (var arrow in Parts.Where(a => a is Arrow).Cast<Arrow>())
            {
                arrow.Locked = false;
            }
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(FoundationInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Assemblies.FoundationInfo")]
    public abstract class FoundationInfo : CustomAssemblyInfo
    {
        public Friction Friction { get; set; }

        public PhotoEyeCollection PhotoEyes { get; set; }
    }
}
