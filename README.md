# CalibrationAdjuster Overview
Given an existing 9-point calibration for eye-tracking (such as Tobii's for the 4C), and also a second-stage calibration performed using a different software, this module will compute the resultant gaze point by applying an offset in x,y as determined by the second-stage calibration.

## How to use and theory of operation
adj abbreviation is for adjuster.

Point[] adjGrid is an array of size 9, with each item representing the calibration point (stimulus) location.
Given 1000x1000 demo screen,
Index 0 for example on Tobii calibration is 10% away from the top left corner of the screen (x=100,y=100).
Index 1 would be at top middle (x=500, y = 100)
...


Point[] adjGridOffset describes the shift in x and shift in y in pixels to be applied for a given adjGrid[] coordinate.
It describes the discrepancy between the Tobii original calibration and the second stage calibration for each adjGrid coordinate and stores that difference.

Method GetCalibrationAdjustment(Point p). p is the gaze point determined by Tobii SDK, and the method returns p shifted (adjusted) by a weighted sum of relevant elements in adjGridOffset.

## How weighted sum is calculated

For simplicity consider the 1 dimensional case, where you have two calibration points (stimuli location) at coordinates A and B.
A and B are separated by 10 pixels, A at x= 0, and B at x=10.
Gaze point p is located at x = 4.
Consequently, the correct way to adjust point p is by applying (60% of the adjustment offered by A since closer) + (40% of adjustmented offered at B).

The same concept is extended into 2 dimensions.  Three cases form:
1) p is at a corner of the screen, in which case it is only close to 1 reference point.
2) p is at a side of the screen, where it is influenced by the adjustment of 2 reference points.
3) p is within the interior (i.e. more than 10% away from any given edge of the screen), and it is affected by 4 reference points.

For detailed calculation, please see source code.

