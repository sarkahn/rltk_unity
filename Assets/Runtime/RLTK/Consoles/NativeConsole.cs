
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using RLTK.Consoles.Backend;
using static RLTK.Consoles.Jobs.TileJobs;

namespace RLTK.Consoles
{
    /// <summary>
    /// A console that exposes schedule functions for parallel operations.
    /// </summary>
    public class NativeConsole : SimpleConsole
    {
        public NativeConsole(int width, int height, Material mat = null) : base(width, height, mat)
        {}

        public JobHandle ScheduleClearScreen(JobHandle inputDeps)
        {
            _isDirty = true;

            _tileJobs = new ClearTilesJob
            {
                tiles = _tiles
            }.Schedule(_tiles.Length, 64, _tileJobs);
            
            return _tileJobs;
        }

        public JobHandle SchedulePrint(int x, int y, string str, JobHandle inputDeps = default)
        {
            _isDirty = true;

            _tileJobs = JobHandle.CombineDependencies(_tileJobs, inputDeps);

            var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

            _tileJobs = new WriteTileGlyphsJob
            {
                bytes = bytes,
                pos = new int2(x,y),
                tiles = _tiles,
                width = Size.x
            }.Schedule(_tileJobs);


            return _tileJobs;
        }

        public JobHandle SchedulePrintColor(int x, int y, string str, Color fgColor, Color bgColor, JobHandle inputDeps)
        {
            _isDirty = true;

            _tileJobs = JobHandle.CombineDependencies(_tileJobs, inputDeps);

            var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

            _tileJobs = new WriteColoredTileGlyphsJob
            {
                bytes = bytes,
                pos = new int2(x,y),
                destination = _tiles,
                width = Size.x,
                fgColor = fgColor,
                bgColor = bgColor
            }.Schedule(_tileJobs);
            
            return _tileJobs;
        }

        public JobHandle ScheduleWriteTiles(NativeArray<Tile> buffer, JobHandle inputDeps)
        {
            _isDirty = true;

            _tileJobs = JobHandle.CombineDependencies(_tileJobs, inputDeps);

            _tileJobs = new CopyAllTilesJob
            {
                source = buffer,
                dest = _tiles
            }.Schedule(_tileJobs);


            return _tileJobs;
        }

        public JobHandle ScheduleReadTiles(int x, int y, int len, NativeArray<Tile> destination, JobHandle inputDeps = default)
        {
            _tileJobs = JobHandle.CombineDependencies(_tileJobs, inputDeps);

            len = math.min(len, Size.x - x);
            int i = y * Size.x + x;
            _tileJobs = new CopyTilesJob
            {
                source = _tiles,
                dest = destination,
                index = i,
                length = len
            }.Schedule(_tileJobs);

            return _tileJobs;
        }
        
        public JobHandle ScheduleReadAllTiles(NativeArray<Tile> buffer, JobHandle inputDeps)
        {
            _tileJobs = JobHandle.CombineDependencies(inputDeps, _tileJobs);

            _tileJobs = new CopyAllTilesJob
            {
                source = _tiles,
                dest = buffer,
            }.Schedule(_tileJobs);

            return _tileJobs;
        }

        public void CompleteJobs()
        {
            _tileJobs.Complete();
        }

        /// <summary>
        /// Should be called once per frame. Updates internal render data based on tile changes.
        /// </summary>
        public override void Update()
        {
            if (_isDirty)
            {
                _isDirty = false;
                _tileJobs = _backend.ScheduleUploadTileData(_tiles, _tileJobs);
            }

            _backend.Update();
        }

    }
}