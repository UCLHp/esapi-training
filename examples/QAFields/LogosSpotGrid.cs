using System.Windows;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Collections.Generic;

/*
 * Script to generate QA plans for the Logos spot grids. Grid on 3000 is 3x3,
 * while 4000 has an extra row in the +/- y-directions.
 * 
 * I couldnt get this to work when adding beams from scratch so the procedure is a little annoying.
 * Patient used is "40x40x40" on the TBOX, Course "LogosGridQA"
 *   (1) Make a new plan and set Total Dose to 1 Gy and Number of Fractions to 1.
 *   (2) Copy the beam so that you have 5 of them and use the "SC" volume as the STV. (Need to ensure 
 *       that the STV associated with each beam is not the BODY structure as this is too large and you 
 *       will get calculation errors due to the WET of the beam path).
 *   (3) Make a default energy layer on each field by right-clicking on Beam Line > Properties then
 *       the Beam Line Modifiers tab. Click the small box that says "Manually select beam line...",
 *       then the "Nominal Energy" box and input the energy. (This cannot be done via ESAPI).
 *   (4) Repeat for all fields. Energies used for QA are 70, 100, 150, 200, 240 MeV
 *   (5) Calculate dose.
 *   (6) Choose from 3000/4000 below and specify the spot per MU for each beam. Run this script.
 *   (7) Recalculate the plan, UNTICKING the Beam Line boxes.
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
        public void Execute(ScriptContext context) 
        {

            ///////////////////////////////////////////////////////////////////
            /// Select 3000/4000 and define  MUs for energies in the
            /// order: 70, 100, 150, 200, 240 MeV
            ///////////////////////////////////////////////////////////////////
            
            //string PLAN = "4000";
            string PLAN = "3000";
            List<int> beamMUs = new List<int>() { 150, 105, 70, 50, 40 };

            ///////////////////////////////////////////////////////////////////


            Patient patient = context.Patient;
            patient.BeginModifications();


            // Define spot positions dependent on Logos device
            List<float> coordsXY = new List<float>();
            int numSpots = 0;
            if (PLAN == "3000")
            {
                // List of x,y coords as in dicom
                coordsXY = new List<float>() {
                    -125.0f,-125.0f,-125.0f,0.0f,-125.0f,125.0f,
                    0.0f,125.0f,0.0f,0.0f,0.0f,-125.0f,
                    125.0f,-125.0f,125.0f,0.0f,125.0f,125.0f
                };
                numSpots = coordsXY.Count() / 2;
            }
            else if (PLAN == "4000")
            {
                coordsXY = new List<float>() {
                    -125.0f,-175.0f,-125.0f,-125.0f,-125.0f,0.0f,-125.0f,125.0f,-125.0f,175.0f,
                    0.0f,175.0f,0.0f,125.0f,0.0f,0.0f,0.0f,-125.0f,0.0f,-175.0f,
                    125.0f,-175.0f,125.0f,-125.0f,125.0f,0.0f,125.0f,125.0f,125.0f,175.0f
                };
                numSpots = coordsXY.Count() / 2;
            }


            int beamCount = -1;
            foreach (IonBeam beam in context.IonPlanSetup.IonBeams)
            {
                beamCount++;

                // Need these to convert between spot weight and MU
                //var beamMeterset = beam.Meterset.Value;
                //var totalCumulativeCPWeight = beam.IonControlPoints.Max(icp => icp.MetersetWeight);
                //
                // Note: technically the spot MU is given by:
                //       spot MU = spotWeight * (totalCumulativeCPWeight / beamMeterset)
                // but setting Total Dose to 1 Gy and having field weights all set
                // to 1 means the spot weight and MU are equivalent.

                var totalCumulativeCPWeight = beam.IonControlPoints.Max(icp => icp.MetersetWeight);
                var beamMeterset = beam.Meterset.Value;

                IonBeamParameters beamParams = beam.GetEditableParameters();
                IonControlPointPairCollection cpList = beamParams.IonControlPointPairs;

                

                // Set spot list sizes
                int cp = 0;
                foreach (IonControlPointPair icpp in cpList)
                {
                    cp += 1;
                    if (cp == 1)  //going to remove spots from all later CPs
                    {
                        icpp.ResizeRawSpotList(numSpots);

                        //Create grid
                        // Only edit the first spot and set all others to zero
                        IonSpotParametersCollection rawSpotList = icpp.RawSpotList;
                        int spotCount = 0;
                        foreach (IonSpotParameters spot in rawSpotList)
                        {
                            // set weight to give desired MU
                            var spotMU = beamMUs[beamCount];
                            spot.Weight = (float)(spotMU);   // Weight will go in as MU if Prescription is 1 Gy
                            spot.X = coordsXY[spotCount * 2];
                            spot.Y = coordsXY[spotCount * 2 + 1];

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
            }
 


        }
    }
}