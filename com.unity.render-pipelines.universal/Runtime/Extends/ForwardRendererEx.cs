using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{

    partial class UniversalRenderer
    {
        DrawObjectsPassEx drawUIObjectPass;

        BlitPassEx gammaPrePass, gammaPostPass;

        /// <summary>
        /// 
        /// Inject to end of UniversalRenderer's ctor
        /// </summary>
        /// <param name="data"></param>
        public void InitEx(UniversalRendererData data)
        {
            InitCameraGammaRendering(data);
        }

        public void SetupEx(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SetupCameraGammaRendering(context, ref renderingData);
        }


        public void DisposeEx()
        {
            gammaPostPass.Cleanup();
        }


        public void InitCameraGammaRendering(UniversalRendererData data)
        {
            gammaPrePass = new BlitPassEx(nameof(gammaPrePass),RenderPassEvent.AfterRendering+10, m_BlitMaterial);

            drawUIObjectPass = new DrawObjectsPassEx(nameof(drawUIObjectPass), false, RenderPassEvent.AfterRendering+11, RenderQueueRange.transparent, LayerMask.GetMask("UI"), m_DefaultStencilState, data.defaultStencilState.stencilReference);
            gammaPostPass = new BlitPassEx(nameof(gammaPostPass),RenderPassEvent.AfterRendering+20, m_BlitMaterial);
        }

        public void SetupCameraGammaRendering(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            var isUICamera = QualitySettings.activeColorSpace == ColorSpace.Linear &&
                //cameraData.exData.colorSpaceUsage == ColorSpace.Gamma &&
                cameraData.renderType == CameraRenderType.Overlay
                ;

            if (isUICamera)
            {
                drawUIObjectPass.Setup(cameraData.camera.cullingMask);
                drawUIObjectPass.RenderTarget = cameraData.exData.enableFSR ? PostProcessPass.FsrShaderConstants._EASUOutputTexture : ShaderPropertyId._FULLSIZE_GAMMA_TEX;

                EnqueuePass(drawUIObjectPass);
                
                //remove original blit pass
                DequeuePass(m_FinalBlitPass); // ui cammera use gammaPostPass

                if (!cameraData.exData.enableFSR)
                {
                    gammaPrePass.SetupPrePass(cameraData.cameraTargetDescriptor, m_ActiveCameraColorAttachment);
                    EnqueuePass(gammaPrePass);
                }

                gammaPostPass.SetupPostPass(cameraData.cameraTargetDescriptor, m_ActiveCameraColorAttachment);
                EnqueuePass(gammaPostPass);
            }
        } 



        public static bool IsApplyFinalPostProcessing(ref RenderingData renderingData, bool anyPostProcessing, bool lastCameraInTheStack)
        {
            return anyPostProcessing && lastCameraInTheStack &&
                (renderingData.cameraData.exData.enableFSR || renderingData.cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing)
                ;
        }
    }

}
