using System.ComponentModel;
using System.Windows.Media;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Interfaces
{
    [TypeConverter(typeof(ObjectConverter))]
    public interface IStraightSurface
    {
        Data.BoxGeometry Geometry { get; }

        Data.StraightSurface SurfaceInfo { get; }

        float GetSurfaceHeight();

        void Refresh();

        void UpdateColor();

        void Dispose();
    }
}
