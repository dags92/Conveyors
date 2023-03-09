using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Parts.Sensors;
using Experior.Core.Parts;
using Experior.Core.Properties;

namespace Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation
{
    /// <summary>
    /// <c>ISection</c> defines the required class members to be handled by <c>SectionHandler</c>.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    public interface ISection
    {
        string Name { get; set; }

        Beam Sensor { get; }

        float SensorDistance { get; set; }

        float Length { get; set; }

        float Width { get; set; }

        Vector3 LocalPosition { get; set; }

        Matrix4x4 LocalOrientation { get; set; }

        AuxiliaryData.SurfaceType SurfaceType { get; set; }

        Color Color { get; set; }

        SectionHandler Handler { get; }

        void SetFixPointVisualization(FixPoint.Types fixPoint, bool value);

        (Vector3, Matrix4x4) GetEndFixPointPose();

    }
}
