namespace CalibrationAdjuster
{

    /*
        MIT License
        Copyright 2018 Rishi Kapadia

        https://github.com/rishikapadia7/CalibrationAdjuster

        Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
        The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */
    public static class CalibrationAdjusterStatic
    {
        public static bool isInitialized = false; //gets set to true when the arrays have been initialized
        public static Point[] adjGrid; //the points at which the adjustment grid offsets are recorded
        public static Point[] adjGridOffset; //the actual offset applied at each of the adjGrid points

        private static Point GetCornerOffset(Point p, int a)
        {
            if (a < adjGridOffset.Length)
            {
                return adjGridOffset[a];
            }
            else
            {
                return new Point(0, 0);
            }

        }

        private static Point GetSideOffset(Point p, int a, int b)
        {
            if (a >= adjGridOffset.Length || b >= adjGridOffset.Length)
            {
                return new Point(0, 0);
            }

            double da, db, dtotal, wa, wb, wtotal;
            da = PointHelper.GetPointDistance(p, adjGrid[a]);
            db = PointHelper.GetPointDistance(p, adjGrid[b]);
            dtotal = da + db;

            wa = dtotal - da;
            wb = dtotal - db;
            wtotal = wa + wb;


            //Apply weighted sum of a and b offsets
            return new Point(Convert.ToInt16(adjGridOffset[a].X * (wa / wtotal) + adjGridOffset[b].X * (wb / wtotal)),
                Convert.ToInt16(adjGridOffset[a].Y * (wa / wtotal) + adjGridOffset[b].Y * (wb / wtotal)));
        }

        public static int IndexOfMinDistance(List<double> distances)
        {
            if (distances.Count < 1)
            { return 0; }

            int currentMinIndex = 0;
            double currentMinVal = distances[0];
            for (int i = 1; i < distances.Count; i++)
            {
                if (currentMinVal > distances[i])
                {
                    currentMinIndex = i;
                    currentMinVal = distances[i];
                }
            }
            return currentMinIndex;
        }


        //a and b are same horizontally, and     c and d are same horizontally
        private static Point GetCentralOffset(Point p, int a, int b, int c, int d)
        {
            if (a >= adjGridOffset.Length || b >= adjGridOffset.Length || c >= adjGridOffset.Length || d >= adjGridOffset.Length)
            {
                return new Point(0, 0);
            }

            double da, db, dc, dd, dtotal, wa = 0, wb = 0, wc = 0, wd = 0;
            da = PointHelper.GetPointDistance(p, adjGrid[a]);
            db = PointHelper.GetPointDistance(p, adjGrid[b]);
            dc = PointHelper.GetPointDistance(p, adjGrid[c]);
            dd = PointHelper.GetPointDistance(p, adjGrid[d]);
            dtotal = da + db + dc + dd;

            Point A = adjGrid[a];
            Point B = adjGrid[b];
            Point C = adjGrid[c];
            Point D = adjGrid[d];

            List<Point> corners = new List<Point>() { A, B, C, D };

            List<double> distances = new List<double>();
            double distanceSum = 0;
            //compute distances
            for (int i = 0; i < corners.Count; i++)
            {
                Point corner = corners[i];
                double distance = PointHelper.GetPointDistance(p, corner);
                distances.Add(distance);
                distanceSum += distance;
            }

            double fractionLeft = 1; //100%
            while (corners.Count > 0)
            {
                int currentMinIndex = IndexOfMinDistance(distances);

                //1 - (di / dt) * (n - 1)
                double contribution = 1 - (distances[currentMinIndex] / distanceSum) * (distances.Count - 1);

                if (corners[currentMinIndex] == A)
                {
                    wa = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == B)
                {
                    wb = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == C)
                {
                    wc = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == D)
                {
                    wd = fractionLeft * contribution;
                }

                fractionLeft -= contribution * fractionLeft;
                distanceSum -= distances[currentMinIndex];
                distances.RemoveAt(currentMinIndex);
                corners.RemoveAt(currentMinIndex);
            }

            //Apply weighted sum of offsets
            return new Point(
                Convert.ToInt16(adjGridOffset[a].X * wa
                + adjGridOffset[b].X * wb
                + adjGridOffset[c].X * wc
                + adjGridOffset[d].X * wd),
                Convert.ToInt16(
                    adjGridOffset[a].Y * wa
                    + adjGridOffset[b].Y * wb
                    + adjGridOffset[c].Y * wc
                    + adjGridOffset[d].Y * wd
            ));
        }


        public static Point GetCalibrationAdjustment(Point p)
        {
            Point adj = new Point(0, 0);
            if (!isInitialized) return adj;

            //assume user has done a 9 point fixed calibration
            //assume the corners that are calibrated to are fairly representative of the extremties towards the screen edge

            /*
           index corresponds to following visual locations
              0     1      2
              3     4      5
              6     7      8

               (point 0,0) is top left corner.
           */

            //horizontally before first column
            if (p.X <= adjGrid[0].X)
            {
                if (p.Y <= adjGrid[0].Y)
                {
                    //corner apply 0's offset
                    adj = GetCornerOffset(p, 0);
                }
                else if (p.Y < adjGrid[3].Y)
                {
                    //some of 0 and 3
                    adj = GetSideOffset(p, 0, 3);
                }
                else if (p.Y < adjGrid[6].Y)
                {
                    //some of 3 and 6
                    adj = GetSideOffset(p, 3, 6);
                }
                else
                {
                    //corner of 6
                    adj = GetCornerOffset(p, 6);
                }
            }
            //horizontally between first and second column
            else if (p.X <= adjGrid[1].X)
            {
                if (p.Y <= adjGrid[1].Y)
                {
                    //some of 0 and 1
                    adj = GetSideOffset(p, 0, 1);
                }
                else if (p.Y < adjGrid[4].Y)
                {
                    //some of 0,1,3,4
                    adj = GetCentralOffset(p, 0, 1, 3, 4);
                }
                else if (p.Y < adjGrid[7].Y)
                {
                    //some of 3,4,6,7
                    adj = GetCentralOffset(p, 3, 4, 6, 7);
                }
                else
                {
                    //some of 6,7
                    adj = GetSideOffset(p, 6, 7);
                }
            }
            //horizontally between second and third column
            else if (p.X < adjGrid[2].X)
            {
                if (p.Y < adjGrid[2].Y)
                {
                    //some of 1 and 2
                    adj = GetSideOffset(p, 1, 2);
                }
                else if (p.Y < adjGrid[5].Y)
                {
                    //some of 1,2,4,5
                    adj = GetCentralOffset(p, 1, 2, 4, 5);
                }
                else if (p.Y < adjGrid[8].Y)
                {
                    //some of 4,5,7,8
                    adj = GetCentralOffset(p, 4, 5, 7, 8);
                }
                else
                {
                    //some of 7,8
                    adj = GetSideOffset(p, 7, 8);

                }
            }
            //after 3rd column
            else
            {
                if (p.Y <= adjGrid[2].Y)
                {
                    //corner apply 2's offset
                    adj = GetCornerOffset(p, 2);
                }
                else if (p.Y < adjGrid[5].Y)
                {
                    //some of 2 and 5
                    adj = GetSideOffset(p, 2, 5);
                }
                else if (p.Y < adjGrid[8].Y)
                {
                    //some of 5 and 8
                    adj = GetSideOffset(p, 5, 8);

                }
                else
                {
                    //corner of 8
                    adj = GetCornerOffset(p, 8);
                }
            }

            return adj;
        }
    }

    public static class PointHelper
    {
        //Get euclidean distance between points a nd b via pythagarus
        public static int GetPointDistance(Point a, Point b)
        {
            return (int)Math.Round(Math.Sqrt(Math.Pow(Math.Abs(a.X - b.X), 2) + Math.Pow(Math.Abs(a.Y - b.Y), 2)));
        }

        public static bool isNearPoint(Point a, Point b, int threshold)
        {
            return GetPointDistance(a, b) < threshold;
        }
    }
}
