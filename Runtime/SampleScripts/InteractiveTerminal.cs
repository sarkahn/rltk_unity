using Sark.RenderUtils;
using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static Sark.Terminals.CodePage437;

namespace Sark.RLTK.Samples
{
    public class InteractiveTerminal : MonoBehaviour
    {
        [SerializeField]
        int2 _size = new int2(80, 40);

        bool _dirty = false;

        protected BasicPathingMap _map;
        protected TerminalBehaviour _term;

        protected Controls _controls;

        InputAction _addNoise;
        InputAction _resize;
        InputAction _clear;

        DraggingControls _drag;
        MapNoise _noise = new MapNoise();

        bool _addingWalls = false;
        bool _resizing = false;

        public BasicPathingMap GetMap() => _map;

        protected virtual void OnEnable()
        {
            _term = GetComponent<TerminalBehaviour>();
            _drag = GetComponent<DraggingControls>();
            _drag.OnPointDragged += OnPointDragged;
            _drag.OnDragEnded += OnDragEnded;
            _drag.OnDragStarted += OnDragStarted;

            _controls = new Controls();
            _controls.Enable();

            _addNoise = _controls.Default.AddNoise;
            _resize = _controls.Default.ResizeMap;
            _clear = _controls.Default.Clear;

            _map = new BasicPathingMap(_size.x, _size.y, Allocator.Persistent);
        }

        protected virtual IEnumerator Start()
        {
            _term.Resize(_size.x, _size.y);
            SetDirty();

            yield return null;
            //Debug.Log($"Resizing terminal to {_size}");
        }

        protected virtual void OnDisable()
        {
            _drag.OnDragEnded -= OnDragEnded;
            _drag.OnPointDragged -= OnPointDragged;

            _map.Dispose();
        }

        void Update()
        {
            HandleClear();
            HandleResize();
            HandleNoise();

            BeforeDraw();

            if (_dirty)
            {
                _dirty = false;

                DrawTerminal();

                AfterDraw();
            }
        }

        protected void SetDirty() { _dirty = true; }

        protected virtual void BeforeDraw() { }
        protected virtual void AfterDraw() { }
        protected virtual void OnClear() { }
        protected virtual void OnMapChanged(int2 p) { }
        protected virtual void OnResized(int2 newSize) { }

        void HandleResize()
        {
            if (_resize.triggered && !_resizing)
            {
                _resizing = true;
                var vec = _resize.ReadValue<Vector2>();

                var newSize = _size;
                newSize.x += (int)vec.x;
                newSize.y += (int)vec.y;

                newSize = math.max(1, newSize);

                if (!_size.Equals(newSize))
                {
                    FindObjectOfType<TiledCamera>().TileCount = newSize;

                    _size = newSize;

                    _map.Dispose();
                    _map = new BasicPathingMap(_size.x, _size.y, Allocator.Persistent);

                    _term.Resize(_size.x, _size.y);
                    _dirty = true;

                    OnResized(_size);
                }
            }
            else
                _resizing = false;
        }

        void HandleNoise()
        {
            if (_addNoise.triggered)
            {
                DoNoise();
            }
        }

        void HandleClear()
        {
            if (_clear.triggered)
                Clear();
        }

        void OnDragStarted(int2 p)
        {
            _addingWalls = !_map.IsObstacle(p);
        }

        void OnPointDragged(int2 p)
        {
            _map.SetIsObstacle(p, _addingWalls);
            OnMapChanged(p);
            _dirty = true;
        }

        void OnDragEnded(HashSet<int2> points)
        {
            _addingWalls = false;
        }

        protected void DoNoise()
        {
            Clear();
            SetDirty();
            _noise.AddNoiseToMap(_map);
        }

        [BurstCompile]
        struct RebuildConsoleFromMapJob : IJob
        {
            public TileData Tiles;
            public NativeArray<bool> Map;
            public void Execute()
            {
                for (int i = 0; i < Map.Length; ++i)
                {
                    var t = Tiles[i];
                    t.glyph = Map[i] ? ToCP437('#') : ToCP437(' ');
                    t.fgColor = Color.white;
                    t.bgColor = Color.black;
                    Tiles[i] = t;
                }
            }
        }

        void Clear()
        {
            _map.Clear();
            OnClear();
        }

        void DrawTerminal()
        {
            _term.SetDirty();

            //Debug.Log($"Drawing Terminal. MapSize {_map.Size}. TermSize {_term.Size}. TilesSize {_term.Tiles.Size}");

            new RebuildConsoleFromMapJob
            {
                Tiles = _term.Tiles,
                Map = _map.Points
            }.Run();
        }

        protected int2 GetMouseConsolePosition()
        {
            float2 mouseXY = Mouse.current.position.ReadValue();
            float3 mousePos = new float3(mouseXY, transform.position.z);

            float3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            int2 tileIndex = _term.WorldPosToTileIndex(worldPos);
            return tileIndex;
        }
    } 
}
