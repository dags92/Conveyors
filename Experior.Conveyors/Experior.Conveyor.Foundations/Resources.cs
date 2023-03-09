using System.Windows.Media;
using Experior.Core.Resources;

namespace Experior.Conveyor.Foundations
{
    /// <summary>
    /// Class <c>Resources</c> provides the functionality to get embedded images and resources.
    /// </summary>
    public static class Resources
    {
        private static readonly Experior.Core.Resources.EmbeddedImageLoader Icons = new Experior.Core.Resources.EmbeddedImageLoader(System.Reflection.Assembly.GetExecutingAssembly());

        private static readonly Experior.Core.Resources.EmbeddedResourceLoader Meshes = new Experior.Core.Resources.EmbeddedResourceLoader(System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary>
        /// Gets the embedded <c>ImageSource</c>.
        /// </summary>
        /// <param name="name">Name of the file allocated inside the folder Icon.</param>
        public static ImageSource GetIcon(string name) => Icons.Get(name);

        /// <summary>
        /// Gets the <c>EmbeddedResource</c>.
        /// </summary>
        /// <param name="name">Name of the file allocated inside the folder Mesh.</param>
        public static EmbeddedResource GetMesh(string name) => Meshes.Get(name);
    }
}
