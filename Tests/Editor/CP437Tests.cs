using NUnit.Framework;
using RLTK;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static RLTK.CodePage437;

[TestFixture]
public class CP437Tests
{
    [Test]
    public void TestHello()
    {
        byte[] hello = new byte[] { 72, 101, 108, 108, 111 };
        var convert = StringToCP437Alloc("Hello");

        Assert.AreEqual(hello, convert);
    }

    [Test]
    public void TestFancy()
    {
        byte[] test = new byte[] { 171, 165, 176, 206, 234, 247 };
        var convert = StringToCP437Alloc("½Ñ░╬Ω≈");

        Assert.AreEqual(test, convert);
    }

    [Test]
    public void TestFirstGroup()
    {
        var test = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        var convert = StringToCP437Alloc("☺☻♥♦♣♠•◘○◙♂♀♪♫☼");

        Assert.AreEqual(test, convert);
    }

    [Test]
    public void TestRange()
    {
        for (int i = 0; i < 256; ++i)
        {
            var ch = ToChar((byte)i);
            var by = ToCP437(ch);
            var convertedCH = ToChar(by);
            var convertedBY = ToCP437(ch);

            Assert.AreEqual(ch, convertedCH);
            Assert.AreEqual(by, convertedBY);
        }
    }

    [Test]
    public void TestConvert()
    {
        {
            string str = "Hello";
            var convertedBytes = StringToCP437(str, Allocator.Temp);
            var convertedString = CP437ToString(convertedBytes);
            Assert.AreEqual(convertedString, str);
        }

        {
            NativeArray<byte> bytes = new NativeArray<byte>(5, Allocator.Temp);
            //72, 101, 108, 108, 111
            bytes[0] = 72;
            bytes[1] = 101;
            bytes[2] = 108;
            bytes[3] = 108;
            bytes[4] = 111;

            var convertedString = CP437ToString(bytes);
            var convertedBytes = StringToCP437(convertedString, Allocator.Temp);

            for (int i = 0; i < bytes.Length; ++i)
                Assert.AreEqual(bytes[i], convertedBytes[i]);
        }
    }
}
