using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Conveyor.Foundations.Parts.Supports;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Rendering.Interfaces;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors
{
    public class Curve : BasicCurve
    {
        #region Fields

        private CurveInfo _info;

        #endregion

        #region Constructor

        public Curve(CurveInfo info) : base(info)
        {
            _info = info;
        }

        #endregion

        #region Public Properties

        [Description("The center point between the start and end fixpoint")]
        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                UpdateSupports();
            }
        }

        [Category("Visualization")]
        [DisplayName(@"Support")]
        [PropertyOrder(4)]
        public virtual bool UseSupport
        {
            get => _info.UseSupport;
            set
            {
                _info.UseSupport = value;
                UpdateSupports();
            }
        }

        [Browsable(true)]
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;

                UpdateSupports();
            }
        }

        public override string Category => "Conveyors";

        public override ImageSource Image => Revolution == Revolution.Clockwise ? Common.Icon.Get("CurveCWRoller") : Common.Icon.Get("CurveCCWRoller");

        #endregion

        #region Protected Properties

        protected Standard SupportStart { get; private set; }

        protected Standard SupportEnd { get; private set; }

        protected Standard SupportMiddle { get; private set; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
                return;

            base.Refresh();

            UpdateSupports();
        }

        public override bool Move(Vector3 delta)
        {
            if (delta.Y != 0)
            {
                var tempTolerance = delta.Y - Section.Position.Y;
                if (delta.Y < 0 && (StartFixPoint.Position.Y + tempTolerance <= 0.2f || EndFixPoint.Position.Y + tempTolerance <= 0.2f))
                    return false;
            }

            if (delta != Vector3.Zero)
            {
                var res = base.Move(delta);

                if (!res)
                    return res;
            }

            if (delta.Y != 0)
            {
                Info.height += delta.Y;

                if (UseSupport)
                    UpdateSupports();
            }

            return true;
        }

        #endregion

        #region Protected Methods

        protected override void BuildAssembly()
        {
            base.BuildAssembly();

            CreateSupports();
            Refresh();
        }

        protected virtual void CreateSupports()
        {
            SupportStart = new Standard(Colors.LightGray, 1, Width) { Embedded = true };
            SupportEnd = new Standard(Colors.LightGray, 1, Width) { Embedded = true };

            Add(SupportStart);
            Add(SupportEnd);
        }

        protected virtual void UpdateSupports()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (SupportStart == null || SupportEnd == null)
                {
                    return;
                }

                if ((UseSupport && Position.Y <= 0.2f) || (!(UseSupport && Visible)))
                {
                    SupportStart.Visible = SupportEnd.Visible = false;

                    if (SupportMiddle != null)
                        SupportMiddle.Visible = false;

                    return;
                }

                // Spiral:
                if (HeightDifference > 0f)
                {
                    SupportEnd.Visible = Trigonometry.Rad2Angle(Angle) < 300f;

                    if (SupportMiddle != null & Trigonometry.Rad2Angle(Angle) > 600f)
                        SupportMiddle.Visible = false;
                }
                // Curve:
                else
                {
                    if (!SupportEnd.Visible)
                        SupportEnd.Visible = true;

                    if (SupportMiddle != null)
                        SupportMiddle.Visible = true;
                }

                if (!SupportStart.Visible)
                    SupportStart.Visible = true;

                var heightOffset = HeightDifference > 0f ? 0.015f : 0f;
                var tempHeight = Surface.GetSurfaceHeight();
                SupportStart.Height = StartFixPoint.Position.Y - Section.Position.Y - (tempHeight) + heightOffset;
                SupportEnd.Height = EndFixPoint.Position.Y - Section.Position.Y - (tempHeight);

                SupportStart.Width = Width;
                SupportEnd.Width = Width;

                SupportStart.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, -tempHeight - SupportStart.Height / 2 + heightOffset, -Radius), Trigonometry.Angle2Rad(5f), Radius, Revolution);
                SupportEnd.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, -tempHeight - SupportEnd.Height / 2 + HeightDifference, -Radius), Angle - Trigonometry.Angle2Rad(5f), Radius, Revolution);
                SupportEnd.LocalYaw = Revolution == Revolution.Clockwise ? Angle : -Angle;

                if (Angle > (float)Math.PI / 2)
                {
                    if (SupportMiddle == null)
                    {
                        SupportMiddle = new Standard(Colors.LightGray, 1, Width) { Embedded = true };
                        Add(SupportMiddle);
                    }

                    SupportMiddle.Height = StartFixPoint.Position.Y - Section.Position.Y - (tempHeight) + (HeightDifference / 2);
                    SupportMiddle.Width = Width;
                    SupportMiddle.LocalPosition = Trigonometry.RotationPoint(new Vector3(0, -tempHeight - SupportMiddle.Height / 2 + HeightDifference / 2, -Radius), Angle / 2, Radius, Revolution);
                    SupportMiddle.LocalYaw = Revolution == Revolution.Clockwise ? Angle / 2 : -Angle / 2;
                }
                else if (Angle <= (float)Math.PI / 2 && SupportMiddle != null)
                {
                    Remove(SupportMiddle);
                    SupportMiddle.Dispose();
                    SupportMiddle = null;
                }
            });
        }

        #endregion
    }

    [TypeConverter(typeof(CurveInfo))]
    [Serializable]
    [XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.CurveInfo")]
    public class CurveInfo : BasicCurveInfo
    {
        public bool UseSupport { get; set; } = true;
    }
}
