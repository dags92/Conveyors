using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Conveyor.Foundations.Parts.Supports;
using Experior.Core.Mathematics;
using Experior.Core.Properties;
using Experior.Interfaces;

namespace Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors
{
    public class Chain : BasicChain
    {
        #region Fields

        private readonly ChainConveyorInfo _info;

        #endregion

        #region Constructor

        public Chain(ChainConveyorInfo info) : base(info)
        {
            _info = info;
        }

        #endregion

        #region Public Properties

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
        public override float Yaw
        {
            get => base.Yaw;
            set
            {
                if (base.Yaw.IsEffectivelyEqual(value))
                    return;

                base.Yaw = value;
                UpdateSupports();
            }
        }

        [Browsable(true)]
        public override float Roll
        {
            get => base.Roll;
            set
            {
                if (value < -(float)Math.PI / 4 || value > (float)Math.PI / 4)
                {
                    Log.Write("Roll value must be in the range of: -45° ≤ X ≤ 45°", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (base.Roll.IsEffectivelyEqual(value))
                    return;

                base.Roll = value;
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

        public override string Category => "Conveyors";

        public override ImageSource Image => Common.Icon.Get("RollerConveyor");

        #endregion

        #region Protected Properties

        protected Standard SupportStart { get; private set; }

        protected Standard SupportEnd { get; private set; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
            {
                return;
            }

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
            SupportStart = new Standard(Colors.LightGray, 1, _info.width) { Embedded = true };
            SupportEnd = new Standard(Colors.LightGray, 1, _info.width) { Embedded = true };

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

                if ((UseSupport && (StartFixPoint.Position.Y <= 0.2f || EndFixPoint.Position.Y <= 0.2f) || (!(UseSupport && Visible))))
                {
                    SupportStart.Visible = SupportEnd.Visible = false;
                    return;
                }

                SupportStart.Visible = SupportEnd.Visible = true;

                SupportStart.Width = Width + SideGuideWidth - Standard.FootLength * 2;
                SupportEnd.Width = Width + SideGuideWidth - Standard.FootLength * 2;

                var startPos = Length < 0.12f ? Length / 2 : Standard.FootLength;
                var endPos = Length - startPos;
                var heightDiff = (float)Math.Sin(Roll) * startPos;

                var surfaceHeight = Surface.GetSurfaceHeight() / 2;
                SupportStart.Height = StartFixPoint.Position.Y + heightDiff - Section.Position.Y - surfaceHeight;
                SupportEnd.Height = EndFixPoint.Position.Y - heightDiff - Section.Position.Y - surfaceHeight;

                SupportEnd.LocalRoll = Trigonometry.Roll(StartFixPoint.Position, EndFixPoint.Position);
                SupportStart.LocalRoll = Trigonometry.Roll(StartFixPoint.Position, EndFixPoint.Position);

                var start = new Vector3(0, 0, startPos);
                start = Vector3.Transform(start, Matrix4x4.CreateFromYawPitchRoll(Yaw + (float)Math.PI / 2, -Roll, 0));
                start += Position;

                var end = new Vector3(0, 0, endPos);
                end = Vector3.Transform(end, Matrix4x4.CreateFromYawPitchRoll(Yaw + (float)Math.PI / 2, -Roll, 0));
                end += Position;

                var startLegGlobalPosition = new Vector3(start.X, start.Y - SupportStart.Height / 2 - surfaceHeight, start.Z);
                var endLegGlobalPosition = new Vector3(end.X, end.Y - SupportEnd.Height / 2 - surfaceHeight, end.Z);

                Matrix4x4.Invert(Matrix4x4.CreateFromYawPitchRoll(Yaw, Pitch, Roll), out var inverseWorld);
                SupportStart.LocalPosition = Vector3.Transform(startLegGlobalPosition - Position, inverseWorld);
                SupportEnd.LocalPosition = Vector3.Transform(endLegGlobalPosition - Position, inverseWorld);
            });
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(ChainConveyorInfo)), XmlType(TypeName = "Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors.ChainConveyorInfo")]
    public class ChainConveyorInfo : BasicChainConveyorInfo
    {
        public bool UseSupport { get; set; } = true;
    }
}
