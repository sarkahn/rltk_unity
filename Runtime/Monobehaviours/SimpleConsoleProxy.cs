﻿using RLTK.Consoles;
using RLTK.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.MonoBehaviours
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class SimpleConsoleProxy : MonoBehaviour, IConsole
    {
        [SerializeField]
        int _width = 16;

        [SerializeField]
        int _height = 16;

        [SerializeField]
        NativeConsole _console;
        bool _resized;

        public Material Material => _renderer.sharedMaterial;
        
        public int2 Size => _console == null ? new int2(_width, _height) : _console.Size;

        public int Width => _console.Width;
        public int Height => _console.Height;

        public int CellCount => _console.CellCount;

        public int2 PixelsPerUnit => _console.PixelsPerUnit;

        public Mesh Mesh => _console.Mesh;

        MeshRenderer _renderer;
        MeshFilter _filter;

        private void Awake()
        {
            if( _renderer == null )
                _renderer = GetComponent<MeshRenderer>();
            
            if( _filter == null )
                _filter = GetComponent<MeshFilter>();

            if (_renderer.sharedMaterial == null)
                _renderer.sharedMaterial = RenderUtility.DefaultMaterial;

            RebuildConsole();
        }

        private void OnEnable()
        {
            RebuildConsole();
        }

        void RebuildConsole()
        {
            _console?.Dispose();

            _console = new NativeConsole(_width, _height, Material);
            _filter.sharedMesh = _console.Mesh;

            _resized = true;
        }

        private void LateUpdate()
        {
            if (_resized)
            {
                _resized = false;
                _console.Resize(_width, _height);
                RenderUtility.UpdatePixelEffectData(_console);
            }

            RenderUtility.UpdatePixelEffectProperties(_console, Material);
            _console.Update();
        }


        private void OnDisable()
        {
            _console?.Dispose();
        }
        
        private void OnValidate()
        {
            _width = math.max(1, _width);
            _height = math.max(1, _height);
                
            _resized = true;
        }

        public void RebuildIfDirty()
        {
            throw new System.NotImplementedException();
        }

        public void ClearScreen()
        {
            _console.ClearScreen();
        }

        public void Resize(int w, int h)
        {
            _width = w;
            _height = h;
            _resized = true;
        }

        public void Draw()
        {
            _console.Draw();
        }

        public int At(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void Print(int x, int y, string str) => _console.Print(x, y, str);

        public void PrintColor(int x, int y, string str, Color fgColor = default, Color bgColor = default)
            => _console.PrintColor(x, y, str, fgColor, bgColor);

        public void Set(int x, int y, Color fgColor, Color bgColor, byte glyph) => 
            _console.Set(x, y, fgColor, bgColor, glyph);

        public void DrawBox(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBox(int x, int y, int width, int height, Color fgColor, Color bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBoxDouble(int x, int y, int width, int height, Color fgColor, Color bgColor)
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

        public NativeArray<Tile> ReadTiles(int x, int y, int length, Allocator allocator) => 
            _console.ReadTiles(x, y, length, allocator);
        

        public JobHandle ScheduleWriteTiles(NativeArray<Tile> input, JobHandle inputDeps) => 
            _console.ScheduleWriteTiles(input, inputDeps);

        public JobHandle ScheduleCopyTiles(NativeArray<Tile> buffer, JobHandle inputDeps) => 
            _console.ScheduleReadAllTiles(buffer, inputDeps);

        public NativeArray<Tile> ReadAllTiles(Allocator allocator) => 
            _console.ReadAllTiles(allocator);

        public void WriteTiles(int x, int y, NativeArray<Tile> tiles) => 
            _console.WriteTiles(x, y, tiles);

        public void WriteAllTiles(NativeArray<Tile> input) => 
            _console.WriteAllTiles(input);

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            Rect r = new Rect();
            r.size = new Vector2(Size.x, Size.y);
            r.center = transform.position;
            Gizmos.DrawWireCube(r.center, r.size);

            var col = Color.blue;
            col.a = .15f;
            Gizmos.color = col;

            Gizmos.DrawCube(r.center, r.size * .95f);
        }
#endif
    }
}