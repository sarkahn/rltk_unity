
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK
{
    public static class FOV
    {
        public interface IVisibilityMap
        {
            bool IsOpaque(int2 p);
            bool IsInBounds(int2 p);
        }

        public static NativeList<int2> GetVisiblePoints<T>(int2 origin, int range, T visibilityMap) where T : IVisibilityMap
        {
            NativeList<int2> points = new NativeList<int2>((range * 2) * (range * 2), Allocator.TempJob);

            new FOVJob<T>
            {
                origin = origin,
                range = range,
                visiblePoints = points,
                map = visibilityMap
            }.Run();
            
            return points;
        }

        public static JobHandle ScheduleGetVisiblePoints<T>(int2 origin, int range, T visibilityMap, NativeList<int2> pointsBuffer, JobHandle inputDeps)
            where T : IVisibilityMap
        {
            return new FOVJob<T>
            {
                origin = origin,
                range = range,
                visiblePoints = pointsBuffer,
                map = visibilityMap
            }.Schedule(inputDeps);
        }


        [BurstCompile]
        struct FOVJob<T> : IJob where T : IVisibilityMap
        {
            public T map;

            public NativeList<int2> visiblePoints;

            public int2 origin;
            public int range;

            public void Execute()
            {
                BresenhamCircle circle = new BresenhamCircle(origin, range);
                var points = circle.GetPoints(Allocator.Temp);
                for( int i = 0; i < points.Length; ++i )
                {
                    var p = points[i];

                    ScanFOVLine(origin, p, map, visiblePoints);
                }
            }
        }
        
        static void ScanFOVLine<T>(int2 start, int2 end, T map, NativeList<int2> visiblePoints) where T : IVisibilityMap
        {
            var line = new VectorLine(start, end);
            var linePoints = line.GetPoints(Allocator.Temp);
            for (int i = 0; i < linePoints.Length; ++i)
            {
                var p = linePoints[i];

                if (!map.IsInBounds(p))
                    return;

                visiblePoints.Add(p);

                if (map.IsOpaque(p))
                    return;
            }
        }

    }


}