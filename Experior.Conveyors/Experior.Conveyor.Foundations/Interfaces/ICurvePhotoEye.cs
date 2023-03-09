using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Interfaces
{
    public interface ICurvePhotoEye
    {
        Revolution Revolution { get; }

        float Width { get; set; }

        float Radius { get; }

        float Angle { get; }

        float HeightDifference { get; } // Spiral

        void UpdatePhotoEye(ConveyorPhotoEye photoEye);
    }
}
