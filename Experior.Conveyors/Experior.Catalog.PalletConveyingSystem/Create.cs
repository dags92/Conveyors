using System.Windows.Media;
using Experior.Catalog.PalletConveyingSystem.Assemblies.Conveyors;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Core.Assemblies;

namespace Experior.Catalog.PalletConveyingSystem
{
    internal class Common
    {
        public static Experior.Core.Resources.EmbeddedImageLoader Icon;
        public static Experior.Core.Resources.EmbeddedResourceLoader Mesh;
    }

    public class Create
    {
        #region Conveyors

        public static Assembly RollerConveyor(string title, string subtitle, object properties)
        {
            var info = new RollerInfo()
            {
                name = Assembly.GetValidName("Roller "),
                height = 0.6f,
                GeometryData = new BoxGeometry()
                {
                    Length = 1.5f,
                    Width = 0.9f
                },
                Boundaries = new Boundaries()
                {
                    UseRightBoundary = false,
                    UseLeftBoundary = false
                },
                SurfaceData = new StraightSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Roller(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly ChainConveyor(string title, string subtitle, object properties)
        {
            var info = new ChainConveyorInfo()
            {
                name = Assembly.GetValidName("Chain "),
                height = 0.6f,
                Boundaries = new Boundaries()
                {
                    UseRightBoundary = false,
                    UseLeftBoundary = false
                },
                GeometryData = new BoxGeometry()
                {
                    Length = 1.5f,
                    Width = 0.9f
                }
            };

            var assembly = new Chain(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        #endregion

        #region Equipment



        #endregion

        #region Utilities

        public static AuxiliaryData.SurfaceType GetSurfaceType(string tittle)
        {
            var type = tittle.ToLower();
            return type.Contains("roller") ? AuxiliaryData.SurfaceType.Roller : type.Contains("strap") ? AuxiliaryData.SurfaceType.Strap : AuxiliaryData.SurfaceType.Belt;
        }

        public static Color GetSurfaceColor(string tittle)
        {
            var type = tittle.ToLower();
            return type.Contains("roller") ? Colors.Silver : type.Contains("strap") ? Colors.DimGray : Colors.Black;
        }

        #endregion

    }
}