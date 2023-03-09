using System.Windows.Media;

namespace Experior.Catalog.PalletConveyingSystem
{
    public class PalletConveyingSystem : Experior.Core.Catalog
    {
        #region Constructor

        public PalletConveyingSystem()
            : base("Pallet Conveying System")
        {
            Simulation = Experior.Core.Environment.Simulation.Physics;

            Common.Mesh = new Experior.Core.Resources.EmbeddedResourceLoader(System.Reflection.Assembly.GetExecutingAssembly());
            Common.Icon = new Experior.Core.Resources.EmbeddedImageLoader(System.Reflection.Assembly.GetExecutingAssembly());

            #region Roller Conveyors

            Add(Common.Icon.Get("Roller"), "Roller Conveyors", "Straight", Simulation, Create.RollerConveyor);

            #endregion

            #region Chain Conveyors

            Add(Common.Icon.Get("Chain"), "Chain Conveyors", "Straight", Simulation, Create.ChainConveyor);

            #endregion
        }

        #endregion

        #region Public Properties

        public override ImageSource Logo => Common.Icon.Get("Pallet");

        #endregion
    }
}