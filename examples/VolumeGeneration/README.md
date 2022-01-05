# VolumeGeneration

Testing the functionality to automate volume generation. This script will take a plan with a CTV_High and a CTV_Low target and, using the margin defined in the code, generate all ORVs from the OARs as well as all OTVs, OTV_eds, CTV_eds expanded and cropped appropriately to the ORVs, body contour etc.


## Learning points
- Bit of a hack fix required when High Resolution structures are present. Certain volume operations will not work if using a mixture of low and high resolution structures, and so we must convert them all to low (or high?). There seems to be an in-built method to convert low to high resolution, but I couldn't make it work. There is no in-built reverse operation.