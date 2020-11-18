using Sark.Common.GridUtil;
using Sark.Pathfinding;
using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sark.RLTK.Samples
{
    public class PathfindingDemo : InteractiveTerminal
    {
        int2? _start;
        int2? _end;
        Color _startColor = Color.blue;
        Color _endColor = Color.green;

        AStar<int> _aStar;
        NativeList<int> _path;

        InputAction _rmb;
        InputAction _pathfind;

        ControlsTerminal _controlsTerm;

        bool _firstRun = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            _rmb = _controls.Default.RightMouse;
            _pathfind = _controls.Default.FindPath;

            _aStar = new AStar<int>(_map.Length, Allocator.Persistent);
            _path = new NativeList<int>(_map.Length, Allocator.Persistent);

            _controlsTerm = FindObjectOfType<ControlsTerminal>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _aStar.Dispose();
            _path.Dispose();
        }

        protected override void BeforeDraw()
        {
            HandleSetStartEnd();
            HandlePathfind();
        }

        protected override void AfterDraw()
        {
            DrawPath();
            DrawStartEnd();
        }

        protected override void OnClear()
        {
            ClearStartEnd();
            ClearPath();
        }

        protected override void OnMapChanged(int2 p)
        {
            ClearPath();
            if (_start != null && _start.Value.Equals(p))
                _start = null;
            if (_end != null && _end.Value.Equals(p))
                _end = null;
        }

        void HandleSetStartEnd()
        {
            if (_rmb.triggered)
            {
                int2 p = GetMouseConsolePosition();
                if (!_map.IsInBounds(p))
                    return;

                ClearPath();

                if (_start == null)
                {
                    _start = p;
                    _map.SetIsObstacle(p, false);
                    SetDirty();
                    return;
                }

                if (_end == null)
                {
                    _end = p;
                    _map.SetIsObstacle(p, false);
                    SetDirty();
                    return;
                }

                ClearStartEnd();

                SetDirty();
            }
        }

        void HandlePathfind()
        {
            if (_pathfind.triggered && _start != null && _end != null)
            {
                ClearPath();

                int start = Grid2D.PosToIndex(_start.Value, _term.Width);
                int end = Grid2D.PosToIndex(_end.Value, _term.Width);

                if (_firstRun)
                {
                    FindPath(start, end);
                    _firstRun = false;
                    ClearPath();
                }
                FindPath(start, end);

                SetDirty();
            }
        }

        void FindPath(int start, int end)
        {
            var sw = new Stopwatch();
            sw.Start();
            new FindPathJob
            {
                AStar = _aStar,
                Map = _map,
                Path = _path,
                Start = start,
                End = end,
            }.Run();
            sw.Stop();

            if (_path.Length > 0)
                _controlsTerm.SetDisplayText($"Path took {sw.Elapsed.TotalMilliseconds} ms");
            else
            {
                _controlsTerm.SetDisplayText($"Couldn't find path!");
            }
        }

        void DrawStartEnd()
        {
            if (_start != null)
            {
                _term.Set(_start.Value.x, _start.Value.y, _startColor, 'S');
            }

            if (_end != null)
            {
                _term.Set(_end.Value.x, _end.Value.y, _endColor, 'E');
            }
        }

        void DrawPath()
        {
            if (_end == null || _start == null)
                return;

            new DrawPathJob
            {
                AStar = _aStar,
                Tiles = _term.Tiles,
                StartColor = _startColor,
                EndColor = _endColor,
                Path = _path,
            }.Run();

            DrawStartEnd();
        }

        void ClearStartEnd()
        {
            _start = null;
            _end = null;
        }

        void ClearPath()
        {
            _path.Clear();
            _aStar.Clear();
        }

        [BurstCompile]
        struct DrawPathJob : IJob
        {
            public TileData Tiles;
            public AStar<int> AStar;
            public NativeList<int> Path;
            public Color StartColor;
            public Color EndColor;

            public void Execute()
            {
                var visited = AStar.GetVisited(Allocator.Temp);

                for (int i = 0; i < visited.Length; ++i)
                {
                    int2 point = Grid2D.IndexToPos(visited[i], Tiles.Width);
                    Tiles.Set(point.x, point.y, Color.red, '.');
                }

                for (int i = 0; i < Path.Length; ++i)
                {
                    Color col = Color.Lerp(StartColor, EndColor, (float)i / Path.Length);
                    int2 point = Grid2D.IndexToPos(Path[i], Tiles.Width);
                    Tiles.Set(point.x, point.y, col, '█');
                }
            }
        }

        [BurstCompile]
        struct FindPathJob : IJob
        {
            public AStar<int> AStar;
            public NativeList<int> Path;
            public BasicPathingMap Map;
            public int Start;
            public int End;

            public void Execute()
            {
                AStar.FindPath(Map, Start, End, Path);
            }
        }
    } 
}
