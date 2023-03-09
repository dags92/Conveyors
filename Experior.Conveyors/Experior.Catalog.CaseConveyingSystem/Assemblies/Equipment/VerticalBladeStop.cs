using System;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Pneumatics;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment
{
    public class VerticalBladeStop : Pusher
    {
        #region Fields

        private readonly VerticalBladeStopInfo _info;

        #endregion

        #region Constructor

        public VerticalBladeStop(VerticalBladeStopInfo info) : base(info)
        {
            _info = info;

            if (Pneumatic != null)
                Pneumatic.Axis = Base.Axes.Y;

            Refresh();
        }

        #endregion

        #region Public Properties

        public override ImageSource Image => Common.Icon.Get("VerticalBladeStop");

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if (_info == null)
                return;

            Body.Length = _info.Pneumatic.Stroke;
            Body.LocalRoll = -(float)Math.PI / 2;
            Body.LocalYaw = (float)Math.PI;
            Body.LocalPosition = new Vector3(0, -Body.Length - 0.045f, 0);

            Front.LocalRoll = (float)Math.PI / 2;
            Front.LocalPosition = new Vector3(0, 0, 0);

            Back.LocalRoll = (float)Math.PI / 2;
            Back.LocalPosition = new Vector3(0, -Body.Length + 0.0165f, 0);

            Piston.LocalPitch = (float)Math.PI / 2;
            Piston.Length = _info.Pneumatic.Stroke * 1.05f;
            Piston.LocalPosition = new Vector3(0, -Piston.Length / 2 + 0.01f, 0);

            Head.Length = PusherLength;
            Head.Width = PusherWidth;
            Head.Height = PusherHeight;
            Head.LocalPosition = Piston.LocalPosition + new Vector3(0, Piston.Length / 2 + Head.Height / 2, 0);
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(VerticalBladeStopInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment.VerticalBladeStopInfo")]
    public class VerticalBladeStopInfo : PusherInfo
    {

    }
}
