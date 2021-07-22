using System.Windows;
using System.Windows.Media;
using SystemMonitor.Models;

namespace SystemMonitor.Helper
{
    public static class ExHelper
    {
        public static void ChangePoints(this PointCollection pc, Vector v, int pointNum = 0)
        {
            for (int i = pointNum; i < pc.Count - pointNum; i++)
            {
                pc[i] += v;
            }
        }

        public static void InsertAndMove(this PointCollection pc, DisplayItems item, int index = 1, int pointNum = 1)
        {
            Point p = new(0, item.PointData * ((item.CanvasHeight - 5) / (double)item.MaxPointData));
            pc.Insert(index, p);
            pc.RemoveAt(item.DotDensity + 2);
            Vector v = new(item.CanvasWidth / (double)item.DotDensity, 0);
            for (int i = pointNum; i < pc.Count - pointNum; i++)
            {
                pc[i] += v;
            }
        }

        public static void SetYMax(this PointCollection pc, DisplayItems item)
        {
            for (int i = 0; i < pc.Count; i++)
            {
                Point v = pc[i];
                v.Y = ChangeYByPoint((float)v.Y, item.MaxPointData, item.CanvasHeight);
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