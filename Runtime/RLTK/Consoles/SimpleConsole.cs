using UnityEngine;
using System.Collections;
using RLTK.Consoles;
using RLTK;
using Unity.Mathematics;
using Unity.Collections;
using RLTK.Consoles.Backend;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using static RLTK.Consoles.Jobs.TileJobs;
using System.Collections.Generic;
using RLTK.Rendering;

public class SimpleConsole : IConsole
{

    public int2 Size
    {
        get;
        private set;
    }

    public int Width => Size.x;
    public int Height => Size.y;

    public Material Material { get; private set; }

    public int CellCount => Size.x * Size.y;
    
    public int2 PixelsPerUnit => RenderUtility.PixelsPerUnit(Material);
    
    protected SimpleMeshBackend _backend;
    protected NativeArray<Tile> _tiles;
    protected JobHandle _tileJobs;

    protected bool _isDirty;

    Mesh _mesh;
    public Mesh Mesh => _mesh;

    public SimpleConsole(Material mat = null)
    {
        _mesh = new Mesh();

        Material = mat == null ? RenderUtility.DefaultMaterial : mat;
    }

    /// <summary>
    /// A simple console that allows you to write Ascii to it.
    /// </summary>
    public SimpleConsole(int width, int height, Material mat = null) : this(mat)
    {
        Resize(width, height);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int At(int x, int y) => y * Size.x + x;

    public void ClearScreen()
    {
        _isDirty = true;

        _tileJobs.Complete();

        new ClearTilesJob
        {
            tiles = _tiles
        }.Run(_tiles.Length);
    }

    public void Print(int x, int y, string str)
    {
        _isDirty = true;

        _tileJobs.Complete();

        var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

        new WriteTileGlyphsJob
        {
            bytes = bytes,
            pos = new int2(x, y),
            tiles = _tiles,
            width = Size.x,
        }.Run();
    }

    public NativeArray<Tile> ReadTiles(int x, int y, int len, Allocator allocator)
    {
        NativeArray<Tile> buffer = new NativeArray<Tile>(len, allocator);

        len = math.min(len, Size.x - x);
        

        NativeArray<Tile>.Copy(_tiles, At(x,y), buffer, 0, len);

        return buffer;
    }

    public NativeArray<Tile> ReadAllTiles(Allocator allocator)
    {
        NativeArray<Tile> output = new NativeArray<Tile>(_tiles.Length, allocator);
        NativeArray<Tile>.Copy(_tiles, output);
        return output;
    }

    public void WriteTiles(int x, int y, NativeArray<Tile> tiles)
    {
        _isDirty = true;
        _tileJobs.Complete();

        int len = math.min(tiles.Length, Size.x - x);

        NativeArray<Tile>.Copy(tiles, 0, _tiles, At(x,y), len);
    }

    public void WriteAllTiles(NativeArray<Tile> tiles)
    {
        _isDirty = true;
        _tileJobs.Complete();
        NativeArray<Tile>.Copy(tiles, _tiles);
    }


    public void PrintColor(int x, int y, string str, Color fgColor, Color bgColor)
    {
        _isDirty = true;
        _tileJobs.Complete();

        var bytes = CodePage437.StringToCP437(str, Allocator.TempJob);

        new WriteColoredTileGlyphsJob
        {
            bytes = bytes,
            pos = new int2(x, y),
            destination = _tiles,
            width = Size.x,
            fgColor = fgColor,
            bgColor = bgColor
        }.Run();
    }
    
    public virtual void Update()
    {
        if (_isDirty)
        {
            _isDirty = false;
            _backend.UploadTileData(_tiles);
        }

        _backend.Update();
    }

    /// <summary>
    /// Draw the console to the screen manually.
    /// </summary>
    public void Draw()
    {
        RenderUtility.DrawConsole(this, Material);
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
        var t = _tiles[At(x,y)];
        return t.glyph;
    }


    public void Resize(int w, int h)
    {
        if (w == Width && h == Height)
            return;
        
        Size = new int2(w, h);

        if (_tiles.IsCreated)
            _tiles.Dispose();

        _tiles = new NativeArray<Tile>(CellCount, Allocator.Persistent);

        for (int i = 0; i < _tiles.Length; ++i)
            _tiles[i] = Tile.EmptyTile;

        if(_backend == null )
            _backend = new SimpleMeshBackend(Width, Height, _mesh);
        else
            _backend.Resize(Width, Height);
        
        _isDirty = true;
    }
    

    public void Set(int x, int y, Color fgColor, Color bgColor, byte glyph)
    {
        int i = At(x, y);
        _tiles[i] = new Tile
        {
            fgColor = fgColor,
            bgColor = bgColor,
            glyph = glyph
        };
    }
    
    public void Dispose()
    {
        _tileJobs.Complete();

        _backend?.Dispose();

        if (_tiles.IsCreated)
            _tiles.Dispose();
    }


}
