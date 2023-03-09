using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Experior.Core.Assemblies;
using Experior.Core.Parts;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Pneumatics
{
    /// <summary>
    /// Class <c>Linear</c> provides the implementation of a Linear Pneumatic Cylinder.
    /// </summary>
    public class Linear : Base
    {
        #region Fields

        private readonly LinearInfo _info;

        #endregion

        #region Constructor

        public Linear(Assembly parent, LinearInfo info) : base(parent, info)
        {
            _info = info;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the list of Parts which will be moved in the scene. 
        /// </summary>
        protected List<RigidPart> Parts { get; } = new List<RigidPart>();

        /// <summary>
        /// Gets the list of Assemblies which will be moved in the scene. 
        /// </summary>
        protected List<Assembly> Assemblies { get; } = new List<Assembly>();

        #endregion

        #region Public Methods

        /// < summary>
        /// Adds <c>part</c> to <see cref="Parts"/>.
        /// </summary>
        public void Add(RigidPart part)
        {
            if (Parts.Contains(part))
            {
                return;
            }

            Parts.Add(part);
        }

        /// < summary>
        /// Adds <c>assembly</c> to <see cref="Assemblies"/>.
        /// </summary>
        public void Add(Assembly assembly)
        {
            if (Assemblies.Contains(assembly))
            {
                return;
            }

            Assemblies.Add(assembly);
        }

        /// < summary>
        /// Removes <c>part</c> from <see cref="Parts"/>.
        /// </summary>
        public void Remove(RigidPart part)
        {
            Parts.Remove(part);
        }

        /// < summary>
        /// Removes <c>assembly</c> from <see cref="Assemblies"/>.
        /// </summary>
        public void Remove(Assembly assembly)
        {
            Assemblies.Remove(assembly);
        }

        /// < summary>
        /// Removes all parts and assemblies from <see cref="Parts"/> and <see cref="Assemblies"/>.
        /// </summary>
        public void RemoveAll()
        {
            Assemblies.Clear();
            Parts.Clear();
        }

        #endregion

        #region Protected Methods

        /// < summary>
        /// Calculates the displacement produced by <c>deltaTime</c>.
        /// </summary>
        protected override void ExecuteStep(float deltaTime)
        {
            var deltaMovement = deltaTime * Speed;

            deltaMovement *= Stroke < 0 ? -1f : 1f;
            deltaMovement *= State == States.Injecting ? -1f : 1f;

            Position += deltaMovement;
            Move(deltaMovement);
        }

        /// < summary>
        /// Moves all the parts and assemblies by a <c>delta</c> distance considering the direction.
        /// </summary>
        protected override void Move(float delta)
        {
            foreach (var part in Parts)
            {
                part.LocalPosition += delta * Direction;
            }

            foreach (var assembly in Assemblies)
            {
                assembly.LocalPosition += delta * Direction;
            }
        }

        /// < summary>
        /// Enables or Disables all the parts and assemblies.
        /// </summary>
        protected override void Enable(bool value)
        {
            foreach (var part in Parts)
            {
                part.Enabled = value;
            }

            foreach (var assembly in Assemblies)
            {
                assembly.Enabled = value;
            }
        }

        /// < summary>
        /// Deselects all the parts and assemblies.
        /// </summary>
        protected override void DeSelectPartsAndAssemblies()
        {
            foreach (var part in Parts)
            {
                part.Deselect();
            }

            foreach (var assembly in Assemblies)
            {
                assembly.Deselect();
            }
        }

        #endregion
    }

    [TypeConverter(typeof(ObjectConverter))]
    [Serializable]
    [XmlInclude(typeof(LinearInfo))]
    [XmlType(TypeName = "Experior.Surface.Foundations.Pneumatics.LinearInfo")] 
    public class LinearInfo : BaseInfo
    {

    }
}
