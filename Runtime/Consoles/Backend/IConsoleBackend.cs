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
        void Resize(int width, int height);
        
        /// <summary>
        /// Immediately rebuilds the backend using the given tile data
        /// </summary>
        void UploadTileData(NativeArray<Tile> tiles);

        /// <summary>
        /// Schedule parallel jobs to rebuild the internal render data. Requires read-only access to the 
        /// tiles array.
        /// </summary>
        JobHandle ScheduleUploadTileData(NativeArray<Tile> tiles, JobHandle inputDeps = default);
        
        /// <summary>
        /// Should be called once per frame. Manages internal job completion without blocking.
        /// </summary>
        void Update();

    }
}