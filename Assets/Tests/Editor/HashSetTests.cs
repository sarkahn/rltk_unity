using NUnit.Framework;
using RLTK.NativeContainers;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using System.Linq;

[TestFixture]
public class HashSetTests
{
    [Test]
    public void EnumerateTest()
    {
        NativeHashSet<int2> points = new NativeHashSet<int2>(10, Allocator.Temp);

        for (int i = 0; i < 10; ++i)
            points.TryAdd(new int2(i, 0));

        
        var arr = points.ToArray();

        Assert.AreEqual(10, arr.Length);

    }
}
