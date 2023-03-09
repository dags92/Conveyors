using System;
using System.Windows.Media;
using Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors;
using Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Accumulation;
using Experior.Catalog.CaseConveyingSystem.Assemblies.Equipment;
using Experior.Conveyor.Foundations.Assemblies;
using Experior.Conveyor.Foundations.Data;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Core.Assemblies;
using Experior.Rendering.Interfaces;
using Curve = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Curve;
using CurveInfo = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.CurveInfo;
using Straight = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.Straight;
using StraightInfo = Experior.Catalog.CaseConveyingSystem.Assemblies.Conveyors.StraightInfo;

namespace Experior.Catalog.CaseConveyingSystem
{
    internal class Common
    {
        public static Experior.Core.Resources.EmbeddedImageLoader Icon;
        public static Experior.Core.Resources.EmbeddedResourceLoader Mesh;
    }

    public class Create
    {
        #region Conveyors

        public static Assembly StraightConveyor(string title, string subtitle, object properties)
        {
            var info = new StraightInfo()
            {
                name = Assembly.GetValidName("Straight "),
                height = 1f,
                GeometryData = new BoxGeometry()
                {
                    Length = 1f,
                    Width = 0.45f
                },
                SurfaceData = new StraightSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Straight(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly MergeConveyor(string title, string subtitle, object properties)
        {
            var info = new MergeInfo()
            {
                name = Assembly.GetValidName("Merge "),
                height = 1f,
                Flow = Merge.FlowDirection.Infeed,
                GeometryData = new BoxGeometry()
                {
                    Length = 1f,
                    Width = 0.45f
                },
                SurfaceData = new StraightSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    NoseOverDirection = AuxiliaryData.NoseOverDirection.Left,
                    NoseAngle = (float)Math.PI / 4,
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Merge(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly DivertConveyor(string title, string subtitle, object properties)
        {
            var info = new MergeInfo()
            {
                name = Assembly.GetValidName("Divert "),
                height = 1f,
                Flow = Merge.FlowDirection.Outfeed,
                GeometryData = new BoxGeometry()
                {
                    Length = 1f,
                    Width = 0.45f
                },
                SurfaceData = new StraightSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    NoseOverDirection = AuxiliaryData.NoseOverDirection.Left,
                    NoseAngle = (float)Math.PI / 4,
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Merge(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly TransferConveyor(string title, string subtitle, object properties)
        {
            var info = new TransferInfo()
            {
                name = Assembly.GetValidName("Transfer "),
                height = 1f,
                GeometryData = new BoxGeometry()
                {
                    Length = 1f,
                    Width = 0.45f
                },
                SurfaceData = new StraightSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                },
            };

            var assembly = new Transfer(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly CurveCwConveyor(string title, string subtitle, object properties)
        {
            var info = new CurveInfo()
            {
                name = Assembly.GetValidName("Curve "),
                height = 1f,
                GeometryData = new CurvedGeometry()
                {
                    Radius = 0.6f,
                    Width = 0.45f,
                },
                SurfaceData = new CurvedSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Curve(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly CurveCcwConveyor(string title, string subtitle, object properties)
        {
            var info = new CurveInfo()
            {
                name = Assembly.GetValidName("Curve "),
                height = 1f,
                GeometryData = new CurvedGeometry()
                {
                    Radius = 0.6f,
                    Width = 0.45f,
                    Revolution = Revolution.Counterclockwise
                },
                SurfaceData = new CurvedSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Curve(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly SpiralConveyor(string title, string subtitle, object properties)
        {
            var info = new SpiralInfo()
            {
                name = Assembly.GetValidName("Spiral "),
                height = 1f,
                GeometryData = new CurvedGeometry()
                {
                    Radius = 0.6f,
                    Width = 0.45f,
                    Angle = (float)Math.PI,
                    HeightDifference = 0.4f,
                },
                SurfaceData = new CurvedSurface()
                {
                    SurfaceType = GetSurfaceType(title),
                    Color = GetSurfaceColor(title)
                }
            };

            var assembly = new Spiral(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly RollerAlignment(string title, string subtitle, object properties)
        {
            var info = new AlignmentInfo()
            {
                name = Assembly.GetValidName("Alignment "),
                height = 1f,
                GeometryData = new BoxGeometry()
                {
                    Length = 1f,
                    Width = 0.45f
                },
                SurfaceData = new StraightSurface()
                {
                    Color = Colors.Silver
                }
            };

            var assembly = new Alignment(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly AccumulationConveyor(string title, string subtitle, object properties)
        {
            var info = new AccumulationInfo()
            {
                name = Assembly.GetValidName("Accumulation "),
                length = 2f,
                width = 0.45f,
                NumberOfSections = 4,
                SurfaceType = GetSurfaceType(title)
            };

            var assembly = new Accumulation(info);

            return assembly;
        }

        #endregion

        #region Equipment

        public static Assembly SwitchSorter(string title, string subtitle, object properties)
        {
            var info = new SwitchSorterInfo()
            {
                name = Assembly.GetValidName("Switch Sorter "),
                height = 1f,
                GeometryData = new BoxGeometry()
                {
                    Length = 0.5f,
                    Width = 0.45f
                },
                Boundaries = new Boundaries()
                {
                    UseLeftBoundary = false,
                    UseRightBoundary = false
                },
                SurfaceData = new StraightSurface()
                {
                    Color = Colors.Orange
                }
            };

            var assembly = new SwitchSorter(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly LiftingUnit(string title, string subtitle, object properties)
        {
            var info = new LiftingUnitInfo()
            {
                name = Assembly.GetValidName("Lifting Unit "),
                GeometryData = new BoxGeometry()
                {
                    Length = 0.5f,
                },
            };

            var assembly = new LiftingUnit(info);
            assembly.InsertMotor(Surface.Create());

            return assembly;
        }

        public static Assembly Pusher(string title, string subtitle, object properties)
        {
            var info = new PusherInfo()
            {
                name = Assembly.GetValidName("Pusher "),
                height = 0.8f
            };

            var assembly = new Pusher(info);
            return assembly;
        }

        public static Assembly VerticalBladeStop(string title, string subtitle, object properties)
        {
            var info = new VerticalBladeStopInfo()
            {
                name = Assembly.GetValidName("Vertical Blade Stop "),
                height = 0.8f
            };

            var assembly = new VerticalBladeStop(info);
            return assembly;
        }

        public static Assembly BarcodeReader(string title, string subtitle, object properties)
        {
            var info = new BarcodeReaderInfo()
            {
                name = Assembly.GetValidName("Barcode Reader "),
                height = 0.8f,
                length = 0.6f
            };

            var assembly = new BarcodeReader(info);
            return assembly;
        }

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