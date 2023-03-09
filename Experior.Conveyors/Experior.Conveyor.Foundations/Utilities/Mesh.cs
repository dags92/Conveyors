namespace Experior.Conveyor.Foundations.Utilities
{
    /// <summary>
    /// Class <c>Mesh</c> to manipulate or transform <c>Experior.Core.Parts.Model</c> types
    /// </summary>
    public static class Mesh
    {
        /// <summary>
        /// Scales dimensions by a same factor.
        /// </summary>
        /// <param name="factor">Value used to scale Length, Height and Width dimensions.</param>
        /// <param name="cad">Mesh to be scaled.</param>
        public static void ScaleCadFile(float factor, Core.Parts.Model cad)
        {
            cad.Length *= factor;
            cad.Width *= factor;
            cad.Height *= factor;
        }
    }
}
