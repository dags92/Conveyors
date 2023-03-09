using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Communication.PLC;
using Experior.Core.Loads;
using Experior.Core.Parts;
using Experior.Core.Properties;
using Experior.Interfaces;
using Mesh = Experior.Conveyor.Foundations.Utilities.Mesh;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment
{
    public class BarcodeReader : Beam
    {
        #region Fields

        private readonly BarcodeReaderInfo _info;

        private Model _case;
        private string _message;

        #endregion

        #region Constructor

        public BarcodeReader(BarcodeReaderInfo info) : base(info)
        {
            _info = info;

            if (_info.InputTrigger == null)
                _info.InputTrigger = new Input() { DataSize = DataSize.BOOL, Symbol = "Trigger" };

            if (_info.InputReset == null)
                _info.InputReset = new Input() { DataSize = DataSize.BOOL, Symbol = "Reset" };

            Add(_info.InputTrigger);
            Add(_info.InputReset);

            _info.InputTrigger.On += InputTriggerOn;
            _info.InputReset.On += InputResetOn;

            if (_info.OutputBarcode == null)
                _info.OutputBarcode = new Output { DataSize = DataSize.STRING, Symbol = "Barcode" };
            Add(_info.OutputBarcode);

            _case = new Model(Common.Mesh.Get("BarcodeReader.dae"));
            Add(_case);
            Mesh.ScaleCadFile(0.015f, _case);

            Entering += OnEntering;
        }

        #endregion

        #region Public Properties

        [Browsable(true)] 
        public override float Length { get => base.Length; set => base.Length = value; }

        [Category("Barcode")]
        [DisplayName("Length")]
        [Description("Max. number of digits/characters to read")]
        [PropertyOrder(1)]
        public int MessageLength
        {
            get => _info.MessageLength;
            set
            {
                if (value <= 0)
                {
                    Log.Write("Message Length value must be greater than 0", Colors.Orange, LogFilter.Information);
                    return;
                }

                _info.MessageLength = value;
                OutputBarcode.Length = value;
                
                Reset();
            }
        }

        [Category("PLC Input")]
        [DisplayName("Barcode")]
        [PropertyOrder(2)]
        public Output OutputBarcode
        {
            get => _info.OutputBarcode;
            set => _info.OutputBarcode = value;
        }

        [Category("PLC Output")]
        [DisplayName("Trigger")]
        [PropertyOrder(1)]
        public Input InputTrigger
        {
            get => _info.InputTrigger;
            set => _info.InputTrigger = value;
        }

        [Category("PLC Output")]
        [DisplayName("Reset")]
        [PropertyOrder(2)]
        public Input InputReset
        {
            get => _info.InputReset;
            set => _info.InputReset = value;
        }

        public override ImageSource Image => Common.Icon.Get("BarcodeReader");

        #endregion

        #region Public Methods

        public override void Refresh()
        {
            if(_info == null)
                return;

            base.Refresh();

            _case.LocalPosition = new Vector3(_case.Width * 0.25f, 0, -0.03f - Length / 2);
        }

        public override void Reset()
        {
            SendMessage(string.Empty);
            base.Reset();
        }

        public override void Dispose()
        {
            _info.InputTrigger.On -= InputTriggerOn;
            _info.InputReset.On -= InputResetOn;

            Entering -= OnEntering;

            base.Dispose();
        }

        #endregion

        #region Private Methods

        private void OnEntering(object sender, object trigger)
        {
            if (!(trigger is Load load))
                return;

            if (InputTrigger.Active && !InputReset.Active)
                SendMessage(load.Identification);
        }

        private void InputTriggerOn(Input sender)
        {
            // Reads only one load
            if(Sensor.Active && !InputReset.Active)
                SendMessage(Sensor.Loads[0].Identification);
        }

        private void InputResetOn(Input sender)
        {
            Reset();
        }

        private void SendMessage(string message)
        {
            _message = message.Length > MessageLength ? message.Substring(0, MessageLength) : message;
            OutputBarcode.Send(_message);
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(BarcodeReaderInfo)), XmlType(TypeName = "Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment.BarcodeReaderInfo")]
    public class BarcodeReaderInfo : BeamInfo
    {
        public int MessageLength { get; set; } = 12;

        public Input InputTrigger { get; set; }

        public Input InputReset { get; set; }

        public Output OutputBarcode { get; set; }
    }
}
