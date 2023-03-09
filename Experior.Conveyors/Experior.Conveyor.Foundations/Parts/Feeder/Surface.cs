using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Xml.Serialization;
using Experior.Utils.Feeder;
using Experior.Core.Parts;
using Experior.Core.Loads;
using Experior.Core.Mathematics;
using Experior.Interfaces;
using Experior.Conveyor.Foundations.Assemblies;
using Environment = Experior.Core.Environment;
using Experior.Core.Properties;

namespace Experior.Conveyor.Foundations.Parts.Feeder
{
    /// <summary>
    /// Class <c>Surface</c> provides the functionality of the Standard Feeder.
    /// </summary>
    [TypeConverter(typeof(ObjectConverter))]
    [Serializable, XmlInclude(typeof(Surface)), XmlType(TypeName = "Experior.Surface.Foundations.Parts.Feeder.Surface")]
    public class Surface
    {
        #region Fields

        [XmlIgnore]
        private BaseFeeder _feeder;
        
        [XmlIgnore]
        private Experior.Core.Parts.Triangle _feederSymbol;

        #endregion

        #region Public Properties

        [XmlIgnore]
        public Straight Parent { get; set; }

        [Browsable(false)]
        public BaseFeederInfo Info { get; set; }

        [Browsable(false)]
        public AuxiliaryData.SystemType System { get; set; }

        #endregion

        #region Public Methods

        /// < summary>
        /// Creates a feeder and its visual representation.
        /// </summary>
        public void CreateFeeder()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (!CheckParent())
                {
                    return;
                }

                if (Info == null)
                {
                    Info = new BaseFeederInfo()
                    {
                        name = Experior.Core.Assemblies.Assembly.GetValidName(Parent.Name + " Feeder"),
                        length = 0.25f,
                        height = 0.25f,
                        width = 0.25f,
                        loadtype = System == AuxiliaryData.SystemType.CaseConveying ? "Package" : "Pallet",
                        color = Colors.SandyBrown
                    };
                }

                _feeder = new BaseFeeder(Info);
                _feederSymbol = new Triangle();
                Parent.Add(_feederSymbol);
                Parent.Add(_feeder);

                UpdateFeederSymbol();
                _feeder.OnLoadCreated += LoadCreated;
                _feederSymbol.OnSelected += FeederSymbolSelected;
            });
        }

        /// < summary>
        /// Creates a feeder using <c>Info</c> stored.
        /// </summary>
        public void Reconstruct()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (Info == null)
                {
                    return;
                }

                CreateFeeder();
            });
        }

        /// < summary>
        /// Removes the feeder from its parent.
        /// </summary>
        public void RemoveFeeder()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (!CheckParent())
                {
                    return;
                }

                if (_feeder != null)
                {
                    _feeder.OnLoadCreated -= LoadCreated;
                    Parent.Remove(_feeder);
                    _feeder.Dispose();
                    _feeder = null;
                }

                if (_feederSymbol != null)
                {
                    _feederSymbol.OnSelected -= FeederSymbolSelected;

                    Parent.Remove(_feederSymbol);
                    _feederSymbol.Dispose();
                    _feederSymbol = null;
                }

                Info.Dispose();
                Info = null;

                Experior.Core.Environment.SolutionExplorer.Refresh();
            });
        }

        /// < summary>
        /// Starts feeding.
        /// </summary>
        public void Feed()
        {
            Experior.Core.Environment.InvokeIfRequired(() =>
            {
                if (!CheckParent())
                {
                    return;
                }

                if (_feeder != null)
                {
                    _feeder.Feed();
                    return;
                }

                // TODO: CUSTOM BOX AND PALLET ?

                var load = Experior.Core.Loads.Load.Create(Resources.GetMesh("Default.dae"), 0.25f, 0.25f, 0.25f);
                //var load = Experior.Core.Loads.Load.Create(Common.Mesh.Get("ToteLoad.x"), 0.25f, 0.25f, 0.25f);
                //var load = Experior.Core.Loads.Load.CreateCan(0.2f, 0.5f, Colors.MistyRose);
                load.CenterOfMassOffsetLocalPosition = Vector3.Zero;
                load.Rigid = Rigids.Box;
                load.Weight = 0.5f;
                load.Friction = new Friction() { Coefficient = Coefficients.Sticky };

                LoadCreated(null, load);

                if (Core.Events.Recorder.Recording)
                {
                    Core.Events.Recorder.Add(Parent.Name, "Feed", load.Info.Copy());
                }
            });
        }

        /// < summary>
        /// Resets feeding.
        /// </summary>
        public void Reset()
        {
            _feeder?.Reset();
        }

        /// < summary>
        /// Generates the a context menu with the options to create/remove and start/stop feeder.
        /// </summary>
        public List<Environment.UI.Toolbar.BarItem> GetContextMenu()
        {
            var menu = new List<Environment.UI.Toolbar.BarItem>();

            if (_feeder != null && _feederSymbol.Selected)
            {
                menu.Add(_feeder.Started
                    ? new Environment.UI.Toolbar.Button("Stop", Resources.GetIcon("FeederStop"))
                    {
                        OnClick = (sender, args) => _feeder.Stop()
                    }
                    : new Environment.UI.Toolbar.Button("Start", Resources.GetIcon("FeederStart"))
                    {
                        OnClick = (sender, args) => _feeder.Start()
                    });
            }

            if (!Environment.Scene.Locked)
            {
                menu.Add(_feederSymbol != null
                    ? new Environment.UI.Toolbar.Button("Remove Feeder", Resources.GetIcon("RemoveFeeder"))
                    {
                        OnClick = (sender, args) => RemoveFeeder()
                    }
                    : new Environment.UI.Toolbar.Button("Insert Feeder", Resources.GetIcon("AddFeeder"))
                    {
                        OnClick = (sender, args) => CreateFeeder()
                    });
            }

            return menu;
        }

        #endregion

        #region Private Methods

        private void UpdateFeederSymbol()
        {
            if (_feederSymbol == null)
            {
                return;
            }

            _feederSymbol.Width = 0.2f; //TODO: CHANGE TO AUTOMATIC SIZE BASED ON PARENT DIMENSIONS !!
            _feederSymbol.Length = 0.2f;

            _feederSymbol.Color = Colors.Gold;
            _feederSymbol.LocalPosition = new Vector3(_feederSymbol.Length / 2, _feederSymbol.Height / 2, 0);
        }

        private void LoadCreated(BaseFeeder sender, Load load)
        {
            if (load == null)
            {
                return;
            }

            float radius;
            var start = Parent.Position;
            var end = Parent.Position + Vector3.Transform(new Vector3(Parent.Length, 0f, 0f), Parent.Orientation);

            var tempPos = start;

            if (Parent.Motor != null && Parent.Motor.Direction == MotorDirection.Backward)
            {
                radius = load.Length / 2;
                tempPos += Vector3.Transform(new Vector3(Parent.Length, 0f, 0f), Parent.Orientation);
            }
            else
            {
                radius = -load.Length / 2;
            }

            load.Position = Trigonometry.RotationPoint(tempPos, Parent.Yaw - (float)Math.PI / 2, radius);
            var up = new Vector3(0, load.Height / 2, 0);
            up = Vector3.Transform(up, Parent.Orientation);

            load.CenterOfMassOffsetLocalPosition = Vector3.Zero;
            load.Position += up;
            load.Yaw = Trigonometry.Yaw(start, end);
            load.Roll = Trigonometry.Roll(start, end);
        }

        private void FeederSymbolSelected(RigidPart sender)
        {
            if (_feederSymbol == null)
            {
                return;
            }

            if (_feeder == null)
            {
                return;
            }

            _feederSymbol.Selected = false;
            _feederSymbol?.Select();
            Experior.Core.Environment.Properties.Set(_feeder);
        }

        private bool CheckParent()
        {
            if (Parent == null)
            {
                Log.Write($"Set the parent for the Feeder", Colors.Orange, LogFilter.Communication);
                return false;
            }

            return true;
        }

        #endregion
    }
}