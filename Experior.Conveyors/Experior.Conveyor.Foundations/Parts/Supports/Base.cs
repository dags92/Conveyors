using System.ComponentModel;
using Experior.Core.Assemblies;
using Experior.Core.Properties;
using Experior.Core.Properties.TypeConverter;

namespace Experior.Conveyor.Foundations.Parts.Supports
{
    /// <summary>
    /// Abstract class <c>SplittableStraight</c> contains common class members for Support components.
    /// </summary>
    public abstract class Base : Experior.Core.Assemblies.Assembly, Platform.ISupport
    {
        #region Constructor

        protected Base(AssemblyInfo info) : base(info)
        {
            Platform.Add(this);
        }

        #endregion

        #region Public Properties

        [Category("Size")]
        [DisplayName("Height")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(1)]
        public virtual float Height { get; set; }

        [Category("Size")]
        [DisplayName("Width")]
        [TypeConverter(typeof(FloatMeterToMillimeter))]
        [PropertyOrder(2)]
        public virtual float Width { get; set; }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            base.Dispose();
            Platform.RemoveLeg(this);
        }

        #endregion
    }
}
