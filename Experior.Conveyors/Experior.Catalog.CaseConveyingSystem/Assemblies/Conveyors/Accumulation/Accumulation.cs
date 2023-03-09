using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Core.Communication.PLC;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation
{
    /// <summary>
    /// <c>Accumulation</c> class provides the implementation of a functional zero pressure accumulation conveyor.
    /// </summary>
    public class Accumulation : Experior.Core.Assemblies.Assembly
    {
        #region Fields

        private readonly AccumulationInfo _info;

        #endregion

        #region Constructor

        public Accumulation(AccumulationInfo info) : base(info)
        {
            _info = info;

            if (_info.InputRelease == null)
                _info.InputRelease = new Input() { DataSize = DataSize.BOOL, Symbol = "Release" };
            Add(_info.InputRelease);

            if (_info.Motor == null)
                _info.Motor = new SurfaceInfo();

            Motor = Surface.Create(_info.Motor);
            Add(Motor);

            if (_info.Sections == null)
                _info.Sections = new List<SectionHandlerInfo>();

            Controller = new Controller(this);

            Experior.Core.Environment.Scene.OnLoaded += SceneOnLoaded;

            CreateSections();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Surface Motor { get; }

        [Browsable(false)]
        public AuxiliaryData.SurfaceType SurfaceType
        {
            get => _info.SurfaceType;
            set
            {
                _info.SurfaceType = value;
                UpdateSectionVisualization();
            }
        }

        [Category("Sections")]
        [DisplayName("Quantity")]
        [PropertyOrder(1)]
        public int NumberOfSections
        {
            get => _info.NumberOfSections;
            set
            {
                if(_info.NumberOfSections == value)
                    return;

                _info.NumberOfSections = value;
                CreateSections();
            }
        }

        [Category("Sections")]
        [DisplayName("Sensor Distance")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public float SensorDistance
        {
            get => _info.SensorDistance;
            set
            {
                if (value < 0)
                {
                    Log.Write("Sensor Distance value must be greater or equal than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.SensorDistance.IsEffectivelyEqual(value))
                    return;

                _info.SensorDistance = value;
                UpdateSections();
            }
        }

        [Category("Sections")]
        [DisplayName("Fixed Length")]
        [Description("Length value will be used to determine the length of each section")]
        [PropertyOrder(3)]
        public bool FixedLength
        {
            get => _info.FixedLength;
            set
            {
                if(_info.FixedLength == value)
                    return;

                _info.FixedLength = value;
                UpdateSections();

                Experior.Core.Environment.Properties.Refresh();
            }
        }

        [Category("Sections")]
        [DisplayName("Length")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyAttributesProvider("DynamicPropertyLength")]
        [PropertyOrder(4)]
        public float Length
        {
            get => _info.length;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Length value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.length.IsEffectivelyEqual(value))
                    return;

                _info.length = value;
                UpdateSections();
            }
        }

        [Category("Sections")]
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(5)]
        public float Width
        {
            get => _info.width;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Width value must be greater than 0 mm", System.Windows.Media.Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.width.IsEffectivelyEqual(value))
                    return;

                _info.width = value;
                UpdateSections();
            }
        }

        [Category("Sections")]
        [DisplayName("Items")]
        [PropertyOrder(6)]
        public List<SectionHandler> Sections { get; } = new List<SectionHandler>();

        [Category("PLC Output")]
        [DisplayName("Release")]
        [PropertyOrder(1)]
        public Input InputRelease
        {
            get => _info.InputRelease;
            set => _info.InputRelease = value;
        }

        [Browsable(false)]
        public Controller Controller { get; }

        [Browsable(true)] 
        public override float Yaw { get => base.Yaw; set => base.Yaw = value; }

        [Browsable(true)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;
                UpdateSectionVisualization();
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                if(value)
                    Sections.ForEach(a => a.UpdateVisualization());
            }
        }

        public override string Category => "Conveyors";

        public override ImageSource Image => SurfaceType == AuxiliaryData.SurfaceType.Belt ? Common.Icon.Get("AccumulationBelt") : Common.Icon.Get("AccumulationRoller");

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if(_info == null)
                return;

            UpdateSections();
        }

        public override void Dispose()
        {
            Controller.Dispose();

            foreach (var section in Sections)
            {
                section.Dispose();
            }

            base.Dispose();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyLength(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = FixedLength;
        }

        #endregion

        #region Private Methods

        private void CreateSections()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (Sections.Count < NumberOfSections)
                {
                    for (var i = Sections.Count; i < NumberOfSections; i++)
                    {
                        SectionHandlerInfo sectionInfo;

                        if (_info.Sections.Count - 1 >= i)
                        {
                            sectionInfo = _info.Sections[i];
                        }
                        else
                        {
                            sectionInfo = new SectionHandlerInfo();
                            _info.Sections.Add(sectionInfo);
                        }

                        var section = new SectionHandler(this, i, sectionInfo);
                        Sections.Add(section);
                    }
                }
                else if (Sections.Count > NumberOfSections)
                {
                    for (var i = Sections.Count - 1; i >= NumberOfSections; i--)
                    {
                        var section = Sections[i];

                        section.Dispose();
                        Sections.Remove(section);
                    }
                }

                UpdateSectionName();
                UpdateSections();
            });
        }

        private void UpdateSectionName()
        {
            var count = 0;
            foreach (var section in Sections)
            {
                section.Conveyor.Name = "Section " + count;
                count++;
            }

            if(Experior.Core.Environment.Scene.Paused)
                Experior.Core.Environment.SolutionExplorer.Refresh();
        }

        private void UpdateSections()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                // Tail is located at (0,0,0)
                for (var i = Sections.Count - 1; i >= 0; i--)
                {
                    var section = Sections[i];
                    var previous = i + 1 > Sections.Count - 1 ? null : Sections[i + 1];
                    var next = i - 1 < 0 ? null : Sections[i - 1];

                    var straights = Sections.Count(a => a.Shape == SectionHandler.SectionShape.Straight);
                    var sectionLength = Length / straights;

                    section.UpdateDimensions(sectionLength, Width, SensorDistance);
                    section.SetLocalPosition(previous, next);
                }
            });
        }

        private void UpdateSectionVisualization()
        {
            Sections.ForEach(a => a.UpdateVisualization());
        }

        private void SceneOnLoaded()
        {
            Experior.Core.Environment.Scene.OnLoaded -= SceneOnLoaded;
            UpdateSections();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(AccumulationInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation.AccumulationInfo")]
    public class AccumulationInfo : Experior.Core.Assemblies.AssemblyInfo
    {
        public int NumberOfSections = 2;
        public bool FixedLength = true;
        public List<SectionHandlerInfo> Sections;

        public float SensorDistance = 0.4f;
        public AuxiliaryData.SurfaceType SurfaceType;

        public SurfaceInfo Motor;
        public Input InputRelease;
    }
}
