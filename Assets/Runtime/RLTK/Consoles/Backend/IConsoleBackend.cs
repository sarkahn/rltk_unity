using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace RLTK.Consoles.Backend
{
    /// <summary>
    /// A backend to manage the internal render data for a console.
    /// </summary>
    public interface IConsoleBackend : IDisposable
    {
        int2 PixelsPerUnit { get; }
        void Draw(Material mat);
        
        /// <summary>
        /// Immediately rebuilds the internal render data.
        /// </summary>
        void Rebuild(int w, int h, NativeArray<Tile> tiles);

        /// <summary>
        /// Schedule parallel jobs to rebuild the internal render data. Requires read-only access to the 
        /// tiles array.
        /// </summary>
        JobHandle ScheduleRebuild(int w, int h, NativeArray<Tile> tiles, JobHandle inputDeps = default);


        /// <summary>
        /// Should be called once per frame. Manages internal job completion without blocking.
        /// </summary>
        void ApplyMeshChanges();

    }
}