using RLTK.FieldOfView;
using RLTK.FieldOfView.BeveledCorners;
using Sark.Common.GridUtil;
using Sark.RLTK.Samples;
using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sark.RLTK.Samples
{
    public class FOVDemo : InteractiveTerminal
    {
        VisibilityMap _visibility;

        bool _visibilityMapOn = false;

        [SerializeField]
        int _range = 8;

        bool _scrolling = false;

        protected override IEnumerator Start()
        {
            base.Start();

            yield return null;

            DoNoise();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _visibility = new VisibilityMap(_map.Width, _map.Height, _map, Allocator.Persistent);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _visibility.Dispose();
        }

        protected override void BeforeDraw()
        {
            base.BeforeDraw();

            float2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.y != 0 && !_scrolling)
            {
                _scrolling = true;
                _range += (int)math.sign(scroll.y);
            }
            else
                _scrolling = false;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _visibilityMapOn = !_visibilityMapOn;
                SetDirty();
            }

            var p = GetMouseConsolePosition();
            if (!_term.IsInBounds(p))
                return;

            if(_visibilityMapOn)
            {
                _visibility.Clear();
                new FOVJob
                {
                    Map = _visibility,
                    Origin = p,
                    Range = _range
                }.Run();

                SetDirty();
            }
        }

        protected override void AfterDraw()
        {
            base.AfterDraw();

            if (!_visibilityMapOn)
                return;

            _term.ClearScreen();
            var tiles = _term.Tiles;

            new DrawMapJob
            {
                Visibility = _visibility,
                Tiles = tiles
            }.Run();

            _term.SetDirty();
            _term.ImmediateUpdate();
        }
        protected override void OnResized(int2 newSize)
        {
            _visibility.Dispose();
            _visibility = new VisibilityMap(_map.Width, _map.Height, _map, Allocator.Persistent);
        }
    }

    [BurstCompile]
    struct DrawMapJob : IJob
    {
        public VisibilityMap Visibility;
        public TileData Tiles;
        public void Execute()
        {
            for (int i = 0; i < Visibility.Length; ++i)
            {
                int2 p = Grid2D.IndexToPos(i, Visibility.Width);
                if (Visibility[i])
                {
                    if (Visibility.IsOpaque(p))
                        Tiles.Set(p.x, p.y, '#');
                    else
                        Tiles.Set(p.x, p.y, '.');
                }
            }
        }
    }

    [BurstCompile]
    struct FOVJob : IJob
    {
        public VisibilityMap Map;
        public int Range;
        public int2 Origin;

        public void Execute()
        {
            FOV.Compute(Origin, Range, Map);
        }
    }

    struct VisibilityMap : IVisibilityMap
    {
        BasicPathingMap opaque;
        NativeArray<bool> visible;
        int2 size;

        public int Width => size.x;
        public int Height => size.y;
        public int Length => visible.Length;

        public VisibilityMap(int w, int h, BasicPathingMap obstacleMap, Allocator allocator)
        {
            size = new int2(w, h);
            visible = new NativeArray<bool>(w * h, allocator);
            opaque = obstacleMap;
        }

        public bool this[int i] => visible[i];

        public int PosToIndex(int2 p) =>
            Grid2D.PosToIndex(p, Width);

        public bool IsInBounds(int2 p) => 
            Grid2D.InBounds(p, size);

        public float Distance(int2 a, int2 b)
        {
            return math.distance(a, b);
        }

        public bool IsOpaque(int2 p)
        {
            if (!IsInBounds(p))
                return true;
            return opaque.IsObstacle(p);
        }

        public void SetVisible(int2 p)
        {
            if (!IsInBounds(p))
                return;
            visible[PosToIndex(p)] = true;
        }

        public void Clear()
        {
            for (int i = 0; i < visible.Length; ++i)
                visible[i] = false;
        }

        public void Dispose()
        {
            visible.Dispose();
        }
    }
}
