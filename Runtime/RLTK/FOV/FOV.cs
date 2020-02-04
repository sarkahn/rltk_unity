
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
        /// <summary>
        /// This interface can be implemented in a struct which can then be used inside an <see cref="FOVJob{T}"/>
        /// </summary>
        public interface IVisibilityMap
        {
            bool IsOpaque(int2 p);
            bool IsInBounds(int2 p);
        }

        /// <summary>
        /// <para>Utility function to make the syntax for scheduling/running an FOV job a little nicer. Usage:</para>
        /// <para>FOV.GetVisiblePointsJob(0,5,map,points).Run()</para>
        /// <para>var job = FOV.GetVisiblePointsJob(0,5,map,points); job.Schedule();</para>
        /// </summary>
        public static FOVJob<T> GetVisiblePointsJob<T>(int2 origin, int range, T visibilityMap, NativeList<int2> buffer) where T : IVisibilityMap
        {
            return new FOVJob<T>
            {
                origin = origin,
                range = range,
                map = visibilityMap,
                buffer = buffer
            };
        }

        [BurstCompile]
        public struct FOVJob<T> : IJob where T : IVisibilityMap
        {
            public T map;

            public NativeList<int2> buffer;

            public int2 origin;
            public int range;

            public void Execute()
            {
                GetVisiblePoints(origin, range, map, buffer);
            }
        }

        public static void GetVisiblePoints<T>(int2 origin, int range, T visibilityMap, NativeList<int2> pointsBuffer)
            where T : IVisibilityMap
        {
            BresenhamCircle circle = new BresenhamCircle(origin, range);
            var points = circle.GetPoints(Allocator.Temp);
            for (int i = 0; i < points.Length; ++i)
            {
                var p = points[i];

                ScanFOVLine(origin, p, visibilityMap, pointsBuffer);
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