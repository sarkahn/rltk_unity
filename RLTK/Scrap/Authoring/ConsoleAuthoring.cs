using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Scrapped
{

    struct SimpleConsoleData : IComponentData
    {
        public int Width;
        public int Height;
    }

    // A hybrid component to let us tweak console parameters from inside the editor.
    // A component system can query for this to build a console for the ECS world.

    // This doesn't work so well. The hybrid renderer uses shared component data on renderers and sets those 
    // SCD values based on material and mesh. So for a procedural mesh that would mean it has to move chunks
    // every time we change something in it. For now it makes more sense to use other rendering options.
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [RequiresEntityConversion]
    public class ConsoleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Width = 16;
        public int Height = 16;
        public ConsoleBackendType Backend = ConsoleBackendType.SimpleMesh;

        Entity _entity;

        void Awake()
        {
            // If our mesh and material aren't initialized before conversion the 
            // conversion will fail.
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
            var renderer = GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
                renderer.sharedMaterial = new Material(Shader.Find("Custom/GlyphShader"));
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddComponentData(entity, new SimpleConsoleData
            {
                Width = Width,
                Height = Height,
            });

            _entity = entity;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Width = math.max(1, Width);
            Height = math.max(1, Height);
        }
#endif

        public enum ConsoleBackendType
        {
            SimpleMesh,
        };
    }
}