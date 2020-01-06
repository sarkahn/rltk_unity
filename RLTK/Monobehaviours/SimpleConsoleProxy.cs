using RLTK.Consoles;
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

        SimpleConsole _console;
        bool _resized;

        public int2 Size => _console.Size;

        public int Width => _console.Width;
        public int Height => _console.Height;

        public int CellCount => _console.CellCount;

        public int2 PixelsPerUnit => _console.PixelsPerUnit;

        private void OnEnable()
        {
            RebuildConsole();
        }

        void RebuildConsole()
        {
            var renderer = GetComponent<MeshRenderer>();
            var filter = GetComponent<MeshFilter>();

            filter.sharedMesh = new Mesh();

            if (renderer.sharedMaterial == null)
                renderer.sharedMaterial = new Material(Shader.Find("Custom/GlyphShader"));

            _console?.Dispose();

            var backend = new SimpleMeshBackend(_width, _height, filter.sharedMesh);
            _console = new SimpleConsole(_width, _height, backend, Allocator.Persistent);

            _console.ClearScreen();
        }

        private void Update()
        {
            if (_resized)
            {
                _resized = false;
                RebuildConsole();
            }

            _console.RebuildIfDirty();
        }

        private void LateUpdate()
        {
            _console.Update();
        }


        private void OnDisable()
        {
            _console?.Dispose();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled && Application.isPlaying)
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
            throw new System.NotImplementedException();
        }

        public void Draw(Font font, Material mat)
        {
            throw new System.NotImplementedException();
        }

        public int At(int x, int y)
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

        public void Set(int x, int y, Color32 fgColor, Color32 bgColor, byte glyph)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawHollowBox(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
        {
            throw new System.NotImplementedException();
        }

        public void DrawBoxDouble(int x, int y, int width, int height, Color32 fgColor, Color32 bgColor)
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

        //public NativeArray<Tile> CopyTiles(Allocator allocator) => _console.CopyTiles(allocator);

        public JobHandle WriteTiles(NativeArray<Tile> input, JobHandle inputDeps) => 
            _console.ScheduleWriteTiles(input, inputDeps);

        public JobHandle CopyTiles(NativeArray<Tile> buffer, JobHandle inputDeps) => 
            _console.ScheduleCopyTiles(buffer, inputDeps);

        public void CopyTiles(NativeArray<Tile> buffer) => _console.CopyTiles(buffer);

        public void WriteTiles(NativeArray<Tile> buffer) => _console.WriteTiles(buffer);
    }
}