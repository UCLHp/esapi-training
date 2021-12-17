using System.Windows;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

/*
 * Script will take the first beam in the plan and generate a
 * 10x10cm spot grid with 2.5mm separation and 10 MU per spot
 * 
 */

[assembly: ESAPIScript(IsWriteable = true)]
namespace VMS.TPS
{
    class Script
    {


        public Script()
        {
        }
        public void Execute(ScriptContext context)  //, System.Windows.Window window) // will open up a big empty window
        {

            Patient patient = context.Patient;
            patient.BeginModifications();


            IonBeam beam = context.IonPlanSetup.IonBeams.First();

            // Need these to convert between spot weight and MU
            var beamMeterset = beam.Meterset.Value;
            var totalCumulativeCPWeight = beam.IonControlPoints.Max(icp => icp.MetersetWeight);
            // spot MU = ionbeamMU * spotWeight / cumulativeIonCPWeight
            // -->  spotWeight = spot MU * cumulativeIonCPWeight / ionbeamMU 
            //MessageBox.Show("beam.Meterset.Value = " + beamMeterset + "\ntotalCumCPWeight = " + totalCumulativeCPWeight);

            IonBeamParameters beamParams = beam.GetEditableParameters();
            IonControlPointPairCollection cpList = beamParams.IonControlPointPairs;

            // Set spot list sizes
            var cp = 0;
            foreach (IonControlPointPair icpp in cpList)
            {
                cp += 1;
                if (cp==1)
                {
                    icpp.ResizeRawSpotList(1681);
                    //icpp.NominalBeamEnergy = 100;  
                    // read only property; set it in Beam Line > Edit > Beam Line Modifiers
                    
                    var strt_x = -50.0;
                    var strt_y = -50.0;
                    var spacing = 2.5;
                    int spotsPerRowCol = 41;
                    int spotCount = 0;
                    //Create grid
                    // Only edit the first spot and set all others to zero
                    IonSpotParametersCollection rawSpotList = icpp.RawSpotList;
                    foreach (IonSpotParameters spot in rawSpotList)
                    {
                        var row = spotCount / spotsPerRowCol;
                        var col = spotCount % spotsPerRowCol;

                        var xPos = strt_x + row * spacing;
                        var yPos = strt_y + col * spacing;
                        
                        // set weight to give desired MU
                        var spotMU = 10;
                        spot.Weight = (float)(spotMU * totalCumulativeCPWeight / beamMeterset);
                        spot.X = (float)xPos;       // set X position of scanning spot
                        spot.Y = (float)yPos;       // set Y position of scanning spot
                        
                        spotCount += 1;
                    }
                }
                else
                {
                    //icpp.ResizeRawSpotList(0);
                    // Need to just set all weights to zero to remove the control point
                    IonSpotParametersCollection rawSpotList = icpp.RawSpotList;
                    foreach (IonSpotParameters spot in rawSpotList)
                    {
                        spot.Weight = 0;
                        spot.X = (float)0.0;       // set X position of scanning spot
                        spot.Y = (float)0.0;       // set Y position of scanning spot                                                  
                    }
                }



            }


            // Apply scan spot changes to Eclipse
            beam.ApplyParameters(beamParams);




             

            // Check final number of spots
            IonBeamParameters beamParams2 = beam.GetEditableParameters();
            IonControlPointPairCollection cpList2 = beamParams2.IonControlPointPairs;
            // Count spots
            var nspots = 0;
            //var ncps = 0;
            foreach (IonControlPointPair icpp in cpList2)
            {
                IonSpotParametersCollection rawSpotList = icpp.RawSpotList;
                nspots += rawSpotList.Count;
                //ncps += 1;
            }
            MessageBox.Show( "Total final spots = " + nspots.ToString()  );

            


        }
    }
}