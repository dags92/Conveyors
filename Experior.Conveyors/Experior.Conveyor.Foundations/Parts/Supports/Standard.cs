using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Core.Assemblies;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;
using Mesh = Experior.Conveyor.Foundations.Utilities.Mesh;

namespace Experior.Conveyor.Foundations.Parts.Supports
{
    /// <summary>
    /// Class <c>Standard</c> provides the visualization of supports using <c>Model</c>
    /// </summary>
    public class Standard : Base
    {
        #region Fields

        private readonly Model _leg1;
        private readonly Model _leg2;
        private readonly Model _feet1;
        private readonly Model _feet2;
        private readonly Model _topConnector;
        private readonly Model _bottomConnector;

        private readonly float _legWidth;

        #endregion

        #region Constructor

        public Standard(Color color, float height, float width) : base(new AssemblyInfo())
        {
            ListSolutionExplorer = false;

            Info.height = height;
            Info.width = width;
            Info.color = color;

            _leg1 = new Model(Resources.GetMesh("Support.stl")) { Color = Colors.LightGray };
            _leg2 = new Model(Resources.GetMesh("Support.stl")) { Color = Colors.LightGray };

            _feet1 = new Model(Resources.GetMesh("Feet.stl")) { Color = Colors.LightGray };
            _feet2 = new Model(Resources.GetMesh("Feet.stl")) { Color = Colors.LightGray };

            _topConnector = new Model(Resources.GetMesh("Profile.stl")) { Color = Colors.LightGray };
            _bottomConnector = new Model(Resources.GetMesh("Profile.stl")) { Color = Colors.LightGray };

            Add(_leg1);
            Add(_leg2);

            Add(_feet1);
            Add(_feet2);

            Add(_topConnector);
            Add(_bottomConnector);

            Mesh.ScaleCadFile(0.001f, _leg1);
            Mesh.ScaleCadFile(0.001f, _leg2);
            Mesh.ScaleCadFile(0.002f, _feet1);
            Mesh.ScaleCadFile(0.002f, _feet2);

            Mesh.ScaleCadFile(0.001f, _topConnector);
            Mesh.ScaleCadFile(0.001f, _bottomConnector);

            _legWidth = _leg1.Width;
            InvokeRefresh();
        }

        #endregion

        #region Public Properties

        [Category("Size")]
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public override float Height
        {
            get => _leg1.Height;
            set
            {
                if (Disposed)
                {
                    return;
                }

                if (value <= 0f)
                {
                    value = 0.01f;
                }

                Info.height = value;
                _leg1.Height = value;
                _leg2.Height = value;

                _leg1.Height -= _feet1.Height;
                _leg2.Height -= _feet2.Height;

                InvokeRefresh();
            }
        }

        [Category("Size")]
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public override float Width
        {
            get => Info.width;
            set
            {
                if (Disposed)
                {
                    return;
                }

                if (value <= _leg1.Width * 2)
                {
                    return;
                }

                Info.width = value;

                _bottomConnector.Width = Info.width - _legWidth * 2;
                _topConnector.Width = Info.width - _legWidth * 2;

                InvokeRefresh();
            }
        }

        [Browsable(false)]
        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                if (Disposed)
                {
                    return;
                }

                base.Position = value;

                var h1 = Info.height;
                var h2 = Info.height;

                //leg1
                var result1 = Platform.Collide(new Vector3(_leg1.Position.X, Info.height, _leg1.Position.Z));
                if (result1 > 0) // Toppen af benet er hævet over platformen
                {
                    var y1 = result1;

                    if (y1 > 0) // Toppen af benet er hævet over platformen
                    {
                        h1 = y1; //the new height
                    }
                }

                if (h1 != _leg1.Height)
                {
                    _leg1.Height = h1; // Something is changed
                }

                //leg2
                var result2 = Platform.Collide(new Vector3(_leg2.Position.X, Info.height, _leg2.Position.Z));
                if (result2 > 0) // Toppen af benet er hævet over platformen
                {
                    var y2 = result2;

                    if (y2 > 0) // Toppen af benet er hævet over platformen
                    {
                        h2 = y2; //the new height
                    }
                }

                if (h2 != _leg2.Height) //Something is changed
                {
                    _leg2.Height = h2;
                }

                _leg1.Height -= _feet1.Height;
                _leg2.Height -= _feet2.Height;

                _bottomConnector.Visible = _leg2.Height == _leg1.Height && Visible;
                _topConnector.Visible = _bottomConnector.Visible;

                InvokeRefresh();
            }
        }

        [Browsable(false)]
        public static float FootLength { get; } = 0.05f;

        public override string Category => "Supports";

        public override ImageSource Image { get; }

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            _leg1.LocalPosition = new Vector3(0, 0, Width / 2 - _legWidth / 2);
            _leg2.LocalPosition = new Vector3(0, 0, -Width / 2 + _legWidth / 2);

            _feet1.LocalPosition = new Vector3(0, -_leg1.Height / 2 - _feet1.Height / 2, Width / 2 - _legWidth / 2);
            _feet2.LocalPosition = new Vector3(0, -_leg2.Height / 2 - _feet2.Height / 2, -Width / 2 + _legWidth / 2);

            _bottomConnector.Width = Info.width - _legWidth * 2;
            _topConnector.Width = _bottomConnector.Width;

            _bottomConnector.LocalPosition = new Vector3(0, -Height / 2 + _bottomConnector.Height, 0);
            _topConnector.LocalPosition = new Vector3(0, Height / 2 - _topConnector.Height / 2, 0);
        }

        #endregion
    }
}
