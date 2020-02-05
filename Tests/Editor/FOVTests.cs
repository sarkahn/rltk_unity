using NUnit.Framework;
using RLTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using RLTK.FieldOfView;
using RLTK.FieldOfView.Bresenham;
using RLTK.NativeContainers;
using Unity.Collections.LowLevel.Unsafe;

[TestFixture]
public class FOVTests
{
    public struct TestMap : IVisibilityMap, IDisposable
    {
        int width;
        int height;
        public NativeArray<bool> opaquePoints;
        public NativeList<int2> visiblePoints;

        public TestMap(int width, int height, Allocator allocator, params int2[] opaquePoints)
        {
            this.width = width;
            this.height = height;
            this.opaquePoints = new NativeArray<bool>(width * height, allocator);
            this.visiblePoints = new NativeList<int2>(width * height / 2, allocator);
            foreach (var p in opaquePoints)
                this.opaquePoints[p.y * width + p.x] = true;
        }
        
        public bool IsInBounds(int2 p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

        public bool IsOpaque(int2 p) => opaquePoints[p.y * width + p.x];

        public void SetVisible(int2 p) => visiblePoints.Add(p);

        public float Distance(int2 a, int2 b) => math.distance(a, b);

        public void Dispose()
        {
            opaquePoints.Dispose();
            visiblePoints.Dispose();
        }
    }

    [Test]
    public void TestFOV()
    {
        var map = new TestMap(20, 20, Allocator.TempJob,
            new int2(1, 1),
            new int2(2, 1));

        FOV.Compute(0, 5, map);

        var points = map.visiblePoints;

        Assert.False(points.Contains(new int2(3, 3)));
        Assert.True(points.Contains(new int2(2, 1)));
        //Assert.True(points.Contains(new int2(1, 1)));
        
        map.Dispose();
    }

    [BurstCompile]
    struct FOVJob : IJob
    {
        public int2 origin;
        public int range;
        public TestMap map;

        public void Execute()
        {
            FOV.Compute(origin, range, map);
        }
    }

    [Test]
    public void UseFOVInsideJob()
    {
        var map = new TestMap(20, 20, Allocator.TempJob,
        new int2(1, 1),
        new int2(2, 1));
        
        new FOVJob
        {
            origin = 0,
            range = 5,
            map = map,
        }.Schedule().Complete();

        var points = map.visiblePoints;

        //Assert.False(points.Contains(new int2(3, 3)));
        //Assert.True(points.Contains(new int2(2, 1)));
        //Assert.True(points.Contains(new int2(1, 1)));
        
        map.Dispose();
    }

    [Test]
    public void ViewshedContainsNoDuplicates()
    {
        var map = new TestMap(20, 20, Allocator.TempJob,
        new int2(1, 1),
        new int2(2, 1));

        FOV.Compute(0, 5, map);

        var points = map.visiblePoints;
        Assert.AreEqual(points.Length, points.ToArray().Distinct().ToArray().Length);

        map.Dispose();
    }



}
