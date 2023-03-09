using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Experior.Rendering.Interfaces;
using static Experior.Conveyor.Foundations.Assemblies.AuxiliaryData;

namespace Experior.Conveyor.Foundations.Parts.Boundaries
{
    /// <summary>
    /// Class <c>SplittableStraight</c> provides the visualization of a straight conveyor side guide.
    /// Functionality for automatic splitting is provided when an intersection between two <c>Intersectable</c> types is detected.
    /// </summary>

    [TypeConverter(typeof(ObjectConverter))]

    public class SplittableStraight : Intersectable
    {
        #region Fields

        private float _length, _width, _height, _ramp;
        private Friction _friction;

        private readonly Box _sideGuide;
        private readonly List<Box> _splits = new List<Box>();
        private readonly float _splittedHeight = 0;

        #endregion

        #region Constructor

        public SplittableStraight(float length, Data.Boundaries info, SideGuidePositions side, Color color) 
            : base(new AssemblyInfo())
        {
            CollisionDistanceTolerance = 0.01f;

            _length = length;
            _width = info.SideGuideWidth;
            _height = info.SideGuideHeight;
            _friction = info.Friction;

            SideGuideInfo = info;
            Side = side;
            Color = color;

            _sideGuide = new Box(color, length, SideGuideInfo.SideGuideHeight, SideGuideInfo.SideGuideWidth, 0f, RampEnds.None)
            {
                Rigid = true
            };
            Add(_sideGuide);

            _sideGuide.RampType = Side == SideGuidePositions.Right ? RampTypes.Top : RampTypes.Bottom;
            _sideGuide.RampEnd = RampEnds.Both;

            Refresh();
        }

        #endregion

        #region Public Properties

        [DisplayName("Length")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(0)]
        public override float Length
        {
            get => _length;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Length value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_length.IsEffectivelyEqual(value))
                {
                    return;
                }

                _length = value;
                InvokeRefresh();
            }
        }

        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public override float Height
        {
            get => _height;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Height value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_height.IsEffectivelyEqual(value))
                {
                    return;
                }

                _height = value;
                InvokeRefresh();
            }
        }

        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public override float Width
        {
            get => _width;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Width value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_width.IsEffectivelyEqual(value))
                {
                    return;
                }

                _width = value;
                InvokeRefresh();
            }
        }

        [DisplayName("Ramp")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(3)]
        public virtual float Ramp
        {
            get => _ramp;
            set
            {
                if (value < 0)
                {
                    Log.Write("Ramp value must be greater or equal than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_ramp.IsEffectivelyEqual(value))
                {
                    return;
                }

                _ramp = value;
                InvokeRefresh();
            }
        }

        [DisplayName("Friction")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(4)]
        [Browsable(true)]
        public Friction Friction
        {
            get => _friction;
            set
            {
                _friction = value;

                if (_sideGuide != null)
                {
                    _sideGuide.Friction = value;
                }

                foreach (var box in _splits)
                {
                    box.Friction = value;
                }
            }
        }

        [Browsable(true)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                if (_sideGuide != null)
                {
                    _sideGuide.Color = value;
                }

                foreach (var box in _splits)
                {
                    box.Color = value;
                }
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                _sideGuide.Visible = value && _splits.Count == 0;

                foreach (var box in _splits)
                {
                    box.Visible = value;
                }
            }
        }

        #endregion

        #region Protected Properties

        protected Data.Boundaries SideGuideInfo { get; }

        protected SideGuidePositions Side { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            ConfigureMainBox(_sideGuide.Visible);

            foreach (var box in _splits)
            {
                box.Width = Height;
                box.Height = Width;
                box.Ramp = Ramp;
                box.Rigid = true;
                box.LocalPosition = new Vector3(box.LocalPosition.X, box.LocalPosition.Y * 0f + Height / 2, box.LocalPosition.Z);
            }
        }

        public static SplittableStraight Intersect(FixPoint fixPoint, out Vector3 intersection)
        {
            if (fixPoint.Attached != null)
            {
                intersection = Vector3.Zero;
                return null;
            }

            foreach (var temp in Intersectables)
            {
                temp.UnHighlight();
            }

            foreach (var temp in Intersectables.Where(temp => !temp.Parent.Equals(fixPoint.Parent)))
            {
                if (!(temp is SplittableStraight collection))
                {
                    continue;
                }

                var hit = collection.Itersection(fixPoint, out intersection);

                if (hit == PickResult.Empty)
                {
                    continue;
                }

                if (!(Trigonometry.Length(fixPoint.Position, intersection) < fixPoint.Width / 2))
                {
                    continue;
                }

                collection.Highlight();
                return collection;
            }

            intersection = Vector3.Zero;
            return null;
        }

        public override Intersection SegmentIntersect(Vector3 pos)
        {
            return !_sideGuide.Visible ? Intersection.Empty : _sideGuide.SegmentIntersect(pos, float.MaxValue);
        }

        public override void ClearIntersections()
        {
            base.ClearIntersections();

            if (_splits.Count == 0)
            {
                return;
            }

            foreach (var box in _splits)
            {
                Remove(box);
                box.Dispose();
            }

            _splits.Clear();
            ConfigureMainBox(true);
        }

        #endregion

        #region Protected Methods

        protected override void UpdateIntersections()
        {
            if (!SideGuideInfo.Splittable)
            {
                return;
            }

            foreach (var box in _splits)
            {
                Remove(box);
                box.Dispose();
            }

            _splits.Clear();

            if (Intersections.Count > 0)
            {
                foreach (var split in Parts)
                {
                    var box = AddSplit(split.x2, split.x1);

                    if (box != null)
                    {
                        box.LocalPosition = new Vector3(split.x1 + box.Length / 2, Height / 2, 0);
                    }
                }

                ConfigureMainBox(false);
            }
            else
            {
                ConfigureMainBox(true);
            }
        }

        #endregion

        #region Private Methods

        private Box AddSplit(float x1, float x2)
        {
            if (Height - _splittedHeight <= 0 || x1 - x2 <= 0)
            {
                return null;
            }

            Box box;

            if (Side == SideGuidePositions.Left)
            {
                box = (_sideGuide.Ramp) / (x1 - x2) > 1
                    ? new Box(Color, x1 - x2, Width, Height - _splittedHeight, 1f / 2f, RampSides.None)
                    : new Box(Color, x1 - x2, Width, Height - _splittedHeight, (_sideGuide.Ramp) / (x1 - x2),
                        RampSides.None);

                box.RampType = RampTypes.Bottom;
                box.RampEnd = RampEnds.Both;
            }
            else
            {
                box = (_sideGuide.Ramp) / (x1 - x2) > 1
                    ? new Box(Color, x1 - x2, Width, Height - _splittedHeight, 1f / 2f, RampSides.None)
                    : new Box(Color, x1 - x2, Width, Height - _splittedHeight, (_sideGuide.Ramp) / (x1 - x2),
                        RampSides.None);

                box.RampEnd = RampEnds.Both;
            }

            _splits.Add(box);
            Add(box);

            box.Rigid = true;
            box.Ramp = Ramp;
            box.Friction = Friction;
            box.LocalPitch = -(float)Math.PI / 2;

            if (Selected)
            {
                box.Select();
            }

            return box;
        }

        private void ConfigureMainBox(bool enable)
        {
            if (enable)
            {
                _sideGuide.Length = Length;
                _sideGuide.Width = Height;
                _sideGuide.Height = Width;
                _sideGuide.Ramp = Ramp;

                _sideGuide.Rigid = true;
                _sideGuide.Visible = true;

                _sideGuide.LocalPitch = -(float)Math.PI / 2;
                _sideGuide.LocalPosition = new Vector3(0, Height / 2, 0);
            }
            else
            {
                _sideGuide.Rigid = false;
                _sideGuide.Visible = false;
            }

            _sideGuide.UnHighlight();
        }

        #endregion
    }
}