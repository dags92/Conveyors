using System.Collections.Generic;
using System.Linq;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Communication.PLC;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation
{
    /// <summary>
    /// <c>Controller</c> class handles the zero pressure accumulation logic of <c>Accumulation</c>.
    /// </summary>
    public class Controller
    {
        #region Fields

        private readonly LinkedList<Beam> _sensors = new LinkedList<Beam>();
        private readonly Accumulation _accumulation;

        #endregion

        #region Constructor

        public Controller(Accumulation accumulation)
        {
            _accumulation = accumulation;

            _accumulation.InputRelease.On += InputReleaseOn;
        }

        #endregion

        #region Public Methods

        public void Add(Beam sensor)
        {
            if(_sensors.Contains(sensor))
                return;

            sensor.Entering += OnEntering;
            sensor.Leaving += OnLeaving;

            _sensors.AddLast(sensor);
            Sort();
        }

        public void Remove(Beam sensor)
        {
            if(sensor == null || !(_sensors.Contains(sensor)))
                return;

            sensor.Entering -= OnEntering;
            sensor.Leaving -= OnLeaving;

            _sensors.Remove(sensor);
            Sort();
        }

        public void Dispose()
        {
            _accumulation.InputRelease.On -= InputReleaseOn;
        }

        #endregion

        #region Private Methods

        private void Sort()
        {
            var list = _sensors.ToList();
            list.Sort(Comparison);

            _sensors.Clear();
            foreach (var temp in list)
                _sensors.AddLast(temp);
        }

        private int Comparison(Beam x, Beam y)
        {
            if (!int.TryParse(x.UserData as string, out var xIndex) || !int.TryParse(y.UserData as string, out var yIndex))
                return 0;
            
            return xIndex > yIndex ? 1 : xIndex < yIndex ? -1 : 0;
        }

        private void OnEntering(object sender, object trigger)
        {
            var sensor = _sensors.Find((Beam)sender);
            if (sensor == null)
                return;

            if (sensor.Previous == null) // Head
            {
                if (!_accumulation.InputRelease.Active)
                    sensor.Value.AttachLoad(sensor.Value.Loads[0]);
            }
            else // Middle and Tail
            {
                if (sensor.Previous.Value.Active)
                    sensor.Value.AttachLoad(sensor.Value.Loads[0]);
            }
        }

        private void OnLeaving(object sender, object trigger)
        {
            var sensor = _sensors.Find((Beam)sender);
            if (sensor == null)
                return;
            
            sensor.Value.UnAttachLoads();
            sensor?.Next?.Value.UnAttachLoads();
        }

        private void InputReleaseOn(Input sender)
        {
            _sensors.First.Value.UnAttachLoads();
        }

        #endregion
    }
}
