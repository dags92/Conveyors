using System;
using System.ComponentModel;
using Experior.Core.Collections;

namespace Experior.Conveyor.Foundations.Parts.Sensors
{
    [DisplayName(@"Photo Eyes")]
    public class PhotoEyeCollection : ObservableList<ConveyorPhotoEyeInfo>, ICloneable
    {
        public object Clone()
        {
            var newCollection = new PhotoEyeCollection();

            foreach (var item in this)
            {
                var newSensor = new ConveyorPhotoEyeInfo()
                {
                    Radius = item.Radius,
                    length = item.length,
                    Distance = item.Distance,
                    AutomaticHeight = item.AutomaticHeight,
                    HouseHeight = item.HouseHeight,
                    BeamYaw = item.BeamYaw,
                    BeamPitch = item.BeamPitch,
                };
                newSensor.name = newSensor.GetNewName();

                newCollection.Add(newSensor);
            }

            return newCollection;
        }
    }
}
