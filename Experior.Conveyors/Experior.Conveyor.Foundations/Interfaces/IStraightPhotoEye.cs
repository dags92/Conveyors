using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Parts.Sensors;

namespace Experior.Conveyor.Foundations.Interfaces
{
    public interface IStraightPhotoEye
    {
        float Length { get; set; }

        float Width { get; set; }

        void UpdatePhotoEye(ConveyorPhotoEye photoEye);
    }
}
