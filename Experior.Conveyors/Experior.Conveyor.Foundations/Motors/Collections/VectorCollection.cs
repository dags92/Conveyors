using System.Collections;
using System.Collections.Generic;
using Experior.Core.Assemblies;
using Experior.Core.Parts;
using Experior.Interfaces;

namespace Experior.Conveyor.Foundations.Motors.Collections
{
    public class VectorAssemblyCollection : IEnumerable<Assembly>
    {
        #region Fields

        private readonly Dictionary<Assembly, float> _gears = new Dictionary<Assembly, float>();

        private readonly List<Assembly> _items = new List<Assembly>();

        #endregion

        #region Public Properties

        public Dictionary<Assembly, float> Gears => _gears;

        public List<Assembly> Items => _items;

        #endregion

        #region Public Methods

        public void Add(Assembly assembly)
        {
            Add(assembly, 1);
        }

        public void Add(Assembly assembly, float gear)
        {
            if (!assembly.Configured)
            {
                Log.Write("You must add the assembly to an assembly before it can be controlled by a motor", System.Windows.Media.Colors.Red, LogFilter.Error);

                return;
            }

            if (_items.Contains(assembly))
            {
                return;
            }

            _items.Add(assembly);
            _gears.Add(assembly, gear);
        }

        public void Clear()
        {
            _items.Clear();
            _gears.Clear();
        }

        /// <summary>
        /// Removes the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void Remove(Assembly assembly)
        {
            if (_items.Contains(assembly))
            {
                _items.Remove(assembly);
            }

            if (_gears.ContainsKey(assembly))
            {
                _gears.Remove(assembly);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
        IEnumerator<Assembly> IEnumerable<Assembly>.GetEnumerator() => _items.GetEnumerator();

        #endregion
    }

    public class VectorPartCollection : IEnumerable<RigidPart>
    {
        #region Fields

        private readonly Dictionary<RigidPart, float> _gears = new Dictionary<RigidPart, float>();

        private readonly List<RigidPart> _items = new List<RigidPart>();

        #endregion

        #region Public Properties

        public Dictionary<RigidPart, float> Gears => _gears;

        public List<RigidPart> Items => _items;

        #endregion

        #region Public Methods

        public void Add(RigidPart part)
        {
            Add(part, 1);
        }

        public void Add(RigidPart part, float gear)
        {
            if (!part.Configured)
            {
                Log.Write("You must add the part to an assembly before it can be controlled by a motor", System.Windows.Media.Colors.Red, LogFilter.Error);

                return;
            }

            if (_items.Contains(part))
            {
                return;
            }

            _items.Add(part);
            _gears.Add(part, gear);
        }

        public void Clear()
        {
            _items.Clear();
            _gears.Clear();
        }

        public void Remove(RigidPart part)
        {
            if (_items.Contains(part))
            {
                _items.Remove(part);
            }

            if (_gears.ContainsKey(part))
            {
                _gears.Remove(part);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
        IEnumerator<RigidPart> IEnumerable<RigidPart>.GetEnumerator() => _items.GetEnumerator();

        #endregion
    }
}
