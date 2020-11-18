//using RLTK;
//using RLTK.MonoBehaviours;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.Experimental.GlobalIllumination;
//using UnityEngine.InputSystem;
//using UnityEngine.Profiling;
//using static RLTK.CodePage437;


//namespace PathfindingMapsTest
//{
//    public static class Int2Extension
//    {
//        public static void Deconstruct(this int2 p, out int x, out int y)
//        {
//            x = p.x;
//            y = p.y;
//        }
//    }

//    public class PathfindingMaps : MonoBehaviour
//    {
//        [SerializeField]
//        SimpleConsoleProxy _console = null;

//        Controls _controls;
//        InputAction _leftMouse;
//        InputAction _rightMouse;
//        InputAction _middleMouse;
//        InputAction _lmbDrag;
//        InputAction _togglePathfinding;
//        InputAction _resizeMap;
//        InputAction _stepPathfinding;

//        bool _dragging = false;
//        HashSet<int2> _draggedPoints = new HashSet<int2>();

//        int? _start = null;
//        int? _end = null;
//        TestMap _map;

//        bool _pathfindRunning;

//        AStar3 _aStar;
//        NativeList<int> _path;

//        SlidingAverage _averageTime = new SlidingAverage(30, 0);

//        static Color _startColor = Color.blue;
//        static Color _endColor = Color.green;

//        [BurstCompile]
//        struct FindPathJob : IJob
//        {
//            public AStar3 AStar;
//            public NativeList<int> Path;
//            public TestMap Map;

//            public void Execute()
//            {
//                AStar.FindPath(Map, Path);
//            }
//        }

//        private void OnEnable()
//        {
//            _controls = new Controls();
//            _controls.Enable();
//            _leftMouse = _controls.Default.LeftMouse;
//            _rightMouse = _controls.Default.RightMouse;
//            _middleMouse = _controls.Default.MiddleMouse;
//            _lmbDrag = _controls.Default.LMBDrag;
//            _togglePathfinding = _controls.Default.TogglePathfinding;
//            _resizeMap = _controls.Default.ResizeMap;
//            _stepPathfinding = _controls.Default.StepPathfinding;

//            _lmbDrag.performed += OnDragBegin;
//            _lmbDrag.canceled += OnDragEnd;

//            BuildArrays();

//        }

//        private void OnDisable()
//        {
//            DisposeArrays();
//        }

//        void BuildArrays()
//        {
//            _map = new TestMap(_console.Size, Allocator.Persistent);
//            _path = new NativeList<int>(_console.CellCount, Allocator.Persistent);
//            _aStar = new AStar3(_console.CellCount, Allocator.Persistent);
//        }

//        void DisposeArrays()
//        {
//            _map.Dispose();
//            _path.Dispose();
//            _aStar.Dispose();
//        }

//        private IEnumerator Start()
//        {
//            yield return null;

//            if (_console != null)
//            {
//                _console.ClearScreen();
//                _console.FillBorder(ToCP437('#'));
//            }
//            BuildMapFromConsole();

//            //BuildConsoleFromMap();
//            //_pathfindRunning = true;
//        }

//        void BuildMapFromConsole()
//        {
//            //var (map, start, end) = GetHugeMap(Allocator.Persistent);
//            //_map = map;
//            //_start = start;
//            //_end = end;

//            GridHelper grid = new GridHelper(_console.Size);
//            var bytes = _map.GetData();
//            var tiles = _console.ReadAllTiles(Allocator.Temp);
//            for (int i = 0; i < bytes.Length; ++i)
//            {
//                int2 p = grid.IndexToPos(i);
//                if (_console.Get(p.x, p.y).Value == ToCP437('#'))
//                {
//                    //Debug.Log($"Setting path map wall at {p}");
//                    bytes[i] = 1;
//                }
//                else
//                    bytes[i] = 0;
//            }
//        }

//        void BuildConsoleFromMap()
//        {
//            _console.ClearScreen();
//            var data = _map.GetData();
//            for( int i = 0; i < data.Length; ++i )
//                if(data[i] != 0)
//                {
//                    var p = _map.IndexToPos(i);
//                    _console.Set(p.x, p.y, Color.white, Color.black, ToCP437('#'));
//                }

//            var start = _map.IndexToPos(_start.Value);
//            var end = _map.IndexToPos(_end.Value);
//            _console.Set(start.x, start.y, Color.blue, Color.black, ToCP437('S'));
//            _console.Set(end.x, end.y, Color.green, Color.black, ToCP437('E'));
//        }

//        int2 GetMouseConsolePosition()
//        {
//            float2 mouseXY = Mouse.current.position.ReadValue();
//            float3 mousePos = new float3(mouseXY, transform.position.z);

//            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
//            int2 consolePos = _console.WorldToConsolePosition(worldPos);
//            return consolePos;
//        }

//        void ToggleGlyph(int2 p)
//        {
//            int x = p.x;
//            int y = p.y;
//            byte existing = _console.Get(x, y).Value;

//            int i = new GridHelper().PosToIndex(p);

//            if ((_start != null && i == _start.Value) ||
//                (_end != null && i == _end.Value))
//                return;

//            if (ToChar(existing) == '#')
//            {
//                _console.Set(x, y, Color.black, Color.black, ToCP437(' '));
//            }
//            else
//            {
//                _console.Set(x, y, Color.white, Color.black, ToCP437('#'));
//            }

//            BuildMapFromConsole();
//        }

//        private void Update()
//        {
//            if (_leftMouse.triggered)
//            {
//                int2 consolePos = GetMouseConsolePosition();

//                if (_console.InBounds(consolePos))
//                {
//                    ToggleGlyph(consolePos);
//                }
//            }

//            if (_dragging)
//            {
//                OnDrag();
//            }

//            if (_togglePathfinding.triggered)
//            {
//                _pathfindRunning = !_pathfindRunning;
//                //OnPathfind();
//            }

//            if (_pathfindRunning)
//            {
//                OnPathfind();
//            }

//            if (_middleMouse.triggered)
//            {
//                OnSetPathMarker();
//            }

//            if(_stepPathfinding.triggered)
//            {
//                OnStepPathfinding();
//            }
//        }

//        void SetTile(int i, char c, Color color = default, Color bg = default)
//        {
//            if (color == default)
//                color = Color.white;
//            if (bg == default)
//                bg = Color.black;

//            var p = new GridHelper(_console.Size).IndexToPos(i);
//            _console.Set(p.x, p.y, color, bg, ToCP437(c));
//        }

//        void ClearTile(int i)
//        {
//            var p = new GridHelper(_console.Size).IndexToPos(i);
//            _console.Set(p.x, p.y, Color.white, Color.black, 0);
//        }

//        void ClearAStarTiles()
//        {
//            var open = _aStar.GetFrontier(Allocator.Temp);
//            var closed = _aStar.GetVisited(Allocator.Temp);

//            for (int i = 0; i < open.Length; ++i)
//            {
//                var node = open[i];
//                ClearTile(node.Value);
//            }

//            for (int i = 0; i < closed.Length; ++i)
//            {
//                ClearTile(closed[i]);
//            }
//        }

//        void DrawStart()
//        {
//            if(_start != null)
//                SetTile(_start.Value, 'S', _startColor);
//        }

//        void DrawEnd()
//        {
//            if(_end != null )
//                SetTile(_end.Value, 'E', _endColor);
//        }

//        void OnSetPathMarker()
//        {
//            ClearAStarTiles();
//            ResetAStar();

//            var mousePos = GetMouseConsolePosition();
//            if (!_console.InBounds(mousePos))
//                return;
//            ClearPath();

//            var grid = new GridHelper(_console.Size);
//            int posIndex = grid.PosToIndex(mousePos);

//            if (_start == null)
//            {
//                _start = posIndex;
//                DrawStart();
//                return;
//            }

//            if (_end == null)
//            {
//                _end = posIndex;
//                DrawEnd();
//                ResetAStar();
//                _aStar.SetStartEnd(_start.Value, _end.Value);

//                return;
//            }

//            ClearTile(_end.Value);
//            ClearTile(_start.Value);
//            _end = null;
//            _start = null;
//        }

//        void ClearPath()
//        {
//            for (int i = 1; i < _path.Length - 1; ++i)
//            {
//                ClearTile(_path[i]);
//            }
//        }

//        void OnStepPathfinding()
//        {
//            if (_start == null || _end == null)
//            {
//                Debug.Log("Can't step pathfinding - need a start and end");
//                return;
//            }

//            //Debug.Log("Stepping path");
//            if(!_aStar.Step(_map))
//            {
//                Debug.Log("Pathfinding is complete");
//            }

//            DrawVisitedTiles();
//        }

//        void OnPathfind()
//        {
//            if (_start == null || _end == null)
//                return;

//            ResetAStar();
//            _aStar.SetStartEnd(_start.Value, _end.Value);

//            Profiler.BeginSample("FindPath");
//            new FindPathJob { AStar = _aStar, Map = _map, Path = _path }.Run();
//            Profiler.EndSample();

//            if (_path.IsEmpty)
//                Debug.Log("Couldn't find path!");

//            //DrawVisitedTiles();
//            //DrawStart();
//            //DrawEnd();
//            //DrawPath();
//        }

//        void DrawVisitedTiles()
//        {
//            var frontier = _aStar.GetFrontier(Allocator.Temp);
//            var visited = _aStar.GetVisited(Allocator.Temp);

//            for (int i = 0; i < frontier.Length; ++i)
//            {
//                var node = frontier[i];
//                //Debug.Log($"Open list node position {node.Value}: " +
//                //    $"({_map.IndexToPos(node.Value)})"); ;
//                SetTile(node.Value, 'o', Color.white, Color.blue);
//            }

//            for (int i = 0; i < visited.Length; ++i)
//            {
//                if (visited[i] < 0 || visited[i] > _console.CellCount)
//                    continue;
//                SetTile(visited[i], 'c', Color.white, Color.red);
//            }

//            DrawStart();
//            DrawEnd();
//        }

//        void DrawPath()
//        {
//            for (int i = 1; i < _path.Length - 1; ++i)
//            {
//                var prev = _map.IndexToPos(_path[i - 1]);
//                var curr = _map.IndexToPos(_path[i]);
//                var next = _map.IndexToPos(_path[i + 1]);

//                // Bitmasking based on which edge our previous and next positions are
//                int prevValue = BitValueFromDir(curr - prev);
//                int nextValue = BitValueFromDir(curr - next);
//                int bitValue = prevValue + nextValue;
//                char glyph = GlyphFromBitValue((byte)bitValue);
//                var col = Color.Lerp(_startColor, _endColor, (float)i / _path.Length);
//                SetTile(_path[i], glyph, col);
//            }
//        }

//        void OnDragBegin(InputAction.CallbackContext ctx)
//        {
//            var p = GetMouseConsolePosition();
//            _draggedPoints.Add(p);
//            _dragging = true;
//        }

//        void OnDragEnd(InputAction.CallbackContext ctx)
//        {
//            _dragging = false;
//            _draggedPoints.Clear();
//        }

//        private void OnDrag()
//        {
//            int2 p = GetMouseConsolePosition();

//            if (!_console.InBounds(p))
//                return;

//            if (!_draggedPoints.Contains(p))
//            {
//                _draggedPoints.Add(p);
//                ToggleGlyph(p);
//            }
//        }

//        void ResetAStar()
//        {
//            ClearPath();
//            _path.Clear();
//            _aStar.Clear();
//        }

//        private void OnGUI()
//        {
//            GUILayout.BeginVertical(GUI.skin.box);

//            GUILayout.Label("LMB to Toggle Walls");
//            GUILayout.Label("Middle Mouse to place Start/End");
//            GUILayout.Label("R to toggle pathfinding");
//            GUILayout.Label("Arrows keys to resize map");

//            GUILayout.Label($"Pathfinding : {_pathfindRunning}");

//            if (!_pathfindRunning)
//            {
//                GUILayout.EndVertical();
//                return;
//            }

//            if (_start == null || _end == null)
//            {
//                GUILayout.Label("Need both a start and end point");
//                GUILayout.EndVertical();
//                return;
//            }

//            if (_path.IsEmpty)
//            {
//                GUILayout.Label("Unable to find path!");
//                return;
//            }

//            GUILayout.Label($"Average Pathfinding Time Per Frame: " +
//                $"{_averageTime.GetSmoothedValue()}");

//            GUILayout.EndVertical();
//        }

//        class SlidingAverage
//        {
//            long[] _buffer;
//            int _lastIndex;

//            public SlidingAverage(int numSamples, long initial)
//            {
//                _buffer = new long[numSamples];
//                _lastIndex = 0;
//                Reset(initial);
//            }

//            public void Reset(long value)
//            {
//                for (int i = 0; i < _buffer.Length; ++i)
//                    _buffer[i] = value;
//            }

//            public void Push(long value)
//            {
//                _buffer[_lastIndex] = value;

//                _lastIndex++;
//                if (_lastIndex >= _buffer.Length)
//                    _lastIndex = 0;
//            }

//            public float GetSmoothedValue()
//            {
//                long sum = 0;
//                foreach (var v in _buffer)
//                    sum += v;
//                return (float)((float)sum / _buffer.Length);
//            }
//        }

//        char ver = '│';
//        char hor = '─';
//        char upLeft = '┐';
//        char upRight = '┌';
//        char downLeft = '┘';
//        char downRight = '└';

//        char GlyphFromBitValue(byte val) => val switch
//        {
//            3 => upRight,
//            4 => upLeft,
//            5 => upLeft,
//            6 => hor,
//            9 => ver,
//            10 => downRight,
//            12 => downLeft,
//            _ => ' '
//        };

//        byte BitValueFromDir(int2 dir) => dir switch
//        {
//            (0, 1) => 1 << 0,  // 1
//            (-1, 0) => 1 << 1, // 2
//            (1, 0) => 1 << 2,  // 4
//            (0, -1) => 1 << 3, // 8
//            _ => 0
//        };

//        public static (TestMap, int, int) GetHugeMap(Allocator allocator)
//        {
//            int2 size = new int2(500, 500);
//            var map = new TestMap(size, allocator);

//            int x = size.x / 2;
//            for (int y = 0; y < size.y - 2; ++y)
//                map.SetTile(x, y, 1);


//            int start = map.PosToIndex(0, 0);
//            int end = map.PosToIndex(size.x - 1, 0);
//            return (map, start, end);
//        }
//    }

//}
