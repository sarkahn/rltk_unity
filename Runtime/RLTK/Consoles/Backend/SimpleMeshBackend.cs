
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using RLTK.Consoles.Backend;

using Color = UnityEngine.Color;
using Mesh = UnityEngine.Mesh;
using Debug = UnityEngine.Debug;
using UnityEngine;

// Note: Profiling shows the bottleneck here are the calls to Mesh.SetUV.
//    Using the advanced mesh api or instancing should show massive performance 
//    improvements if the console size needs to scale up. Should be fine as is for now.

namespace RLTK.Consoles
{
    /// <summary>
    /// A rendering backend using Unity's "Simple Mesh API" 
    /// https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Mesh.html
    /// </summary>
    public class SimpleMeshBackend : IConsoleBackend
    {
        Mesh _mesh;
        JobHandle _rebuildDataJob = default;
        bool _isDirty = false;

        NativeArray<int> _indices;
        NativeArray<float3> _verts;
        NativeArray<float2> _uvs;
        NativeArray<Color> _fgColors;
        NativeArray<Color> _bgColors;

        public int2 Size { get; private set; }

        public int CellCount => Size.x * Size.y;

        int2 _pixelsPerUnit = new int2(8, 8);
        
        public int2 PixelsPerUnit => _pixelsPerUnit;

        /// <summary>
        /// Construct a simple mesh backend for use in a console. The allocator will be used
        /// to initialize all internal arrays.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mesh"></param>
        /// <param name="allocator"></param>
        public SimpleMeshBackend(int width, int height, Mesh mesh)
        {
            _mesh = mesh;

            int cellCount = width * height;

            Size = new int2(width, height);

            var allocator = Allocator.Persistent;

            _indices = new NativeArray<int>(cellCount * 6, allocator);
            _verts = new NativeArray<float3>(cellCount * 4, allocator);
            _uvs = new NativeArray<float2>(cellCount * 4, allocator);
            _fgColors = new NativeArray<Color>(cellCount * 4, allocator);
            _bgColors = new NativeArray<Color>(cellCount * 4, allocator);

            InitializeVerts(width, height);
        }

        public void Draw(Material mat)
        {
            Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, mat, 0);
        }

        public void Dispose()
        {
            _rebuildDataJob.Complete();

            _indices.Dispose();
            _verts.Dispose();
            _uvs.Dispose();
            _fgColors.Dispose();
            _bgColors.Dispose();
        }

        void InitializeVerts(int w, int h)
        {
            float3 start = -new float3(w, h, 0) / 2f;
            for (int x = 0; x < w; ++x)
                for (int y = 0; y < h; ++y)
                {
                    int i = y * w + x;

                    float3 p = start + new float3(x, y, 0);
                    // 0---1
                    // | / | 
                    // 2---3
                    int vi = i * 4;
                    _verts[vi + 0] = p + new float3(0, 1, 0);
                    _verts[vi + 1] = p + new float3(1, 1, 0);
                    _verts[vi + 2] = p + new float3(0, 0, 0);
                    _verts[vi + 3] = p + new float3(1, 0, 0);

                    int ii = i * 6;
                    _indices[ii + 0] = vi + 0;
                    _indices[ii + 1] = vi + 1;
                    _indices[ii + 2] = vi + 2;
                    _indices[ii + 3] = vi + 3;
                    _indices[ii + 4] = vi + 2;
                    _indices[ii + 5] = vi + 1;
                }

            _mesh.Clear();

            _mesh.SetVertices(_verts);
            _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _mesh.RecalculateTangents();
        }
        
        /// <summary>
        /// Immediately rebuilds internal rendering data.
        /// </summary>
        public void Rebuild(int w, int h, NativeArray<Tile> tiles)
        {
            // Force mesh refresh on next Update
            _isDirty = true;

            _rebuildDataJob.Complete();

            float2 uvSize = new float2(1f / 16f);

            var uvsJob = new UVJob
            {
                tiles = tiles,
                uvs = _uvs,
                uvSize = uvSize,

            }.Schedule(tiles.Length, 64, _rebuildDataJob);

            var colorJob = new ColorsJob
            {
                tiles = tiles,
                fgColors = _fgColors,
                bgColors = _bgColors,
            }.Schedule(tiles.Length, 64, _rebuildDataJob);

            var jobs = JobHandle.CombineDependencies(uvsJob, colorJob);

            jobs.Complete();
        }

        /// <summary>
        /// Schedule parallel jobs to rebuild the internal mesh data. Requires read-only access to the 
        /// tiles array.
        /// </summary>
        /// <returns>The job handle for the scheduled jobs.</returns>
        public JobHandle ScheduleRebuild(int w, int h, NativeArray<Tile> tiles, JobHandle inputDeps = default)
        {
            _isDirty = true;

            _rebuildDataJob = JobHandle.CombineDependencies(inputDeps, _rebuildDataJob);

            float2 uvSize = new float2(1f / 16f);

            var uvsJob = new UVJob
            {
                tiles = tiles,
                uvs = _uvs,
                uvSize = uvSize,

            }.Schedule(tiles.Length, 64, _rebuildDataJob);

            var colorJob = new ColorsJob
            {
                tiles = tiles,
                fgColors = _fgColors,
                bgColors = _bgColors,
            }.Schedule(tiles.Length, 64, _rebuildDataJob);

            _rebuildDataJob = JobHandle.CombineDependencies(uvsJob, colorJob);
            
            return _rebuildDataJob;
        }

        /// <summary>
        /// Should be called every frame. Applies any scheduled changes.
        /// </summary>
        public void ApplyMeshChanges()
        {
            if (_isDirty)
            {
                // Required by the safety system
                _rebuildDataJob.Complete();
                
                _isDirty = false;
                
                _mesh.SetUVs(0, _uvs);
                _mesh.SetUVs(1, _fgColors);
                _mesh.SetUVs(2, _bgColors);
            }
        }
    }
    

    [BurstCompile]
    struct UVJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Tile> tiles;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> uvs;

        public float2 uvSize;

        public void Execute(int index)
        {
            var tile = tiles[index];

            index = index * 4;

            int2 glyphIndex = new int2(
                tile.glyph % 16,
                16 - 1 - (tile.glyph / 16)
                );



            float2 right = new float2(uvSize.x, 0);
            float2 up = new float2(0, uvSize.y);
            float2 bl = glyphIndex * uvSize;

            uvs[index + 0] = bl + up;
            uvs[index + 1] = bl + up + right;
            uvs[index + 2] = bl;
            uvs[index + 3] = bl + right;
        }
    }

    [BurstCompile]
    struct ColorsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Tile> tiles;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Color> fgColors;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Color> bgColors;

        public void Execute(int index)
        {
            var tile = tiles[index];

            index = index * 4;

            for (int i = 0; i < 4; ++i)
            {
                fgColors[index + i] = tile.fgColor;
                bgColors[index + i] = tile.bgColor;
            }
        }
    }
}