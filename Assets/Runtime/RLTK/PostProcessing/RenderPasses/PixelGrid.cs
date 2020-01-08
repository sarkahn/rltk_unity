using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;

namespace RLTK.PostProcessing.RenderPasses
{
    public class PixelGrid : ScriptableRendererFeature
    {
        //Material material;
        //public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        //public Color gridColorEven_ = Color.magenta;
        //public Color gridColorOdd_ = Color.white;
        //public int2 pixelsPerUnit_ = new int2(8,8);

        PixelGridPass m_ScriptablePass;

        public PixelGridSettings settings;

        public override void Create()
        {

            settings.material = Resources.Load<Material>("Materials/PixelGrid");

            if (settings.material == null)
            {
                Debug.LogWarning("Error finding material for PixelGrid pass");
                settings.enabled = false;
                return;
            }


            m_ScriptablePass = new PixelGridPass(settings);

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.enabled == false)
                return;

            renderer.EnqueuePass(m_ScriptablePass);
        }

        [System.Serializable]
        public class PixelGridSettings
        {
            public bool enabled;
            public Material material;
            public int2 pixelsPerUnit = new int2(8, 8);
            public Color evenColor = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, .15f);
            public Color oddColor = new Color(1, 1, 1, 0.05f);
        }


        class PixelGridPass : ScriptableRenderPass
        {
            string m_ProfilerTag = "DrawFullScreenPass";

            Material material;

            PixelGridSettings settings = new PixelGridSettings();

            public PixelGridPass(PixelGridSettings settings)
            {
                this.settings = settings;
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                material = settings.material;

                if (material == null)
                    return;

                material.SetColor("_GridColorEven", settings.evenColor);
                material.SetColor("_GridColorOdd", settings.oddColor);
                material.SetVector("_PixelsPerUnit", new Vector4(settings.pixelsPerUnit.x, settings.pixelsPerUnit.y, 0, 0));

                Camera camera = renderingData.cameraData.camera;

                var cmd = CommandBufferPool.Get(m_ProfilerTag);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
            }
        }
    }


}