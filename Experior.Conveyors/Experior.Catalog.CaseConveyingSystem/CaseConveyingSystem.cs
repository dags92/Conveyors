using System.Windows.Media;

namespace Experior.Catalog.CaseConveyingSystem
{
    /// <summary>
    /// Class <c>CaseConveyingSystem</c> contains different components related to case conveying system applications.
    /// Components are enabled only for Physics Engine.
    /// </summary>
    public class CaseConveyingSystem : Experior.Core.Catalog
    {
        #region Constructor

        public CaseConveyingSystem()
            : base("Case Conveying System")
        {
            Simulation = Experior.Core.Environment.Simulation.Physics;

            Common.Mesh = new Experior.Core.Resources.EmbeddedResourceLoader(System.Reflection.Assembly.GetExecutingAssembly());
            Common.Icon = new Experior.Core.Resources.EmbeddedImageLoader(System.Reflection.Assembly.GetExecutingAssembly());

            #region Belt Conveyors

            Add(Common.Icon.Get("StraightBelt"), "Belt Conveyors", "Straight", Simulation, Create.StraightConveyor);
            Add(Common.Icon.Get("MergeBelt"), "Belt Conveyors", "Merge", Simulation, Create.MergeConveyor);
            Add(Common.Icon.Get("DivertBelt"), "Belt Conveyors", "Divert", Simulation, Create.DivertConveyor);
            Add(Common.Icon.Get("Transfer"), "Belt Conveyors", "Transfer", Simulation, Create.TransferConveyor);

            Add(Common.Icon.Get("CurveCWBelt"), "Belt Conveyors", "Curve Clockwise", Simulation, Create.CurveCwConveyor);
            Add(Common.Icon.Get("CurveCCWBelt"), "Belt Conveyors", "Curve Counterclockwise", Simulation, Create.CurveCcwConveyor);
            Add(Common.Icon.Get("SpiralBelt"), "Belt Conveyors", "Spiral", Simulation, Create.SpiralConveyor);

            Add(Common.Icon.Get("AccumulationBelt"), "Belt Conveyors", "Accumulation", Simulation, Create.AccumulationConveyor);

            #endregion

            #region Roller Conveyors

            Add(Common.Icon.Get("StraightRoller"), "Roller Conveyors", "Straight", Simulation, Create.StraightConveyor);
            Add(Common.Icon.Get("MergeRoller"), "Roller Conveyors", "Merge", Simulation, Create.MergeConveyor);
            Add(Common.Icon.Get("DivertRoller"), "Roller Conveyors", "Divert", Simulation, Create.DivertConveyor);
            Add(Common.Icon.Get("AlignmentRoller"), "Roller Conveyors", "Alignment", Simulation, Create.RollerAlignment);

            Add(Common.Icon.Get("CurveCWRoller"), "Roller Conveyors", "Curve Clockwise", Simulation, Create.CurveCwConveyor);
            Add(Common.Icon.Get("CurveCCWRoller"), "Roller Conveyors", "Curve Counterclockwise", Simulation, Create.CurveCcwConveyor);
            Add(Common.Icon.Get("SpiralRoller"), "Roller Conveyors", "Spiral", Simulation, Create.SpiralConveyor);

            Add(Common.Icon.Get("AccumulationRoller"), "Roller Conveyors", "Accumulation", Simulation, Create.AccumulationConveyor);

            #endregion

            #region Strap Conveyors

            Add(Common.Icon.Get("StraightStrap"), "Strap Conveyors", "Straight", Simulation, Create.StraightConveyor);
            Add(Common.Icon.Get("MergeStrap"), "Strap Conveyors", "Merge", Simulation, Create.MergeConveyor);
            Add(Common.Icon.Get("DivertStrap"), "Strap Conveyors", "Divert", Simulation, Create.DivertConveyor);

            #endregion

            #region Equipment

            Add(Common.Icon.Get("SwitchSorter"), "Equipment", "Switch Sorter", Simulation, Create.SwitchSorter);
            Add(Common.Icon.Get("LiftingUnit"), "Equipment", "Lifting Unit", Simulation, Create.LiftingUnit);

            Add(Common.Icon.Get("Pusher"), "Equipment", "Pusher", Simulation, Create.Pusher);
            Add(Common.Icon.Get("VerticalBladeStop"), "Equipment", "Vertical Blade Stop", Simulation, Create.VerticalBladeStop);

            Add(Common.Icon.Get("BarcodeReader"), "Equipment", "Barcode Reader", Simulation, Create.BarcodeReader);

            #endregion
        }

        #endregion

        #region Public Properties

        public override ImageSource Logo => Common.Icon.Get("Box");

        #endregion
    }
}