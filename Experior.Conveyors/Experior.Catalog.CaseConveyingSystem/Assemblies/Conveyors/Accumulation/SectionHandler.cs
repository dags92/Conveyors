using System;
using System.ComponentModel;
using System.Numerics;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Rendering.Interfaces;
using Math = System.Math;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation
{
    /// <summary>
    /// <c>SectionHandler</c> class takes care of creation and deletion of <c>ISection</c>.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public class SectionHandler
    {
        #region Fields

        private readonly SectionHandlerInfo _info;
        private readonly int _index;
        private SectionHandler _previous, _next;

        #endregion

        #region Constructor

        public SectionHandler(Accumulation parent, int index, SectionHandlerInfo info)
        {
            _info = info;
            _index = index;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            CreateSection();
        }

        #endregion

        #region Public Enum

        public enum SectionShape
        {
            Straight,
            Curve
        }

        #endregion

        #region Public Properties

        [Category("Section")]
        [DisplayName("Shape")]
        [PropertyOrder(1)]
        public SectionShape Shape
        {
            get => _info.Shape;
            set
            {
                if(_info.Shape == value)
                    return;

                _info.Shape = value;
                CreateSection();
                RequestParentRefresh();

                Experior.Core.Environment.Properties.Refresh();
            }
        }

        [Category("Section")]
        [DisplayName("Parameters")]
        [PropertyOrder(2)]
        public ISection Conveyor { get; private set; }

        [Browsable(false)]
        public Accumulation Parent { get; }

        #endregion

        #region Public Methods

        public void UpdateDimensions(float length, float width, float sensorDistance)
        {
            if(Conveyor == null)
                return;

            if(Parent.FixedLength)
                Conveyor.Length = length;

            Conveyor.Width = width;
            Conveyor.SensorDistance = sensorDistance;
        }

        public void UpdateVisualization()
        {
            if (Conveyor == null)
                return;

            Conveyor.SetFixPointVisualization(FixPoint.Types.Start, _previous == null);
            Conveyor.SetFixPointVisualization(FixPoint.Types.End, _next == null);
            Conveyor.Color = Parent.Color;
            Conveyor.SurfaceType = Parent.SurfaceType;
        }

        public void SetLocalPosition(SectionHandler previous, SectionHandler next)
        {
            if (Conveyor == null)
                return;

            _previous = previous;
            _next = next;

            var pos = Vector3.Zero;
            var yaw = 0f;

            if (_previous != null)
            {
                var pose = _previous.Conveyor.GetEndFixPointPose();
                Trigonometry.GlobalToLocal(Parent.Position, Parent.Orientation, pose.Item1, pose.Item2, out var tempPos, out var tempOri);

                pos = tempPos;
                yaw = Trigonometry.Yaw(tempOri) - (float)Math.PI;  // Note: Default Yaw of Fix Points is -X
            }

            Conveyor.LocalPosition = pos;
            Conveyor.LocalOrientation = Matrix4x4.CreateFromYawPitchRoll(yaw, 0, 0);
            UpdateVisualization();
        }

        public void Dispose()
        {
            RemoveSection();
        }

        public override string ToString()
        {
            return "Section";
        }

        #endregion

        #region Private Methods

        private void CreateSection()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                RemoveSection();

                if (Shape == SectionShape.Straight)
                    CreateStraightSection();
                else
                    CreateCurveSection();

                // Controller requires the index value to Sort the LinkedList !
                // Assigned to UserData to avoid the user modifying its value.
                Conveyor.Sensor.UserData = _index.ToString();
                Parent.Controller.Add(Conveyor.Sensor);
            });
        }

        private void RemoveSection()
        {
            if(!(Conveyor is Core.Assemblies.Assembly assembly))
                return;

            switch (assembly)
            {
                case StraightSection straight:

                    straight.DimensionChanged -= OnDimensionChanged;
                    _info.Straight = null;

                    break;

                case CurveSection curve:

                    curve.DimensionChanged -= OnDimensionChanged;
                    _info.Curve = null;

                    break;
            }

            Parent.Controller.Remove(Conveyor.Sensor);
            Parent.Remove(assembly);
            assembly.Dispose();
            Conveyor = null;
        }

        private void CreateStraightSection()
        {
            if (_info.Straight == null)
            {
                _info.Straight = new StraightSectionInfo()
                {
                    length = 1f,
                    width = 0.45f,
                    height = 1f,
                    SurfaceData = new StraightSurface()
                    {
                        Color = Parent.SurfaceType == AuxiliaryData.SurfaceType.Belt ? System.Windows.Media.Colors.Black : System.Windows.Media.Colors.Silver
                    },
                    Beam = new BeamInfo(){name = "Sensor " + _index}
                };
            }

            var straight = new StraightSection(this, _info.Straight);
            Parent.Add(straight);
            
            if(straight.Motor == null)
                straight.InsertMotor(Parent.Motor);

            straight.DimensionChanged += OnDimensionChanged;
            Conveyor = straight;
        }

        private void CreateCurveSection()
        {
            if (_info.Curve == null)
            {
                _info.Curve = new CurveSectionInfo()
                {
                    height = 1f,
                    GeometryData = new CurvedGeometry()
                    {
                        Radius = 0.6f,
                        Width = 0.45f,
                    },
                    SurfaceData = new CurvedSurface()
                    {
                        Color = Parent.SurfaceType == AuxiliaryData.SurfaceType.Belt ? System.Windows.Media.Colors.Black : System.Windows.Media.Colors.Silver
                    },
                    Beam = new BeamInfo() { name = "Sensor " + _index }
                };
            }

            var curve = new CurveSection(this, _info.Curve);
            Parent.Add(curve);

            if (curve.Motor == null)
                curve.InsertMotor(Parent.Motor);

            curve.DimensionChanged += OnDimensionChanged;
            Conveyor = curve;
        }

        private void OnDimensionChanged(object sender, EventArgs e)
        {
            RequestParentRefresh();
        }

        private void RequestParentRefresh()
        {
            Experior.Core.Environment.InvokeIfRequired(()=> Parent.Refresh());
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(SectionHandlerInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation.SectionHandlerInfo")]
    public class SectionHandlerInfo
    {
        public SectionHandler.SectionShape Shape;
        public StraightSectionInfo Straight;
        public CurveSectionInfo Curve;
    }
}
