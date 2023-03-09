using System.Windows.Media;
using Experior.Rendering.Interfaces;

namespace Experior.Conveyor.Foundations.Interfaces
{
    public interface ICurveSurface
    {
        Data.CurvedSurface SurfaceInfo { get; }

        Data.CurvedGeometry GeometryInfo { get; }

        float GetSurfaceHeight();

        void Refresh();

        void UpdateColor();

        void Dispose();
    }
}
