using NUnit.Framework;
using RLTK;
using RLTK.Consoles;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static RLTK.CodePage437;
using static RLTK.Consoles.Jobs.TileJobs;

[TestFixture]
public class ConsoleNativeTests
{


    [Test]
    public void ScheduleWrite()
    {
        int w = 40;
        int h = 15;
        var console = new NativeConsole(w, h);

        string str = "Hello";

        var job = console.SchedulePrint(0, 0, str);

        job.Complete();

        var tiles = console.ReadTiles(0, 0, 5, Allocator.Temp);

        for (int i = 0; i < 5; ++i)
            Assert.AreEqual(str[i], ToChar(tiles[i].glyph));

        console.Dispose();
    }

    [Test]
    public void ScheduleWriteParallelRead()
    {
        int w = 40;
        int h = 15;
        var console = new NativeConsole(w, h);

        string str = "Hello";

        var jobs = console.SchedulePrint(0, 0, str);

        NativeArray<JobHandle> readJobs = new NativeArray<JobHandle>(5, Allocator.TempJob);

        List<NativeArray<Tile>> buffers = new List<NativeArray<Tile>>();

        for( int i = 0; i < 5; ++i )
        {
            buffers.Add(new NativeArray<Tile>(5, Allocator.TempJob));
            
            var readJob = console.ScheduleReadTiles(0, 0, 5, buffers[i], jobs);

            readJobs[i] = readJob;
        }

        jobs = JobHandle.CombineDependencies(readJobs);

        readJobs.Dispose();

        jobs.Complete();

        for (int i = 0; i < 5; ++i)
        {
            Assert.AreEqual('H', ToChar(buffers[i][0].glyph));
            Assert.AreEqual('e', ToChar(buffers[i][1].glyph));
            Assert.AreEqual('l', ToChar(buffers[i][2].glyph));
            Assert.AreEqual('l', ToChar(buffers[i][3].glyph));
            Assert.AreEqual('o', ToChar(buffers[i][4].glyph));
        }

        for (int i = 0; i < buffers.Count; ++i)
            buffers[i].Dispose();
        

        console.Dispose();
    }


}
