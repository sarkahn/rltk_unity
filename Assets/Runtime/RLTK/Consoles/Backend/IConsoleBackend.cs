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
        /// <summary>
        /// Schedule parallel jobs to rebuild the internal render data. Requires read-only access to the 
        /// tiles array. <see cref="JobHandle.Complete"/> must be called on the returned job handle 
        /// before writing to the array again. The backend should handle the case of calling this
        /// while the internal jobs are already running.
        /// </summary>
        JobHandle ScheduleRebuild(int w, int h, NativeArray<Tile> tiles, JobHandle inputDeps = default);


        /// <summary>
        /// Updates internal render data once jobs are complete.
        /// </summary>
        void Update();

        int2 PixelsPerUnit { get; }
    }
}