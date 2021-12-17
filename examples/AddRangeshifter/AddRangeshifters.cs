using System.Windows;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;


/* Script to add a rangeshifter to all beams in a plan.
 * 
 * (Eclipse loses the beam line and thus rangeshifters after various edits to 
 * a field, such as changing the STV volume / margins etc)
 */

[assembly: ESAPIScript(IsWriteable = true)]
namespace VMS.TPS
{
    class Script
    {

        public Script()
        {
        }

        public void Execute(ScriptContext context) 
        {
            // Hard code supported rangeshifter names
            List<string> availableRangeshifters = new List<string>() { "RS = 5 cm", "RS = 3 cm", "RS = 2 cm" };

            // Simple GUI to select desired rangeshifter
            var selectedRangeshifter = SelectRangeshifterWindow.SelectRangeshifter(availableRangeshifters);

            Patient patient = context.Patient;

            // BeginModifications will throw an exception if the system is not configured for research
            // use or system is a clinical system and the script is not approved.
            // After calling BeginModifications successfully it is possible to modifiy patient data.
            patient.BeginModifications();

            
            foreach (IonBeam ionBeam in context.IonPlanSetup.IonBeams)
            {
                IonBeamParameters beamParams = ionBeam.GetEditableParameters();
                
                beamParams.PreSelectedRangeShifter1Id = selectedRangeshifter;
                beamParams.PreSelectedRangeShifter1Setting = "IN";

                // Apply changes to Eclipse; without this nothing will happen
                ionBeam.ApplyParameters(beamParams);
            }
        }
    }



    /*
     * To get this "Window" GUI to work I had to add two lines to the project file:
     *    <UseWPF>true</UseWPF>
     *    <UseWindowsForms>true</UseWindowsForms>
     * as explained here: https://stackoverflow.com/a/58129582/8709538
     * 
     */
    class SelectRangeshifterWindow : Window
    {

        // Simple GUI to select a rangeshifter from list

        public static string SelectRangeshifter(List<string> rsList)
        {
            win = new Window();
            //win.Title = "Choose rangeshifter:";

            win.WindowStartupLocation = WindowStartupLocation.Manual;
            win.Left = 400;
            win.Top = 300;
            win.Width = 200;
            win.Height = 200;

            var grid = new Grid();
            win.Content = grid;

            var text = new TextBlock();
            text.Text = "     Select rangeshifter:";
            text.Margin = new Thickness(10,8,10,10);
            text.FontSize = 12;
            grid.Children.Add(text);

            var list = new ListBox();
            foreach (var s in rsList)
            {
                list.Items.Add(s);
            }

            list.VerticalAlignment = VerticalAlignment.Top;
            list.Margin = new Thickness(30, 30, 30, 55);
            grid.Children.Add(list);

            var button = new Button();
            button.Content = "OK";
            button.Height = 30;
            button.Width = 70;
            button.VerticalAlignment = VerticalAlignment.Bottom;
            button.Margin = new Thickness(30, 10, 30, 20);
            button.Click += button_Click;

            grid.Children.Add(button);

            if (win.ShowDialog() == true)
            {
                return (string)list.SelectedItem;
            }
            return null;
        }

        static Window win = null;

        static void button_Click(object sender, RoutedEventArgs e)
        {
            win.DialogResult = true;
            win.Close();
        }

    }


}
