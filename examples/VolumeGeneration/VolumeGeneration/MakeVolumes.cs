using System.Windows;
using System.Linq;   // need this for Where method
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media;

[assembly: ESAPIScript(IsWriteable = true)]
namespace VMS.TPS
{
    class Script
    {

        //Method needed for HighResolution bug hack
        public static int _GetSlice(double z, StructureSet SS)
        {
            var imageRes = SS.Image.ZRes;
            return System.Convert.ToInt32((z - SS.Image.Origin.z) / imageRes);
        }

        public Script()
        {
        }

        public void Execute(ScriptContext context)
        {

            // Specify margin (mm) for OTVs/PRVs
            int MARGIN = 3;


            Patient patient = context.Patient;

            // BeginModifications will throw an exception if the system is not
            // configured for research use or system is a clinical system and
            // the script is not approved.
            // After calling BeginModifications successfully it is possible
            // to modifiy patient data.
            patient.BeginModifications();



            // Structures that should exist: TV_high, CTV_low, critical OARs
            // Margin will be added to all OARs
            List<string> oars = new List<string>() { "Canal", "Aorta" };
            string ctv_h = "CTV_High";
            string ctv_l = "CTV_Low";

            // Structures to be made
            string ctv_h_ed = "CTV_High_ed";
            string ctv_l_ed = "CTV_Low_ed";
            string otv_h = "OTV_High";
            string otv_l = "OTV_Low";
            string otv_h_ed = "OTV_High_ed";
            string otv_l_ed = "OTV_Low_ed";



            //Get list of all structure names now in patient
            //  NOTE: Only the context at the point of script execution is accessible. So this structure list cannot be updated with
            //  any changes made by this script, i.e. if we add more structures we can never get them from this method later.
            //  The reason is that in these stand alone scripts we cannot save to the database, we have to just click save after script execution. 
            var structures = context.StructureSet.Structures;


            //Get the required existing structures (should really check they exist)
            Structure CTV_High = structures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains(ctv_h.ToUpper())).FirstOrDefault();
            Structure CTV_Low = structures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains(ctv_l.ToUpper())).FirstOrDefault();
            Structure body = structures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("BODY")).FirstOrDefault();
  


            Structure OTV_High = context.StructureSet.AddStructure("PTV", otv_h);
            Structure OTV_Low = context.StructureSet.AddStructure("PTV", otv_l);
            //Margins: First make a SegmentVolume object via Margin() method and
            //    then assign this to the Structure's SegmentVolume attribute 
            OTV_High.SegmentVolume = CTV_High.Margin(MARGIN);
            OTV_Low.SegmentVolume = CTV_Low.Margin(MARGIN);



            Structure OTV_High_ed = context.StructureSet.AddStructure("PTV", otv_h_ed);
            OTV_High_ed.SegmentVolume = OTV_High.And(body);

            Structure OTV_Low_ed = context.StructureSet.AddStructure("PTV", otv_l_ed);
            OTV_Low_ed.SegmentVolume = OTV_Low.And(body);
            OTV_Low_ed.SegmentVolume = OTV_Low_ed.Sub(OTV_High_ed);


            // How to set structure colours; requires a reference to PresentationCore.dll (probably found somewhere like
            // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0) and "using System.Windows.Media;"
            OTV_High.Color = Color.FromRgb(50, 50, 220);
            OTV_High_ed.Color = Color.FromRgb(50, 50, 220);
            OTV_Low.Color = Color.FromRgb(30, 30, 190);
            OTV_Low_ed.Color = Color.FromRgb(30, 30, 190);



            Structure CTV_High_ed = context.StructureSet.AddStructure("CTV", ctv_h_ed);
            CTV_High_ed.SegmentVolume = CTV_High.And(body);

            Structure CTV_Low_ed = context.StructureSet.AddStructure("CTV", ctv_l_ed);
            CTV_Low_ed.SegmentVolume = CTV_Low.And(body);
            CTV_Low_ed.SegmentVolume = CTV_Low_ed.Sub(OTV_High_ed);


            // List the structures we will want to crop back from crtiical PRVs
            List<Structure> cropFromOARs = new List<Structure>()
            {
                OTV_High_ed, OTV_Low_ed, CTV_High_ed, CTV_Low_ed
            };


            // Create PRV structures using Margin(); crop CTV/OTVs from these        
            foreach (string oar in oars)
            {
                Structure oarStruct = structures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains(oar.ToUpper())).FirstOrDefault();
                string prvName = oar + "_PRV";
                Structure prv = context.StructureSet.AddStructure("Organ", prvName);
                prv.SegmentVolume = oarStruct.Margin(MARGIN);


                // Crop all _ed OTVs/CTVs from PRVs
                foreach (Structure structure in cropFromOARs)   
                {

                    // ---------------- START HACK FIX ------------------------------------------------------------------------
                    // Hack fix for bug in HighResolution leading to error "segment volumes have different geometries"
                    // Take from: https://www.reddit.com/r/esapi/comments/mbbxa3/low_and_high_resolution_structures/gsps72u?utm_source=share&utm_medium=web2x&context=3
                    // which references: https://jhmcastelo.medium.com/tips-for-vvectors-and-structures-in-esapi-575bc623074a
                    if (prv.IsHighResolution)
                    {
                        Structure lowResSSource = context.StructureSet.AddStructure("CONTROL", "lowResSrc");
                        var mesh = prv.MeshGeometry.Bounds;
                        var meshLow = _GetSlice(mesh.Z, context.StructureSet);
                        var meshUp = _GetSlice(mesh.Z + mesh.SizeZ, context.StructureSet) + 1;
                        for (int j = meshLow; j <= meshUp; j++)
                        {
                            var contours = prv.GetContoursOnImagePlane(j);
                            if (contours.Length > 0)
                            {
                                lowResSSource.AddContourOnImagePlane(contours[0], j);
                            }
                        }
                        prv.SegmentVolume = lowResSSource.SegmentVolume;
                        context.StructureSet.RemoveStructure(lowResSSource);
                    }
                    // ---------------   END HACK FIX ------------------------------------------------------------------------


                    // All of the above was so that we could successfully execute the following command...
                    // Without it we get an error that the objects "have different geometries".
                    // I think the issue is that "prv" has been generated as a high resolution structure. Although there is a
                    // Structure.ConvertToHighResolution() method, it did not work on "structure". There is no method to convert "prv"
                    // to the default resolution, hence the hack fix above.
                    structure.SegmentVolume = structure.Sub(prv);
                }


            }


        }
    }
}