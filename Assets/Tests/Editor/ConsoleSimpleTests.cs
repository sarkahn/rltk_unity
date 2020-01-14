using UnityEngine;
using System.Collections;
using NUnit.Framework;

using static RLTK.CodePage437;
using Unity.Collections;
using RLTK;

[TestFixture]
public class ConsoleSimpleTests
{
    [Test]
    public void WriteRead()
    {
        int w = 40;
        int h = 15;
        var console = new SimpleConsole(w, h, null, new Mesh());

        console.Print(0, 0, "Hello");
        var tiles = console.ReadTiles(0, 0, 5, Allocator.Temp);

        Assert.AreEqual('H', ToChar(tiles[0].glyph));
        Assert.AreEqual('e', ToChar(tiles[1].glyph));
        Assert.AreEqual('l', ToChar(tiles[2].glyph));
        Assert.AreEqual('l', ToChar(tiles[3].glyph));
        Assert.AreEqual('o', ToChar(tiles[4].glyph));

        console.Dispose();
    }

    [Test]
    public void WriteAllReadAll()
    {
        int w = 40;
        int h = 15;
        var console = new SimpleConsole(w, h, null, new Mesh());

        var tiles = new NativeArray<Tile>(w * h, Allocator.Temp);

        var exclamation = ToCP437('!');

        for( int i = 0; i < tiles.Length; ++i )
        {
            var t = tiles[i];
            t.glyph = exclamation;
            tiles[i] = t;
        }

        console.WriteAllTiles(tiles);

        var copy = console.ReadAllTiles(Allocator.Temp);

        for (int i = 0; i < copy.Length; ++i)
            Assert.AreEqual(exclamation, copy[i].glyph);
        
        console.Dispose();
    }

    [Test]
    public void WriteAtReadAt()
    {
        int w = 40;
        int h = 15;
        var console = new SimpleConsole(w, h, null, new Mesh());

        console.Print(5, 5, "Hello");

        var tiles = console.ReadTiles(5, 5, 5, Allocator.Temp);

        Assert.AreEqual('H', ToChar(tiles[0].glyph));
        Assert.AreEqual('e', ToChar(tiles[1].glyph));
        Assert.AreEqual('l', ToChar(tiles[2].glyph));
        Assert.AreEqual('l', ToChar(tiles[3].glyph));
        Assert.AreEqual('o', ToChar(tiles[4].glyph));

        console.Dispose();
    }

    [Test]
    public void PrintColor()
    {
        int w = 40;
        int h = 15;
        var console = new SimpleConsole(w, h, null, new Mesh());

        string str = "Hello";

        console.PrintColor(5, 5, str, Color.red, Color.blue);

        var tiles = console.ReadTiles(5, 5, 5, Allocator.Temp);

        for (int i = 0; i < 5; ++i)
        {
            Assert.AreEqual(str[i], ToChar(tiles[i].glyph));
            Assert.AreEqual(Color.red, tiles[i].fgColor);
            Assert.AreEqual(Color.blue, tiles[i].bgColor);
        }

        console.Dispose();
    }

    [Test]
    public void Resize()
    {
        int w = 40;
        int h = 15;
        var console = new SimpleConsole(w, h, null, new Mesh());

        Assert.AreEqual(40, console.Width);
        Assert.AreEqual(15, console.Height);

        console.Resize(10, 10);

        Assert.AreEqual(10, console.Width);
        Assert.AreEqual(10, console.Height);

        Assert.Throws<System.IndexOutOfRangeException>(
            ()=>console.Set(10, 10, Color.white, Color.black, 2));

        console.Dispose();
    }


}
