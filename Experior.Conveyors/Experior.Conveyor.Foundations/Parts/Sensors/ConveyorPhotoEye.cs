using System;
using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Interfaces;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Parts.Sensors
{
    /// <summary>
    /// Class <c>ConveyorPhotoEye</c> provides the visualization and functionality of a Photoelectric sensor.
    /// <c>ConveyorPhotoEye</c> has been designed to be used with Straight and Curve conveyors through the property <c>PhotoEyeCollection</c>.
    /// <c>Parent</c> MUST implement the interface <c>IStraightPhotoEye</c> or <c>ICurvePhotoEye</c>.
    /// </summary>
    public class ConveyorPhotoEye : PhotoEye
    {
        #region Fields

        private readonly ConveyorPhotoEyeInfo _info;

        #endregion

        #region Constructor

        public ConveyorPhotoEye(ConveyorPhotoEyeInfo info, Assembly parent) : base(info)
        {
            _info = info;
            Movable = false;
            Parent = parent;

            Refresh();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public override float Length
        {
            get => base.Length;
            set => base.Length = value;
        }

        [Category("Position")]
        [DisplayName("Distance")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public float Distance
        {
            get => _info.Distance;
            set
            {
                if (value < 0)
                {
                    Log.Write("Distance value must be greater or equal than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.Distance.IsEffectivelyEqual(value))
                    return;

                _info.Distance = value;

                if (Parent != null)
                {
                    Name = _info.GetNewName();
                    Experior.Core.Environment.SolutionExplorer.Refresh();
                }

                InvokeRefresh();
            }
        }

        [Category("Size")]
        [DisplayName("Automatic Height")]
        [Description("Height is automatically updated based on conveyor side guide height changes")]
        [PropertyOrder(3)]
        public bool AutomaticHeight
        {
            get => _info.AutomaticHeight;
            set
            {
                _info.AutomaticHeight = value;

                if (!value)
                {
                    _info.HouseHeight = Sensor.LocalPosition.Y - Case.Length / 2;
                }
                else
                {
                    switch (Parent)
                    {
                        case IStraightPhotoEye straight:
                            straight.UpdatePhotoEye(this);
                            break;
                        case ICurvePhotoEye curve:
                            curve.UpdatePhotoEye(this);
                            break;
                    }
                }

                Experior.Core.Environment.Properties.Refresh(this); //TEST IT
            }
        }

        [Category("Size")]
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyAttributesProvider("DynamicPropertyHeight")]
        [PropertyOrder(4)]
        public override float Height
        {
            get => base.Height;
            set => base.Height = value;
        }

        [Category("Orientation")]
        [DisplayName("Yaw")]
        [TypeConverter(typeof(RadiansToDegrees))]
        [PropertyAttributesProvider("DynamicPropertyOrientation")]
        [PropertyOrder(1)]
        public float BeamYaw
        {
            get => _info.BeamYaw;
            set
            {
                if (value < Trigonometry.Angle2Rad(-45f) || value > Trigonometry.Angle2Rad(45f))
                {
                    Log.Write("Beam Yaw value must be in the range -45° ≤ X ≤ 45°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.BeamYaw.IsEffectivelyEqual(value))
                    return;

                _info.BeamYaw = value;
                InvokeRefresh();
            }
        }

        [Category("Orientation")]
        [DisplayName("Pitch")]
        [TypeConverter(typeof(RadiansToDegrees))]
        [PropertyAttributesProvider("DynamicPropertyOrientation")]
        [PropertyOrder(2)]
        public float BeamPitch
        {
            get => _info.BeamPitch;
            set
            {
                if (value < Trigonometry.Angle2Rad(-45f) || value > Trigonometry.Angle2Rad(45f))
                {
                    Log.Write("Beam Pitch value must be in the range -45° ≤ X ≤ 45°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.BeamPitch.IsEffectivelyEqual(value))
                    return;

                _info.BeamPitch = value;
                InvokeRefresh();
            }
        }

        [Browsable(false)]
        [Category("Orientation")]
        [DisplayName("Revolution")]
        [PropertyOrder(3)]
        public Revolution Revolution
        {
            get => _info.Revolution;
            set
            {
                if(_info.Revolution == value)
                    return;

                _info.Revolution = value;
                InvokeRefresh();
            }
        }

        [PropertyAttributesProvider("DynamicPropertyAssembly")]
        public override string SectionName { get => base.SectionName; set => base.SectionName = value; }

        [PropertyAttributesProvider("DynamicPropertyAssembly")]
        public override string AssociatedControlPanelName { get => base.AssociatedControlPanelName; set => base.AssociatedControlPanelName = value; }

        [PropertyAttributesProvider("DynamicPropertyAssembly")]
        public override EventCollection Events { get => base.Events; set => base.Events = value; }

        [PropertyAttributesProvider("DynamicPropertyAssembly")]
        public override bool Enabled { get => base.Enabled; set => base.Enabled = value; }

        public override ImageSource Image => Resources.GetIcon("PhotoEye");

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null || Parent == null)
                return;

            base.Refresh();

            switch (Parent)
            {
                case IStraightPhotoEye straight:

                    var tempLength = (float)(Length / Math.Cos(BeamYaw));
                    tempLength = (float)(tempLength / Math.Cos(BeamPitch));

                    var tempHeight = (float)Math.Abs(tempLength / 2 * Math.Sin(BeamPitch));

                    Sensor.Width = tempLength;
                    Sensor.LocalPosition = new Vector3(Distance, tempHeight + Height + Case.Height / 2, 0);
                    Sensor.LocalYaw = BeamYaw;
                    Sensor.LocalPitch = BeamPitch;

                    // Yaw:
                    var yawSign = Math.Sign(BeamYaw);
                    var zOffset = (float)Math.Abs(tempLength / 2 * Math.Sin(BeamYaw));

                    Case.LocalPosition = new Vector3(Distance + (zOffset * -yawSign), Sensor.LocalPosition.Y - Case.Height / 3, -Length / 2);
                    Plate.LocalPosition = new Vector3(Distance + (zOffset * yawSign), Sensor.LocalPosition.Y, Length / 2 + Plate.Width / 2);

                    // Pitch:
                    var pitchSign = Math.Sign(BeamPitch);
                    var yOffset = (float)Math.Abs(tempLength / 2 * Math.Sin(BeamPitch));

                    Case.LocalPosition += new Vector3(0, (pitchSign > 0 ? yOffset : -yOffset), 0);
                    Plate.LocalPosition += new Vector3(0, (pitchSign > 0 ? -yOffset : yOffset), 0);

                    Case.LocalYaw = BeamYaw;
                    Case.LocalPitch = (float)Math.PI + BeamPitch;

                    Plate.LocalYaw = BeamYaw;
                    Plate.LocalPitch = BeamPitch;

                    break;

                case ICurvePhotoEye curve:

                    var sensorCenter = new Vector3(0, 0, curve.Radius);
                    var caseCenter = new Vector3(0, 0, curve.Radius - Length / 2);
                    var plateCenter = new Vector3(0, 0, curve.Radius + Length / 2 + Plate.Width / 2);

                    Sensor.Width = Length;
                    
                    var circumference = (float)(2 * Math.PI * curve.Radius);
                    var ratio = Distance / circumference;
                    var yaw = (float)(ratio * 2 * Math.PI);
                    var tempHeightDifference = Math.Abs((curve.HeightDifference * yaw) / curve.Angle) + Height;

                    var sign = Revolution == Revolution.Clockwise ? 1 : -1;
                    sensorCenter = Vector3.Transform(sensorCenter, Matrix4x4.CreateFromYawPitchRoll(yaw * sign, 0, 0));
                    caseCenter = Vector3.Transform(caseCenter, Matrix4x4.CreateFromYawPitchRoll(yaw * sign, 0, 0));
                    plateCenter = Vector3.Transform(plateCenter, Matrix4x4.CreateFromYawPitchRoll(yaw * sign, 0, 0));

                    Sensor.LocalPosition = sensorCenter + new Vector3(0, Case.Height / 2 + tempHeightDifference, -curve.Radius);
                    Case.LocalPosition = caseCenter + new Vector3(0, Sensor.LocalPosition.Y - Case.Height / 3, -curve.Radius);
                    Plate.LocalPosition = plateCenter + new Vector3(0, Sensor.LocalPosition.Y, -curve.Radius);

                    Sensor.LocalYaw = yaw * sign;

                    Case.LocalYaw = Sensor.LocalYaw;
                    Case.LocalPitch = (float)Math.PI;
                    Case.LocalRoll = -(curve.HeightDifference / curve.Angle) * 2f;
                    Case.LocalRoll *= Revolution == Revolution.Counterclockwise ? -1 : 1;

                    Plate.LocalYaw = Sensor.LocalYaw;

                    break;

                default:

                    Log.Write("Photo Eye Parent must implement IStraightPhotoEye or ICurvePhotoEye", Colors.Red, LogFilter.Error);
                    
                    break;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyAssembly(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = Parent == null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyOrientation(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = Parent is IStraightPhotoEye;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DynamicPropertyHeight(PropertyAttributes attributes)
        {
            attributes.IsBrowsable = !AutomaticHeight;
        }

        public override string ToString()
        {
            // To enhance visualization in PhotoEyeCollection
            return Parent != null ? string.Empty : Name;
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(ConveyorPhotoEyeInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Parts.Sensors.ConveyorPhotoEyeInfo")]
    public class ConveyorPhotoEyeInfo : PhotoEyeInfo, IComparer
    {
        #region Public Properties

        [Browsable(false)]
        public float Distance { get; set; }

        [Browsable(false)]
        public bool AutomaticHeight { get; set; } = true;

        [Browsable(false)]
        public float BeamYaw { get; set; }

        [Browsable(false)]
        public float BeamPitch { get; set; }

        [Browsable(false)]
        public Revolution Revolution { get; set; }

        [Browsable(true)]
        [XmlIgnore]
        public ConveyorPhotoEye PhotoEye { get; set; }

        [Browsable(false)] 
        public override float Yaw { get => base.Yaw; set => base.Yaw = value; }

        #endregion

        #region Public Methods

        public int Compare(object x, object y)
        {
            if (!(x is string s1))
            {
                return 0;
            }

            if (!(y is string s2))
            {
                return 0;
            }

            var len1 = s1.Length;
            var len2 = s2.Length;
            var marker1 = 0;
            var marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                var ch1 = s1[marker1];
                var ch2 = s2[marker2];

                // Some buffers we can build up characters in for each chunk.
                var space1 = new char[len1];
                var loc1 = 0;
                var space2 = new char[len2];
                var loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = s1[marker1];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = s2[marker2];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                var str1 = new string(space1);
                var str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    var thisNumericChunk = int.Parse(str1);
                    var thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }
            return len1 - len2;
        }

        public string GetNewName()
        {
            if (name == "ASSEMBLY" || name.Contains("Sensor at:"))
                return "Sensor at: " + Distance * 1000f + " mm";

            return name;
        }

        public override string ToString()
        {
            // To enhance visualization in PhotoEyeCollection
            return name;
        }

        #endregion
    }
}
