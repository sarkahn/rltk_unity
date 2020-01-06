using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

        void PrintColor(int x, int y, Color32 fgColor, Color32 bgColor, string str);

        void Set(int x, int y, Color32 fgColor, Color32 bgColor, byte glyph);

        void DrawBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor);

        void DrawHollowBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor);

        void DrawBoxDouble(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor);

        void DrawHollowBoxDouble(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor);

        void FillRegion(IntRect r, byte glyph, Color32 fgColor, Color32 bgColor);

        //void CopyTiles(NativeArray<Tile> buffer);

        //void WriteTiles(NativeArray<Tile> tiles);

        byte? Get(int x, int y);
    }
}