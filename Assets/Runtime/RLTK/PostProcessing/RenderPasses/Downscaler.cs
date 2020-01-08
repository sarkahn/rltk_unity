using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RLTK.PostProcessing.RenderPasses
{
    public class Downscaler : ScriptableRendererFeature
    {
        class DownscalerPass : ScriptableRenderPass
        {

            public Material blitMaterial = null;
            public int blitShaderPassIndex = 0;
            public int2 rtSize;

            private RenderTargetIdentifier source { get; set; }
            private RenderTargetHandle destination { get; set; }

            RenderTargetHandle m_TemporaryColorTexture;
            string m_ProfilerTag;

            /// <summary>
            /// Create the CopyColorPass
            /// </summary>
            public DownscalerPass(Material blitMaterial, int blitShaderPassIndex, string tag, int2 rtSize)
            {
                this.renderPassEvent = RenderPassEvent.AfterRendering;
                this.blitMaterial = blitMaterial;
                this.blitShaderPassIndex = blitShaderPassIndex;
                m_ProfilerTag = tag;
                m_TemporaryColorTexture.Init("_TemporaryColorTexture");
                this.rtSize = math.max(rtSize, 1);
            }

            /// <summary>
            /// Configure the pass with the source and destination to execute on.
            /// </summary>
            /// <param name="source">Source Render Target</param>
            /// <param name="destination">Destination Render Target</param>
            public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
            {
                this.source = source;
                this.destination = destination;
            }

            /// <inheritdoc/>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.width = rtSize.x;
                opaqueDesc.height = rtSize.y;
                opaqueDesc.depthBufferBits = 0;

                // Can't read and write to same color target, create a temp render target to blit. 
                if (destination == RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, FilterMode.Point);
                    Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, blitShaderPassIndex);
                    Blit(cmd, m_TemporaryColorTexture.Identifier(), source);
                }
                else
                {
                    Blit(cmd, source, destination.Identifier(), blitMaterial, blitShaderPassIndex);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// <inheritdoc/>
            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (destination == RenderTargetHandle.CameraTarget)
                    cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            }
        }

        [System.Serializable]
        public class DownscalerSettings
        {
            public bool enabled;
            public Material CustomBlitMaterial = null;
            public int CustomBlitMaterialPassIndex = -1;
            public string textureId = "_CustomBlitPassTexture";
            public int2 _rtSize = new int2(240, 160);
        }

        public DownscalerSettings settings = new DownscalerSettings();
        RenderTargetHandle m_RenderTextureHandle;

        DownscalerPass pass;

        public override void Create()
        {
            var passIndex = settings.CustomBlitMaterial != null ? settings.CustomBlitMaterial.passCount - 1 : 1;
            settings.CustomBlitMaterialPassIndex = Mathf.Clamp(settings.CustomBlitMaterialPassIndex, -1, passIndex);
            pass = new DownscalerPass(settings.CustomBlitMaterial, settings.CustomBlitMaterialPassIndex, name, settings._rtSize);
            m_RenderTextureHandle.Init(settings.textureId);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled)
                return;

            var src = renderer.cameraColorTarget;

            var dest = RenderTargetHandle.CameraTarget;


            if (settings.CustomBlitMaterial == null)
            {
                settings.CustomBlitMaterial = Resources.Load<Material>("Unlit");
                if (settings.CustomBlitMaterial == null)
                {
                    //Debug.LogWarningFormat("Missing CustomBlit Material. {0} CustomBlit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                    return;
                }
            }

            pass.Setup(src, dest);
            renderer.EnqueuePass(pass);
        }
    } 
}

