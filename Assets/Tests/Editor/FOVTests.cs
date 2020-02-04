using NUnit.Framework;
using RLTK;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[TestFixture]
[BurstCompile]
public class FOVTests
{
    public struct TestMap : FOV.IVisibilityMap, IDisposable
    {
        int width;
        int height;
        public NativeArray<bool> opaquePoints;

        public TestMap(int width, int height, Allocator allocator, params int2[] opaquePoints)
        {
            this.width = width;
            this.height = height;
            this.opaquePoints = new NativeArray<bool>(width * height, allocator);
            foreach (var p in opaquePoints)
                this.opaquePoints[p.y * width + p.x] = true;
        }
        
        public bool IsInBounds(int2 p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

        public bool IsOpaque(int2 p) => opaquePoints[p.y * width + p.x];

        public void Dispose() => opaquePoints.Dispose();
    }

    [Test]
    public void TestFOV()
    {
        var map = new TestMap(20, 20, Allocator.TempJob,
            new int2(1, 1),
            new int2(2, 1));

        var points = FOV.GetVisiblePoints(0, 5, map);

        Assert.False(points.Contains(new int2(3, 3)));
        Assert.True(points.Contains(new int2(2, 1)));
        Assert.True(points.Contains(new int2(1, 1)));

        points.Dispose();
        map.Dispose();
    }
}
