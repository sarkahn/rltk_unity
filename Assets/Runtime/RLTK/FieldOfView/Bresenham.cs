
using System;
using RLTK.NativeContainers;
using Unity.Collections;
using Unity.Mathematics;

namespace RLTK.FieldOfView.Bresenham
{
    public static class FOV
    {


        public static void Compute<T>(int2 origin, int range, T visibilityMap) where T : IVisibilityMap
        {
            NativeHashSet<int2> pointSet = new NativeHashSet<int2>((range * 2) * (range * 2), Allocator.Temp);
            BuildVisibleSet(origin, range, visibilityMap, pointSet);
            var enumerator = pointSet.GetEnumerator();
            while (enumerator.MoveNext())
                visibilityMap.SetVisible(enumerator.Current);
        }

        static void BuildVisibleSet<T>(int2 origin, int range, T visibilityMap, NativeHashSet<int2> buffer)
    where T : IVisibilityMap
        {
            BresenhamCircle circle = new BresenhamCircle(origin, range);
            var points = circle.GetPoints(Allocator.Temp);
            for (int i = 0; i < points.Length; ++i)
            {
                var p = points[i];

                ScanFOVLine(origin, p, visibilityMap, buffer);
            }
        }

        static void ScanFOVLine<T>(int2 start, int2 end, T map, NativeHashSet<int2> pointSet) where T : IVisibilityMap
        {
            var line = new VectorLine(start, end);
            var linePoints = line.GetPoints(Allocator.Temp);
            for (int i = 0; i < linePoints.Length; ++i)
            {
                var p = linePoints[i];

                if (!map.IsInBounds(p))
                    return;

                pointSet.TryAdd(p);

                if (map.IsOpaque(p))
                    return;
            }
        }


    }
    
}