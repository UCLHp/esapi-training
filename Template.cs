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



//[assembly: ESAPIScript(IsWriteable = true)]  //Uncomment if you want to write changes to database


namespace VMS.TPS
{
    class Script
    {

        public Script()
        {
        }

        public void Execute(ScriptContext context)
        {

            Patient patient = context.Patient;

            // BeginModifications will throw an exception if the system is not configured for research
            // use or system is a clinical system and the script is not approved.
            //patient.BeginModifications(); //Uncomment if you want to write changes to database




            MessageBox.Show(patient.Id);


        }
    }

}
