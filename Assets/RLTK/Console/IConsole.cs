using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace RLTK.Consoles
{
    public interface IConsole
    {
        int2 Size { get; }

        int CellCount { get; }

        int2 PixelsPerUnit { get; }

        /// <summary>
        /// Update internal render data if our tile data has changed.
        /// </summary>
        void RebuildIfDirty();
        
        void ClearScreen();

        void Resize(int w, int h);

        void Draw(Font font, Material mat);

        int At(int x, int y);
        
        void Print(int x, int y, string str);

        void PrintColor(int x, int y, string str, Color fgColor, Color bgColor);

        void Set(int x, int y, Color fgColor, Color bgColor, byte glyph);

        void DrawBox(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawHollowBox(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawHollowBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void FillRegion(IntRect r, byte glyph, Color fgColor, Color bgColor);

        JobHandle ScheduleWriteTiles(NativeArray<Tile> input, JobHandle inputDeps);

        JobHandle ScheduleCopyTiles(NativeArray<Tile> buffer, JobHandle inputDeps);

        /// <summary>
        /// Immediately copy the tiles from the console. Will force a sync point.
        /// </summary>
        NativeArray<Tile> CopyTiles(Allocator allocator);

        /// <summary>
        /// Immediately write the tiles to the console. Will force a sync point.
        /// </summary>
        void WriteTiles(NativeArray<Tile> tiles);

        byte? Get(int x, int y);
    }
}