using System.Windows;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Collections.Generic;
using System;

/*
 * Script to analyze the spot list of each beam in the current plan.
 * Highly modulated fields / highly weighted spots will be highlighted for inspection
 * 
 * 
 * Recall that spotMU=spotWeight*(beamMeterset/cumulativeMeterset)
 */

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
            foreach (IonBeam beam in context.IonPlanSetup.IonBeams)
            {

                String BEAM_MESSAGE = "";

                var beamID = beam.Id;
                //string beamName = beam.Name;
                var beamMeterset = beam.Meterset.Value;
                var totalCumulativeCPWeight = beam.IonControlPoints.Max(icp => icp.MetersetWeight);
                var beamWeight = beam.WeightFactor;
                var controlPoints = beam.IonControlPoints;
                var numCPs = controlPoints.Count/2;
                var maxBeamSpotWeight = beam.IonControlPoints.Max(ss => ss.FinalSpotList.Max(a => a.Weight));  //i.e. maximum of the mamximum?
                var maxBeamSpotMU = maxBeamSpotWeight * (beamMeterset / totalCumulativeCPWeight);
                //var avgBeamSpotWeight = beam.IonControlPoints.Average(ss => ss.FinalSpotList.Average(a => a.Weight));  // this would be average of the averages?

                MessageBox.Show("\tBeam: " + beamID +
                                "\n\nMeterset = " + System.Math.Round(beamMeterset, 2) + 
                                "\nCumulativeCPWeight = " + System.Math.Round(totalCumulativeCPWeight, 2) + 
                                "\nBeamWeightFactor = " + beamWeight +
                                "\nNumber of CPs = " + numCPs +
                                "\nMax beam spot MU = " + System.Math.Round(maxBeamSpotMU,2)
                                );

                
                foreach( IonControlPoint cp in beam.IonControlPoints)
                {             

                    // Only take even (start) CPs
                    if( cp.Index %2 == 0)
                    {

                        var energy = cp.NominalBeamEnergy;
                        var ga = cp.GantryAngle;
                        var spotList = cp.FinalSpotList;
                        var numSpots = spotList.Count;
                      
                        var max_CP_MU = spotList.Max(s => s.Weight) * (beamMeterset / totalCumulativeCPWeight);
                        var avg_CP_MU = spotList.Average(s => s.Weight) * (beamMeterset / totalCumulativeCPWeight);

                        // Get list of spot MUs
                        List<double> cpSpotMUs = new List<double>();

                        foreach ( var spot in spotList)
                        {
                            var weight = spot.Weight;
                            var pos = spot.Position;
                            // MU not directly accessible; calc from weight
                            var mu = weight * (beamMeterset / totalCumulativeCPWeight);
                            cpSpotMUs.Add(mu);
                        }

                        // Order spots by MUs lowest to highest
                        cpSpotMUs.Sort();
                        //MessageBox.Show(String.Join("\n",cpSpotMUs));

                        // Warn if there's a single spot 100% hotter than 2nd hottest
                        //MessageBox.Show(cpSpotWeights.Last() + "  " + 1 * cpSpotWeights[cpSpotWeights.Count - 2]);
                        //if ( cpSpotWeights.Last() > 2*cpSpotWeights[ cpSpotWeights.Count-2 ] )
                        if (cpSpotMUs.Last()>100  && cpSpotMUs.Last()>4*avg_CP_MU)
                        {
                            //MessageBox.Show("Warning: Hot spot in beam "+beamID+" at E = "+energy);
                            BEAM_MESSAGE += "\nWarning: Hot spot in beam " + beamID + " at E = " + energy;
                       }

                    }

                }


                if( BEAM_MESSAGE != "")
                {
                    MessageBox.Show(BEAM_MESSAGE);
                }




            }


        }
    }
}