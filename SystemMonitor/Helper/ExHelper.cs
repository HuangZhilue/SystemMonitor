using System;
using System.Diagnostics;
using System.Linq;
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

        public static float InsertAndMove(this PointCollection pc, DisplayItems item, LimitList<float> limitList, float lastMaxY, int index = 1, int pointNum = 1)
        {
            Point point = new(0, item.PointData * ((item.CanvasHeight - 5) / (double)item.MaxPointData));
            pc.Insert(index, point);
            pc.RemoveAt(item.DotDensity + 2);
            //Vector v = new(item.CanvasWidth / (double)item.DotDensity, 0);

            float maxY = limitList.OrderByDescending(x => x).Take(10).Average() / 0.8f;
            maxY = maxY < 1 ? 1 : maxY;
            if (Math.Abs(maxY - lastMaxY) > lastMaxY * 0.05f)
            {
                // 相差超过5%时，使用新的“最大值”
                lastMaxY = maxY;
            }
            //Debug.WriteLine($"{lastMaxY}\t\t{item.PointData}");
            for (int i = pointNum; i < pc.Count - pointNum; i++)
            {
                // 0.5 = pointData / MaxY * H
                Point p = pc[i];
                p.Y = limitList[i - pointNum] / lastMaxY * item.CanvasHeight;
                p.X += item.CanvasWidth / (double)item.DotDensity;
                pc[i] = p;
            }

            return lastMaxY;
        }
    }
}