using System.Windows;
using System.Windows.Media;

namespace SystemMonitor.Helper
{
    public static class ExHelper
    {
        public static void ChangePoints(this PointCollection pc, Vector v, int pointNum = 0)
        {
            for (var i = pointNum; i < pc.Count - pointNum; i++)
            {
                pc[i] += v;
            }
        }

        public static void SetYMax(this PointCollection pc, float yMax = 100f, float viewYHeight = 50)
        {
            for (var i = 0; i < pc.Count; i++)
            {
                var v = pc[i];
                v.Y = ChangeYByPoint((float) v.Y, yMax, viewYHeight);
                pc[i] += GetVectorBy2Point(pc[i], v);
            }
        }

        private static float ChangeYByPoint(float num, float maxNum, float viewYHeight = 50)
        {
            return num * (viewYHeight / maxNum);
        }

        private static Vector GetVectorBy2Point(Point a, Point b)
        {
            return new(b.X - a.X, b.Y - a.Y);
        }
    }
}