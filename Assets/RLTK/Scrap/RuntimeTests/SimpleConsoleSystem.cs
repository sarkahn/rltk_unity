using RLTK;
using RLTK.Consoles;
using RLTK.Consoles.Backend;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
namespace RLTK.Scrapped
{

    // Scrapped for now. RenderMesh is not an appropriate system for
    // procedural meshes. The console mesh must exist in a builtin renderer.
    // or using Graphics.Draw*.
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class SimpleConsoleSystem : JobComponentSystem
    {
        SimpleConsole _console;

        struct ConsoleInitialized : IComponentData { }

        protected override void OnCreate()
        {
            //var authoringData = Object.FindObjectOfType<ConsoleAuthoring>();

            //if( authoringData != null )
            //    InitConsole(authoringData);
        }

        void InitConsole(SimpleConsoleData data, Mesh mesh)
        {

            IConsoleBackend backend = null;

            int w = data.Width;
            int h = data.Height;

            backend = new SimpleMeshBackend(w, h, mesh);


            //switch(data.Backend)
            //{
            //    default:
            //        backend = new SimpleMeshBackend(w, h, filter.sharedMesh);
            //        break;
            //}

            _console = new SimpleConsole(
                w,
                h,
                backend,
                Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _console?.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //Entities
            //    .WithoutBurst()
            //    .WithNone<ConsoleInitialized>() 
            //    .WithStructuralChanges()
            //    .ForEach((Entity e, in SimpleConsoleData data, in RenderMesh renderMesh) =>
            //    {
            //        EntityManager.AddComponent<ConsoleInitialized>(e);

            //        InitConsole(data, renderMesh.mesh);
            //    }).Run();

            return default;
        }
    }

}