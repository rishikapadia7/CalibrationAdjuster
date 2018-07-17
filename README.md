# CalibrationAdjuster
Obtain eye-tracking 9-point calibration adjustments based on second-stage calibration offsets.

## How to use
This is a quick and dirty implementation where the adjustment offsets is a public array of type Point.
By default, adjGridOffset[] has all 9 values as zero.  The (x,y) location for each index can be found in
adjGrid[].  You want to populate adjGridOffset[i] with an adjustment on the screen for the calibration
reference point i.  It essentially describes how many pixels x, and how many pixels y do I shift the 
effective gaze point from the supplied gaze point by the device.
