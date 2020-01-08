using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RLTK.PostProcessing.RenderPasses
{
    public class Scanlines : ScriptableRendererFeature
    {
        const string kDefaultMaterialPath = "Materials/Scanlines";

        class ScanlinesPass : ScriptableRenderPass
        {
            public Material blitMaterial = null;
            public int blitShaderPassIndex = 0;

            private RenderTargetIdentifier source { get; set; }
            private RenderTargetHandle destination { get; set; }

            RenderTargetHandle m_TemporaryColorTexture;
            string m_ProfilerTag;

            /// <summary>
            /// Create the CopyColorPass
            /// </summary>
            public ScanlinesPass(ScanlinesSettings settings, string tag)
            {
                this.renderPassEvent = RenderPassEvent.AfterRendering;
                this.blitMaterial = settings.scanlinesMaterial;
                this.blitShaderPassIndex = settings.materialPassIndex;
                m_ProfilerTag = tag;
                m_TemporaryColorTexture.Init("_TemporaryColorTexture");
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
        public class ScanlinesSettings
        {
            public bool enabled;

            [Header("Pass Settings")]
            public Material scanlinesMaterial;
            public int materialPassIndex = -1;
            public string textureId = "_CustomBlitPassTexture";
        }

        public ScanlinesSettings settings = new ScanlinesSettings();
        RenderTargetHandle m_RenderTextureHandle;

        ScanlinesPass pass;

        public override void Create()
        {
            var passIndex = settings.scanlinesMaterial != null ? settings.scanlinesMaterial.passCount - 1 : 1;
            settings.materialPassIndex = Mathf.Clamp(settings.materialPassIndex, -1, passIndex);
            pass = new ScanlinesPass(settings, name);
            m_RenderTextureHandle.Init(settings.textureId);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled)
                return;

            if (settings.scanlinesMaterial == null)
            {
                settings.scanlinesMaterial = Resources.Load<Material>(kDefaultMaterialPath);
                if (settings.scanlinesMaterial == null)
                {
                    Debug.LogWarningFormat($"No material assigned and unable to find default material at path " +
                        $"Resources/{kDefaultMaterialPath}", this);
                    settings.enabled = false;
                    return;
                }
            }

            var src = renderer.cameraColorTarget;
            var dest = RenderTargetHandle.CameraTarget;

            pass.Setup(src, dest);
            renderer.EnqueuePass(pass);
        }
    }
}