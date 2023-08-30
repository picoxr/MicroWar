using UnityEngine;

namespace MicroWar.Utils
{
    public static class CurveUtils
    {
        //Modifies an existing LineRenderer to generate a curve between two given GameObjects
        public static void DrawCurveBetween(GameObject object1, GameObject object2, int curveStepCount, LineRenderer lineRenderer)
        {
            LineRenderer linkCurve = lineRenderer;
            linkCurve.positionCount = curveStepCount + 2;
            linkCurve.SetPositions(GenerateCurvePoints(object1.transform.position, object2.transform.position, curveStepCount));
        }

        public static void DrawCurveBetween(Vector3 position1, Vector3 Position2, int curveStepCount, LineRenderer lineRenderer)
        {
            LineRenderer linkCurve = lineRenderer;
            linkCurve.positionCount = curveStepCount + 2;
            linkCurve.SetPositions(GenerateCurvePoints(position1, Position2, curveStepCount));
        }

        //Generates the points on a curve between two points. 
        //For a smoother curve increase the curveStepCount
        //A curve needs at least 3 points so that this method generates the third point (midPoint) by finding
        //the middle point between two given points and increases it's height by 1.0 to create a vertical curve.
        private static Vector3[] GenerateCurvePoints(Vector3 pos1, Vector3 pos2, int curveStepCount)
        {
            int pointCount = curveStepCount + 2;
            Vector3[] points = new Vector3[pointCount];

            points[0] = pos1;
            points[pointCount - 1] = pos2;
            Vector3 midPoint = (pos1 + pos2) / 2 + Vector3.up; //Generate the third point at the middle of two given points and increase the height to make the curve vertical.


            for (int i = 1; i < pointCount - 1; i++)
            {
                points[i] = GetCurvePoint(pos1, midPoint, pos2, (float)i / curveStepCount);
            }

            return points;
        }

        //Interpolates the points of the curve.
        private static Vector3 GetCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 midpoint1 = Vector3.Lerp(p1, p2, t);
            Vector3 midpoint2 = Vector3.Lerp(p2, p3, t);
            return Vector3.Lerp(midpoint1, midpoint2, t);
        }
    }
}

