
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

        Material Material { get; }
        
        void ClearScreen();

        void Resize(int w, int h);

        void Draw();

        int At(int x, int y);
        
        void Print(int x, int y, string str);

        NativeArray<Tile> ReadTiles(int x, int y, int len, Allocator allocator);
        NativeArray<Tile> ReadAllTiles(Allocator allocator);

        void WriteTiles(int x, int y, NativeArray<Tile> tiles);
        void WriteAllTiles(NativeArray<Tile> tiles);
        
        void PrintColor(int x, int y, string str, Color fgColor, Color bgColor);

        void Set(int x, int y, Color fgColor, Color bgColor, byte glyph);

        void DrawBox(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawHollowBox(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void DrawHollowBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor);

        void FillRegion(IntRect r, byte glyph, Color fgColor, Color bgColor);

        byte? Get(int x, int y);

        void SetMaterial(Material mat);
    }
}