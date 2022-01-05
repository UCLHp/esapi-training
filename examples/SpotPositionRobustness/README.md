# SpotPositionRobustness

Script to copy the current plan, apply some Gaussian error to each spot position and recalculate the dose.  

Specify the mean and standard deviation of the Gaussian error, or use the GetMeanShift() methods to use the gantry angle and energy-dependent mean X and Y displacements estimated from our xrv124 QA measurements on Gantry 1.

## Learning points
- Some weird issues with copying PlanSetup / IonPlanSetup objects. Must use the plan.Course.CopyPlanSetup() method and cast the PlanSetup object returned to an IonPlanSetup object