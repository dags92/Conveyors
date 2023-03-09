using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Pneumatics;
using Experior.Core.Assemblies;
using Experior.Core.Mathematics;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Experior.Interfaces;
using Colors = System.Windows.Media.Colors;
using Environment = Experior.Core.Environment;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment
{
    public class Pusher : Experior.Core.Assemblies.Assembly
    {
        #region Fields

        private readonly PusherInfo _info;
        private readonly Linear _pneumatic;

        private readonly Box _head;
        private readonly Cylinder _piston;
        private readonly Model _back, _body, _front;

        #endregion

        #region Constructor

        public Pusher(PusherInfo info) : base(info)
        {
            _info = info;

            if (_info.Pneumatic == null)
            {
                _info.Pneumatic = new LinearInfo
                {
                    Axis = Base.Axes.X,
                    Speed = 0.6f,
                    Stroke = 0.25f,
                    Valve = Base.Valves.Monostable
                };
            }

            _pneumatic = new Linear(this, _info.Pneumatic);
            Add(_pneumatic);
            _pneumatic.StrokeChanged += OnStrokeChanged;

            _head = new Box(Color, PusherLength, PusherHeight, PusherWidth) { Rigid = true };
            Add(_head);

            _piston = new Cylinder(Colors.Silver, _info.Pneumatic.Stroke, 0.01f, 10);
            Add(_piston);

            _front = new Model(Common.Mesh.Get("FrontBase_Cylinder.stl"));
            Add(_front);

            _body = new Model(Common.Mesh.Get("Body_Cylinder.stl"));
            Add(_body);

            _back = new Model(Common.Mesh.Get("BackBase_Cylinder.stl"));
            Add(_back);

            _pneumatic.Add(_head);
            _pneumatic.Add(_piston);

            Refresh();
        }

        #endregion

        #region Public Properties

        [Category("Size")]
        [DisplayName("Length")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(0)]
        public float PusherLength
        {
            get => _info.PusherLength;
            set
            {
                if (value <= 0f)
                {
                    Log.Write("Pusher Length value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.PusherLength.IsEffectivelyEqual(value))
                    return;

                _info.PusherLength = value;
                InvokeRefresh();
            }
        }

        [Category("Size")]
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public float PusherHeight
        {
            get => _info.PusherHeight;
            set
            {
                if (value <= 0f)
                {
                    Log.Write("Pusher Height value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.PusherHeight.IsEffectivelyEqual(value))
                    return;

                _info.PusherHeight = value;
                InvokeRefresh();
            }
        }

        [Category("Size")]
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public float PusherWidth
        {
            get => _info.PusherWidth;
            set
            {
                if (value <= 0f)
                {
                    Log.Write("Pusher Width value must be greater than 0 mm", Colors.Orange, LogFilter.Information);
                    return;
                }

                if (_info.PusherWidth.IsEffectivelyEqual(value))
                    return;

                _info.PusherWidth = value;
                InvokeRefresh();
            }
        }

        [Category("Pneumatic")]
        [DisplayName("Parameters")]
        [PropertyOrder(0)]
        public Linear Pneumatic => _pneumatic;

        [Browsable(true)]
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;

                if (_head != null)
                    _head.Color = value;
            }
        }

        public override string Category => "Equipment";

        public override ImageSource Image => Common.Icon.Get("Pusher");

        #endregion

        #region Protected Properties

        protected Box Head => _head;

        protected Cylinder Piston => _piston;

        protected Model Front => _front;

        protected Model Body => _body;

        protected Model Back => _back;

        #endregion

        #region Public Methods

        public override List<Environment.UI.Toolbar.BarItem> ShowContextMenu()
        {
            var menu = new List<Environment.UI.Toolbar.BarItem>
            {
                new Environment.UI.Toolbar.Button("Reset", Common.Icon.Get("Calibrate"))
                {
                    OnClick = (sender, args) => Reset()
                }
            };

            return menu;
        }

        public override void Step(float deltatime)
        {
            Pneumatic.Step(deltatime);
        }

        public override void Reset()
        {
            Pneumatic.Reset();
        }

        public override void Refresh()
        {
            if (_info == null)
                return;

            _body.Length = _info.Pneumatic.Stroke;
            _body.LocalPosition = new Vector3(-0.048f, 0, 0);

            _front.LocalPosition = new Vector3(0, 0, 0);
            _back.LocalPosition = new Vector3(-_body.Length + 0.0165f, 0, 0);

            _piston.LocalYaw = (float)Math.PI / 2;
            _piston.Length = _info.Pneumatic.Stroke * 1.05f;
            _piston.LocalPosition = new Vector3(-_piston.Length / 2 + 0.01f, 0, 0);

            _head.Length = PusherLength;
            _head.Width = PusherWidth;
            _head.Height = PusherHeight;
            _head.LocalPosition = _piston.LocalPosition + new Vector3(_piston.Length / 2 + _head.Length / 2, 0, 0);
        }

        public override void Dispose()
        {
            _pneumatic.StrokeChanged -= OnStrokeChanged;

            base.Dispose();
        }

        #endregion

        #region Private Methods

        private void OnStrokeChanged(object sender, EventArgs e)
        {
            InvokeRefresh();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(PusherInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment.PusherInfo")]
    public class PusherInfo : Experior.Core.Assemblies.AssemblyInfo
    {
        public LinearInfo Pneumatic { get; set; }

        public float PusherLength { get; set; } = 0.01f;

        public float PusherWidth { get; set; } = 0.25f;

        public float PusherHeight { get; set; } = 0.06f;
    }
}
