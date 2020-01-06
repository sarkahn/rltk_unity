
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using RLTK.Consoles.Backend;

namespace RLTK.Consoles
{
    public class SimpleConsole : IConsole, IDisposable
    {
        IConsoleBackend _backend;

        NativeArray<Tile> _tiles;

        public bool IsDirty { get; private set; }

        public int2 Size { get; private set; }

        public int Width => Size.x;
        public int Height => Size.y;

        public int CellCount => Size.x * Size.y;

        
        public int2 PixelsPerUnit => _backend.PixelsPerUnit;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int At(int x, int y) => ((Size.y - 1 - y) * Size.x) + x;

        JobHandle _tileJobs;

        public SimpleConsole(int width, int height, IConsoleBackend backend, Allocator allocator)
        {
            Size = new int2(width, height);
            int tileCount = width * height;
            _tiles = new NativeArray<Tile>(tileCount, allocator);
            for( int i = 0; i < _tiles.Length; ++i )
            {
                _tiles[i] = new Tile
                {
                    glyph = 0,
                    fgColor = Color.white,
                    bgColor = Color.black
                };
            }

            _backend = backend;
        }

        public void ClearScreen()
        {
            IsDirty = true;

            _tileJobs.Complete();

            _tileJobs = new ClearJob
            {
                tiles = _tiles
            }.Schedule(_tiles.Length, 64, _tileJobs);
        }

        [BurstCompile]
        struct ClearJob : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<Tile> tiles;

            public void Execute(int index)
            {
                tiles[index] = Tile.Default;
            }
        }

        /// <summary>
        /// Schedule jobs to update internal render data if our tile data has changed.
        /// </summary>
        public void RebuildIfDirty()
        {
            if (IsDirty)
            {
                _tileJobs = _backend.ScheduleRebuild(Size.x, Size.y, _tiles, _tileJobs);
                IsDirty = false;
            }
        }
        
        public void Update()
        {
            _backend.Update();
        }


        public void Draw(Font font, Material mat)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBoxDouble(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBoxDouble(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void FillRegion(IntRect r, byte glyph, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public byte? Get(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void Print(int x, int y, string str)
        {
            throw new System.NotImplementedException();
        }

        public void PrintColor(int x, int y, Color32 fgColor, Color32 bgColor, string str)
        {
            throw new System.NotImplementedException();
        }
        
        public void ForceImmediateRebuild()
        {
            IsDirty = true;
            RebuildIfDirty();
        }

        public void Resize(int w, int h)
        {
            throw new System.NotImplementedException();
        }

        public void Set(int x, int y, Color32 fgColor, Color32 bgColor, byte glyph)
        {
            throw new System.NotImplementedException();
        }


        public void Dispose()
        {
            _tileJobs.Complete();

            _backend?.Dispose();

            if (_tiles.IsCreated)
                _tiles.Dispose();

        }

        public void CopyTiles(NativeArray<Tile> buffer)
        {
            _tileJobs.Complete();

            NativeArray<Tile>.Copy(_tiles, buffer);
        }

        public void WriteTiles(NativeArray<Tile> buffer)
        {
            _tileJobs.Complete();

            NativeArray<Tile>.Copy(buffer, _tiles);

            IsDirty = true;
        }
        
        public JobHandle ScheduleCopyTiles(NativeArray<Tile> buffer, JobHandle inputDeps)
        {
            _tileJobs = JobHandle.CombineDependencies(inputDeps, _tileJobs);

            _tileJobs = new CopyTilesJob
            {
                source = _tiles,
                dest = buffer,
            }.Schedule(_tileJobs);

            return _tileJobs;
        }

        public JobHandle ScheduleWriteTiles(NativeArray<Tile> input, JobHandle inputDeps)
        {
            _tileJobs = JobHandle.CombineDependencies(inputDeps, _tileJobs);
            
            _tileJobs = new CopyTilesJob
            {
                source = input,
                dest = _tiles,
            }.Schedule(_tileJobs);

            IsDirty = true;

            return _tileJobs;
        }

        [BurstCompile]
        struct CopyTilesJob : IJob
        {
            [ReadOnly]
            public NativeArray<Tile> source;

            [WriteOnly]
            public NativeArray<Tile> dest;

            public void Execute()
            {
                NativeArray<Tile>.Copy(source, dest);
            }
        }

    }
}