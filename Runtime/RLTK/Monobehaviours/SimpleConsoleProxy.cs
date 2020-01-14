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

        NativeConsole _console;
        bool _resized;
        
        public int2 Size => _console == null ? new int2(_width, _height) : _console.Size;

        public int Width => _console.Width;
        public int Height => _console.Height;

        public int CellCount => _console.CellCount;

        public int2 PixelsPerUnit => _console.PixelsPerUnit;

        public Material Material => _console.Material;

        const string DEFAULT_MAT_PATH_ = "Materials/ConsoleMat";


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
            {
                renderer.sharedMaterial = Resources.Load<Material>(DEFAULT_MAT_PATH_);

                if( renderer.sharedMaterial == null )
                {
                    Debug.LogWarning($"No material was set for this console and the default material " +
                        $"was not found at Resources/{DEFAULT_MAT_PATH_}.", gameObject);
                }
            }

            _console?.Dispose();
            
            _console = new NativeConsole(_width, _height, renderer.sharedMaterial, filter.sharedMesh);
        }

        public void Update()
        {
            if (_resized)
            {
                _resized = false;
                RebuildConsole();
            }
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

        public void Set(int x, int y, Color fgColor, Color bgColor, byte glyph)
        {
            throw new System.NotImplementedException();
        }

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

        public void SetMaterial(Material mat) => 
            _console.SetMaterial(mat);

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