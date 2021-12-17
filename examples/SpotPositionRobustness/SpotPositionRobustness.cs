using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


[assembly: ESAPIScript(IsWriteable = true)]
namespace VMS.TPS
{
    public class Script
    {

        public Script()
        {
        }

        //G1: mean y-stdev = 0.22 ; mean x-stdev = 0.091

        public double GetMeanShiftX(double ga_wrong)
        {
            // Mean x-shift on G1 depends on gantry angle

            // I used wrong GA notation; convert here:
            double ga = (ga_wrong + 360) % 360;

            //double xshift = -0.54044 - 0.0009395 * ga + 9.123E-5 * ga * ga +
            //                    1.7791E-10 * Math.Pow(ga,3) - 2.6612E-9 * Math.Pow(ga,4) +
            //                    5.629E-12 * Math.Pow(ga,5);
            double xshift = -0.49134321749456916  +
                            -0.0009830964006037146 * ga +
                             6.830554051511614E-05 * Math.Pow(ga, 2) +
                             9.797568055893474E-08 * Math.Pow(ga, 3) +
                            -1.5167496366441241E-09 * Math.Pow(ga, 4);
            return xshift;
        }

        public double GetMeanShiftY(double energy)
        {
            // Mean y-shift on G1 depends mainly on energy
            //double yshift = -1.38867 + 0.04599 * energy - 0.0007271 * Math.Pow(energy, 2) +
            //                    4.6742E-6 * Math.Pow(energy, 3) - 9.957809E-9 * Math.Pow(energy, 4);
            double yshift = -1.3886668919043927 +
                            0.045992587194150385 * energy +
                            -0.0007271002830439536 * Math.Pow(energy, 2) +
                            4.6742306099199435E-06 * Math.Pow(energy, 3) +
                            -9.578092093939803E-09 * Math.Pow(energy, 4);
            return yshift;
        }


        public double BoxMuller(double mean, double stdDev)
        {
            // Return random spot shift from Gaussian Distribution
            Random rand = new Random();                     //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble();            //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *  Math.Sin(2.0 * Math.PI * u2);      //random normal(0,1)
            double randNormal =  mean + stdDev * randStdNormal;     //random normal(mean,stdDev^2)

            return randNormal;
        }

        public void Execute(ScriptContext context)
        {
            Patient p = context.Patient;
            if (p == null)
                throw new ApplicationException("Please load a patient");

            IonPlanSetup plan = context.IonPlanSetup;
            if (plan == null)
                throw new ApplicationException("Please load an external beam plan");

            p.BeginModifications();

            /*
            // Get or create course with Id '_SpotPosition'
            const string courseId = "_SpotPosition";
            Course course = p.Courses.Where(o => o.Id == courseId).SingleOrDefault();
            if (course == null)
            {
                course = p.AddCourse();
                course.Id = courseId;
            }
            else
            {
                MessageBox.Show("Course " + courseId + " already exists.\nExiting");
                System.Environment.Exit(1);
            }
            */

            IonPlanSetup newPlan = (IonPlanSetup)plan.Course.CopyPlanSetup(plan);        // Odd but have to cast to IonPlanSetup
            //newPlan.Name = "Test_PLAN";                                                // Doesn't work;

            foreach(IonBeam beam in newPlan.IonBeams){

                IonBeamParameters beamParams = beam.GetEditableParameters();
                IonControlPointPairCollection cpList = beamParams.IonControlPointPairs;

                double ga = beam.IonControlPoints.First().GantryAngle;

                foreach (IonControlPointPair icpp in cpList)   //Note these are pairs (we modify both even and odd CP indices)
                {

                    double energy = icpp.NominalBeamEnergy;

                    //Separate shifts in x and y
                    double meanShiftX = GetMeanShiftX(ga);
                    double meanShiftY = GetMeanShiftY(energy);

                    // Update positions in both even and odd CPs
                    foreach(var spot in icpp.RawSpotList)                         //Editing FinalSpotList does not work
                    {
                        spot.X += (float)BoxMuller(meanShiftX, 0.5);
                        spot.Y += (float)BoxMuller(meanShiftY, 0.5);
                        //spot.X += (float)BoxMuller(0, 0.5);
                        //spot.Y += (float)BoxMuller(0, 0.5);
                    }
                    
                }

                // Apply scan spot changes to Eclipse
                beam.ApplyParameters(beamParams);

            }

            newPlan.CalculateDose();
            
            // Cannot save from stand alone scripts; need an Application
        }






    }

}