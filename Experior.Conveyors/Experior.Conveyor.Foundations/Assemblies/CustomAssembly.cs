using Experior.Core.Assemblies;
using System;
using System.Linq;
using System.Xml.Serialization;
using Experior.Interfaces.Collections;

namespace Experior.Conveyor.Foundations.Assemblies
{
    /// <summary>
    /// Abstract class <c>CustomAssembly</c> contains common implementation to reinforce use of code when assembly inserted and scene loaded.
    /// </summary>
    public abstract class CustomAssembly : Assembly
    {
        #region Fields



        #endregion

        #region Constructor

        protected CustomAssembly(CustomAssemblyInfo info)
            : base(info)
        {
            if (Core.Environment.Scene.Loading)
            {
                Core.Environment.Scene.OnLoaded += SceneOnLoaded;
            }

            if (Pasting)
            {
                Experior.Core.Assemblies.Assembly.Items.OnAssembliesAdded += OnAssembliesAdded;
            }
        }

        #endregion

        #region Public Methods

        public override void Inserted()
        {
            base.Inserted();
            Build();
        }

        #endregion

        #region Protected Methods

        protected abstract void BuildAssembly();

        protected abstract void BuildMotors();

        protected abstract void ExecuteSceneOnLoaded();

        #endregion

        #region Private Methods

        private void SceneOnLoaded()
        {
            Experior.Core.Environment.Scene.OnLoaded -= SceneOnLoaded;

            Build();
            ExecuteSceneOnLoaded();
        }

        private void OnAssembliesAdded(object sender, AssemblyCollectionChangedEventArgs e)
        {
            foreach (var item in e.Items.OfType<Assembly>())
            {
                if (item.Pasting)
                {
                    Inserted();
                }
            }

            Experior.Core.Assemblies.Assembly.Items.OnAssembliesAdded -= OnAssembliesAdded;
        }

        private void Build()
        {
            BuildAssembly();
            BuildMotors();
        }

        #endregion
    }

    [Serializable, XmlInclude(typeof(CustomAssemblyInfo)), XmlType(TypeName = "Experior.Surface.Foundations.Assemblies.CustomAssemblyInfo")]
    public class CustomAssemblyInfo : Experior.Core.Assemblies.AssemblyInfo
    {
    }
}