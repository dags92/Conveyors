using System.Collections.Generic;
using Experior.Conveyor.Foundations.Motors;
using Experior.Conveyor.Foundations.Motors.Basic;
using Experior.Conveyor.Foundations.Motors.Interfaces;
using Experior.Interfaces;
using Environment = Experior.Core.Environment;

namespace Experior.Conveyor.Foundations.Utilities
{
    /// <summary>
    /// Class <c>ContextMenu</c> provides the functionality to get generic context menus for the creation of Surface, Vector and Rotational motor types.
    /// </summary>
    public static class ContextMenu
    {
        /// <summary>
        /// Generation of a Menu to create/link surface motors.
        /// </summary>
        /// <param name="assembly"> which the surface motor will be inserted to.</param>
        /// <param name="motor"> used to avoid displaying the current surface motor of the current assembly.</param>
        /// <param name="link"> enables the option to reuse a pre-existing surface motor from a different assembly.</param>
        public static List<Environment.UI.Toolbar.BarItem> CreateSurfaceMotorMenu(Experior.Core.Assemblies.Assembly assembly, IElectricMotor motor, bool link)
        {
            var menu = new List<Experior.Core.Environment.UI.Toolbar.BarItem>();

            // Create New Surface Motor :
            var category = new Environment.UI.Toolbar.SplitButton()
            {
                Text = "Surface", Image = Resources.GetIcon("SurfaceMotor.png")
            };

            var basic = new Environment.UI.Toolbar.SplitButton()
            {
                Text = "Basic", Image = Resources.GetIcon("SurfaceMotor.png")
            };

            basic.Add(new Environment.UI.Toolbar.Button("New", Resources.GetIcon("Add.svg"))
            {
                OnClick = (sender, args) => assembly.InsertMotor(Surface.Create())
            });

            if (link)
            {
                // Linking a Surface Motor :
                var name = motor != null ? motor.Name : string.Empty;

                foreach (var globalMotor in Experior.Core.Motors.Motor.Items.Values)
                {
                    if (name == globalMotor.Name)
                    {
                        continue;
                    }

                    switch (globalMotor)
                    {
                        case IElectricSurfaceMotor basicSurface:

                            basic.Add(new Environment.UI.Toolbar.Button(globalMotor.Name, Resources.GetIcon("Copy"))
                            {
                                OnClick = (sender, args) => assembly.InsertMotor(basicSurface)
                            });

                            break;
                    }
                }
            }

            category.Add(basic);
            menu.Add(category);

            return menu;
        }

        /// <summary>
        /// Generation of a Menu to calibrate vector motor.
        /// </summary>
        /// <param name="motor"> motor to be calibrated.</param>
        public static List<Environment.UI.Toolbar.BarItem> CreateVectorMotorMenu(IElectricVectorMotor motor)
        {
            var menu = new List<Experior.Core.Environment.UI.Toolbar.BarItem>
            {
                new Environment.UI.Toolbar.Button("Reset", Resources.GetIcon("Calibrate"))
                {
                    OnClick = (sender, args) => motor.Calibrate()
                }
            };

            return menu;
        }
    }
}
