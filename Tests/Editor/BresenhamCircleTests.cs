using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[TestFixture]
public class BresenhamCircleTests
{
    [Test]
    public void TestRadius()
    {
        for( int radius = 1; radius < 200; ++radius )
        {
            var circle = new BresenhamCircle(0, radius);
            var points = circle.GetPoints(Allocator.Temp);
        }
    }

    [Test]
    public void TestRadius1()
    {
        int radius = 1;

        var circle = new BresenhamCircle(0, radius);
        var points = circle.GetPoints();

        Assert.AreEqual(4, points.Length);

        Assert.AreEqual(new int2(1, 0), points[0]);
        Assert.AreEqual(new int2(0, 1), points[1]);
        Assert.AreEqual(new int2(-1, 0), points[2]);
        Assert.AreEqual(new int2(0, -1), points[3]);

        points.Dispose();
    }

    [Test]
    public void TestRadius3()
    {
        int radius = 3;

        var circle = new BresenhamCircle(0, radius);
        var points = circle.GetPoints();

        Assert.AreEqual(16, points.Length);

        Assert.AreEqual(new int2(3, 0), points[0]);
        Assert.AreEqual(new int2(0, 3), points[1]);
        Assert.AreEqual(new int2(-3, 0), points[2]);
        Assert.AreEqual(new int2(0, -3), points[3]);
        Assert.AreEqual(new int2(3, 1), points[4]);
        Assert.AreEqual(new int2(-1, 3), points[5]);
        Assert.AreEqual(new int2(-3, -1), points[6]);
        Assert.AreEqual(new int2(1, -3), points[7]);
        Assert.AreEqual(new int2(2, 2), points[8]);
        Assert.AreEqual(new int2(-2, 2), points[9]);
        Assert.AreEqual(new int2(-2, -2), points[10]);
        Assert.AreEqual(new int2(2, -2), points[11]);
        Assert.AreEqual(new int2(1, 3), points[12]);
        Assert.AreEqual(new int2(-3, 1), points[13]);
        Assert.AreEqual(new int2(-1, -3), points[14]);
        Assert.AreEqual(new int2(3, -1), points[15]);

        points.Dispose();
    }

    [BurstCompile]
    struct CircleUsingJob : IJob
    {
        public BresenhamCircle circle;
        public NativeArray<int2> points;

        public void Execute()
        {
            var circlePoints = circle.GetPoints(Allocator.Temp);
            NativeArray<int2>.Copy(circlePoints, points);
        }
    }

    [Test]
    public void UseCircleInsideAJob()
    {
        var circle = new BresenhamCircle(0, 1);
        var points = new NativeArray<int2>(4, Allocator.TempJob);

        new CircleUsingJob
        {
            circle = circle,
            points = points
        }.Schedule().Complete();

        Assert.AreEqual(new int2(1, 0), points[0]);
        Assert.AreEqual(new int2(0, 1), points[1]);
        Assert.AreEqual(new int2(-1, 0), points[2]);
        Assert.AreEqual(new int2(0, -1), points[3]);

        points.Dispose();
    }
}