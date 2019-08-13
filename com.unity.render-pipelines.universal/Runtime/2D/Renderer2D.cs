using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal
{
    internal class Renderer2D : ScriptableRenderer
    {
        VolumeBlendingPass m_VolumeBlendingPass;
        ColorGradingLutPass m_ColorGradingLutPass;
        Render2DLightingPass m_Render2DLightingPass;
        PostProcessPass m_PostProcessPass;
        FinalBlitPass m_FinalBlitPass;
        //PostProcessPass m_FinalPostProcessPass;

        RenderTargetHandle m_ColorTargetHandle;
        RenderTargetHandle m_AfterPostProcessColor;
        RenderTargetHandle m_ColorGradingLut;

        public Renderer2D(Renderer2DData data) : base(data)
        {
            m_VolumeBlendingPass = new VolumeBlendingPass(RenderPassEvent.BeforeRendering);
            m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingOpaques, data.postProcessData);
            m_Render2DLightingPass = new Render2DLightingPass(data);
            m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.postProcessData);
            //m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRenderingPostProcessing, data.postProcessData);
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, CoreUtils.CreateEngineMaterial(data.blitShader));

            m_AfterPostProcessColor.Init("_AfterPostProcessTexture");
            m_ColorGradingLut.Init("_InternalGradingLut");
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            EnqueuePass(m_VolumeBlendingPass);

            ref CameraData cameraData = ref renderingData.cameraData;
            m_ColorTargetHandle = RenderTargetHandle.CameraTarget;
            PixelPerfectCamera ppc = cameraData.camera.GetComponent<PixelPerfectCamera>();
            bool postProcessEnabled = renderingData.cameraData.postProcessEnabled;
            bool useOffscreenColorTexture = (ppc != null && ppc.useOffscreenRT) || postProcessEnabled || cameraData.isHdrEnabled || cameraData.isSceneViewCamera || !cameraData.isDefaultViewport;

            if (useOffscreenColorTexture)
            {
                var filterMode = ppc != null ? ppc.finalBlitFilterMode : FilterMode.Bilinear;
                m_ColorTargetHandle = CreateOffscreenColorTexture(context, ref cameraData.cameraTargetDescriptor, filterMode);
            }

            ConfigureCameraTarget(m_ColorTargetHandle.Identifier(), BuiltinRenderTextureType.CameraTarget);

            m_Render2DLightingPass.ConfigureTarget(m_ColorTargetHandle.Identifier());
            EnqueuePass(m_Render2DLightingPass);

            if (postProcessEnabled)
            {
                m_ColorGradingLutPass.Setup(m_ColorGradingLut);
                EnqueuePass(m_ColorGradingLutPass);

                m_PostProcessPass.Setup(cameraData.cameraTargetDescriptor, m_ColorTargetHandle, m_AfterPostProcessColor, new RenderTargetHandle(), m_ColorGradingLut);
                EnqueuePass(m_PostProcessPass);
            }

            if (useOffscreenColorTexture)
            {
                var blitSource = postProcessEnabled ? m_AfterPostProcessColor : m_ColorTargetHandle;

                if (ppc != null)
                    m_FinalBlitPass.Setup(cameraData.cameraTargetDescriptor, blitSource, ppc.useOffscreenRT, ppc.finalBlitPixelRect);
                else
                    m_FinalBlitPass.Setup(cameraData.cameraTargetDescriptor, blitSource);

                EnqueuePass(m_FinalBlitPass);
            }
        }
        
        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
        {
            cullingParameters.cullingOptions = CullingOptions.None;
            cullingParameters.isOrthographic = cameraData.camera.orthographic;
            cullingParameters.shadowDistance = 0.0f;
        }

        RenderTargetHandle CreateOffscreenColorTexture(ScriptableRenderContext context, ref RenderTextureDescriptor cameraTargetDescriptor, FilterMode filterMode)
        {
            RenderTargetHandle colorTextureHandle = new RenderTargetHandle();
            colorTextureHandle.Init("_CameraColorTexture");

            var colorDescriptor = cameraTargetDescriptor;
            colorDescriptor.depthBufferBits = 32;

            CommandBuffer cmd = CommandBufferPool.Get("Create Camera Textures");
            cmd.GetTemporaryRT(colorTextureHandle.id, colorDescriptor, filterMode);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            return colorTextureHandle;
        }

        public override void FinishRendering(CommandBuffer cmd)
        {
            if (m_ColorTargetHandle != RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_ColorTargetHandle.id);
        }
    }
}
