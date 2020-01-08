
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

        public bool IsDirty { get; private set; }

        public int2 Size { get; private set; }

        public int Width => Size.x;
        public int Height => Size.y;

        public int CellCount => Size.x * Size.y;

        
        public int2 PixelsPerUnit => _backend.PixelsPerUnit;

        IConsoleBackend _backend;
        NativeArray<Tile> _tiles;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int At(int x, int y) => y * Height + x;

        JobHandle _tileJobs;

        public SimpleConsole(int width, int height, IConsoleBackend backend, Allocator allocator)
        {
            Size = new int2(width, height);
            _tiles = new NativeArray<Tile>(CellCount, allocator);
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

            _tileJobs = new ClearJob
            {
                tiles = _tiles
            }.Schedule(_tiles.Length, 64, _tileJobs);
        }

        public JobHandle ScheduleClearScreen(JobHandle inputDeps)
        {
            IsDirty = true;

            _tileJobs = new ClearJob
            {
                tiles = _tiles
            }.Schedule(_tiles.Length, 64, _tileJobs);

            return _tileJobs;
        }

        public void Print(int x, int y, string str)
        {
            IsDirty = true;

            var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

            _tileJobs = new WriteStringJob
            {
                bytes = bytes,
                pos = new int2(x, y),
                tiles = _tiles,
                width = Size.x,
            }.Schedule(_tileJobs);
        }

        public void PrintColor(int x, int y, string str, Color fgColor, Color bgColor)
        {
            IsDirty = true;

            var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

            _tileJobs = new WriteStringJobWithColors
            {
                bytes = bytes,
                pos = new int2(x, y),
                tiles = _tiles,
                width = Size.x,
                fgColor = fgColor,
                bgColor = bgColor
            }.Schedule(_tileJobs);
        }

        /// <summary>
        /// Schedule jobs to update internal render data if our tile data has changed.
        /// </summary>
        public void RebuildIfDirty()
        {
            if (IsDirty)
            {
                //Debug.Log("Writing to backbuffer");
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

        public void DrawBox(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBox(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void FillRegion(IntRect r, byte glyph, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public byte? Get(int x, int y)
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

        public void Set(int x, int y, Color fgColor, Color bgColor, byte glyph)
        {
        }


        public void Dispose()
        {
            _tileJobs.Complete();

            _backend?.Dispose();

            if (_tiles.IsCreated)
                _tiles.Dispose();

        }

        public NativeArray<Tile> CopyTiles(Allocator allocator)
        {
            return new NativeArray<Tile>(_tiles, allocator);
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
            
            //Debug.Log("SCHEDULING WRITE JOB");
            _tileJobs = new CopyTilesJob
            {
                source = input,
                dest = _tiles,
            }.Schedule(_tileJobs);
            
            IsDirty = true;

            return _tileJobs;
        }

        #region Jobs

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

        [BurstCompile]
        struct WriteStringJobWithColors : IJob
        {
            public int2 pos;
            public int width;

            public Color fgColor;
            public Color bgColor;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<byte> bytes;

            public NativeArray<Tile> tiles;

            public void Execute()
            {
                //int index = (height - 1 - pos.y * width) + pos.x;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    int index = pos.y * width + pos.x;
                    if (index >= 0 && index < tiles.Length)
                    {
                        var t = tiles[index];
                        t.glyph = bytes[i];
                        if( fgColor != default )
                            t.fgColor = fgColor;
                        if( bgColor != default )
                            t.bgColor = bgColor;
                        tiles[index] = t;
                    }
                    else
                        return;

                    ++pos.x;

                    if (pos.x >= width)
                    {
                        pos.x = 0;
                        pos.y--;
                    }
                }
            }
        }


        [BurstCompile]
        struct WriteStringJob : IJob
        {
            public int2 pos;
            public int width;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<byte> bytes;

            public NativeArray<Tile> tiles;

            public void Execute()
            {
                //int index = (height - 1 - pos.y * width) + pos.x;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    int index = pos.y * width+ pos.x;
                    if (index >= 0 && index < tiles.Length)
                    {
                        var t = tiles[index];
                        t.glyph = bytes[i];
                        tiles[index] = t;
                    }
                    else
                        return;

                    ++pos.x;

                    if (pos.x >= width)
                    {
                        pos.x = 0;
                        pos.y--;
                    }
                }
            }
        }

        //[BurstCompile]
        //struct WriteTilesJob : IJob
        //{
        //    [ReadOnly]
        //    public NativeArray<int2> positions;
        //    [ReadOnly]
        //    public NativeArray<Tile> sourceTiles;
            
        //    [WriteOnly]
        //    public NativeArray<Tile> destTiles;

        //    public int width;
        //    public int height;

        //    public void Execute()
        //    {
        //        for( int sourceIndex = 0; sourceIndex < sourceTiles.Length; ++sourceIndex )
        //        {
        //            var p = positions[sourceIndex];
        //            int mapIndex = p.y * width + p.x;
        //            destTiles[mapIndex] = sourceTiles[sourceIndex];
        //        }
        //    }
        //}

        #endregion

    }
}