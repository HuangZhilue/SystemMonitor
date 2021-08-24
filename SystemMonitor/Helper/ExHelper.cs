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

        private static float LastMaxY { get; set; } = 100f;

        public static void InsertAndMove(this PointCollection pc, DisplayItems item, LimitList<float> limitList, int index = 1, int pointNum = 1)
        {
            Point point = new(0, item.PointData * ((item.CanvasHeight - 5) / (double)item.MaxPointData));
            pc.Insert(index, point);
            pc.RemoveAt(item.DotDensity + 2);
            //Vector v = new(item.CanvasWidth / (double)item.DotDensity, 0);

            float maxY = limitList.OrderByDescending(x => x).Take(10).Average() / 0.8f;
            maxY = maxY < 1 ? 1 : maxY;
            if (Math.Abs(maxY - LastMaxY) > LastMaxY * 0.05f)
            {
                // 相差超过5%时，使用新的“最大值”
                LastMaxY = maxY;
            }
            Debug.WriteLine($"{LastMaxY}\t\t{item.PointData}");
            for (int i = pointNum; i < pc.Count - pointNum; i++)
            {
                // 0.5 = pointData / MaxY * H
                Point p = pc[i];
                float pYNum = i - pointNum < limitList.Count ? limitList.Reverse().ToList()[i - pointNum] : 0f;
                p.Y = pYNum / LastMaxY * item.CanvasHeight;
                p.X += item.CanvasWidth / (double)item.DotDensity;
                pc[i] = p;
            }
        }

        public static void SetYMax(this PointCollection pc, DisplayItems item, LimitList<float> limitList)
        {
            float maxYt = limitList.OrderByDescending(x=>x).Take(10).Average() / 0.8f;
            float maxY = (float)CommonHelper.FormatBytes(maxYt, out _, out _);
            Debug.WriteLine($"{maxYt}\t\t{maxY}");
            for (int i = 0; i < pc.Count; i++)
            {
                Point v = pc[i];
                v.Y = ChangeYByPoint((float)v.Y, maxY, item.CanvasHeight);
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