using RLTK;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Consoles.Jobs
{
    public class TileJobs : MonoBehaviour
    {

        [BurstCompile]
        public struct CopyAllTilesJob : IJob
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
        public struct CopyTilesJob : IJob
        {
            [ReadOnly]
            public NativeArray<Tile> source;

            [WriteOnly]
            public NativeArray<Tile> dest;

            public int index;
            public int length;

            public void Execute()
            {
                NativeArray<Tile>.Copy(source, index, dest, 0, length);
            }
        }

        [BurstCompile]
        public struct ClearTilesJob : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<Tile> tiles;

            public void Execute(int index)
            {
                tiles[index] = Tile.EmptyTile;
            }
        }

        [BurstCompile]
        public struct WriteColoredTileGlyphsJob : IJob
        {
            public int2 pos;
            public int width;

            public Color fgColor;
            public Color bgColor;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<byte> bytes;

            public NativeArray<Tile> destination;

            public void Execute()
            {
                for (int i = 0; i < bytes.Length; ++i)
                {
                    int index = pos.y * width + pos.x;
                    if (index >= 0 && index < destination.Length)
                    {
                        var t = destination[index];
                        t.glyph = bytes[i];
                        if (fgColor != default)
                            t.fgColor = fgColor;
                        if (bgColor != default)
                            t.bgColor = bgColor;
                        destination[index] = t;
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
        public struct WriteTileGlyphsJob : IJob
        {
            public int2 pos;
            public int width;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<byte> bytes;

            public NativeArray<Tile> tiles;

            public void Execute()
            {
                for (int i = 0; i < bytes.Length; ++i)
                {
                    int index = pos.y * width + pos.x;
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
    }
}