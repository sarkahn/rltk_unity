using RLTK.Consoles;
using RLTK.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
//using RLTK.FieldOfView.Bresenham;
using RLTK.FieldOfView.BeveledCorners;
using RLTK.FieldOfView;

namespace RLTK.Samples
{
    public struct TestMap : IVisibilityMap, IDisposable
    {
        int width;
        int height;
        public NativeArray<bool> opaquePoints;
        public NativeList<int2> visiblePoints;

        public TestMap(int width, int height, Allocator allocator, params int2[] opaquePoints)
        {
            this.width = width;
            this.height = height;
            this.opaquePoints = new NativeArray<bool>(width * height, allocator);
            visiblePoints = new NativeList<int2>(width * height / 2, allocator);
            foreach (var p in opaquePoints)
                this.opaquePoints[p.y * width + p.x] = true;
        }

        public bool IsInBounds(int2 p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

        public bool IsOpaque(int2 p)
        {
            if (!IsInBounds(p))
                return true;
            return opaquePoints[p.y * width + p.x];
        }

        public void SetOpaque(int2 p, bool v) => opaquePoints[p.y * width + p.x] = v;

        public void SetVisible(int2 p)
        {
            if(IsInBounds(p))
                visiblePoints.Add(p);
        }

        public float Distance(int2 a, int2 b) => math.floor(math.distance(a, b));

        public void Dispose()
        {
            visiblePoints.Dispose();
            opaquePoints.Dispose();
        }
    }

    public class FOVExample : MonoBehaviour
    {
        [SerializeField]
        int _range = 10;

        [SerializeField]
        Transform _walls = null;

        [SerializeField]
        SimpleConsoleProxy _console = null;

        List<int2> _wallPositions = new List<int2>();

        int2 WorldToConsolePos(Vector3 p) => new int2(math.floor(p).xy) + (_console.Size / 2);

        TestMap _testMap;


        private void Start()
        {
            _testMap = new TestMap(_console.Width, _console.Height, Allocator.Persistent);

            if( _walls.gameObject.activeInHierarchy )
            {
                var walls = _walls.GetComponentInChildren<Transform>();
                foreach (Transform t in walls)
                {
                    var p = WorldToConsolePos(t.position);
                    _testMap.SetOpaque(p, true);
                }

                Destroy(_walls.gameObject);
            }
        }

        private void OnDestroy()
        {
            _testMap.Dispose();
        }

        private void Update()
        {
            var fovPos = WorldToConsolePos(transform.position);

            _testMap.visiblePoints.Clear();
            //var points = FOV.GetVisiblePoints(fovPos, _range, _testMap, Allocator.Temp).ToNativeArray();
            FOV.Compute(fovPos, _range, _testMap);

            _console.ClearScreen();

            foreach ( var p in _testMap.visiblePoints )
            {
                char ch = _testMap.IsOpaque(p) ? '#' : '.';
                _console.Set(p.x, p.y, Color.white, Color.black, CodePage437.ToCP437(ch));
            }
            
        }
    }
}