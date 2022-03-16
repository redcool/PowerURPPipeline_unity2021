using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class BlitPassEx : FinalBlitPass
    {

        public bool isPrePass,isPostPass;

        public BlitPassEx(string samplerName, RenderPassEvent evt, Material blitMaterial) : base(evt, blitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(samplerName);
        }

        public void SetupPrePass(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
        {
            m_Source = colorHandle.id;
            isPrePass = true;
        }

        public void SetupPostPass(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
        {
            m_Source = colorHandle.id;
            isPostPass = true;
        }

        public void Cleanup()
        {

        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_BlitMaterial == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_BlitMaterial, GetType().Name);
                return;
            }

            m_BlitMaterial.shaderKeywords = null;

            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            ref CameraData cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;
            var needLinearToSRGB = cameraData.exData.NeedLinearToSRGB();

            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
            RenderTargetIdentifier sourceTarget = m_Source;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, profilingSampler))
            {
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.LinearToSRGBConversion,
                    cameraData.requireSrgbConversion);


                if (isPrePass)
                {
                    cameraTarget = ShaderPropertyId._FULLSIZE_GAMMA_TEX;

                    var desc = cameraData.cameraTargetDescriptor;
                    desc.width = cameraData.camera.pixelWidth;
                    desc.height = cameraData.camera.pixelHeight;
                    cmd.GetTemporaryRT(ShaderPropertyId._FULLSIZE_GAMMA_TEX, desc);

                    if (needLinearToSRGB)
                    {
                        cmd.EnableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
                    }
                }

                if (isPostPass)
                {
                    sourceTarget = cameraData.exData.enableFSR ? PostProcessPass.FsrShaderConstants._EASUOutputTexture : ShaderPropertyId._FULLSIZE_GAMMA_TEX;
                    //cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : RenderTargetHandle.GetCameraTarget(cameraData.xr).Identifier();

                    if (needLinearToSRGB)
                    {
                        cmd.DisableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
                        cmd.EnableShaderKeyword(ShaderKeywordStrings.SRGBToLinearConversion);
                    }
                }

                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, sourceTarget);
                cmd.SetRenderTarget(cameraTarget);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

                if (isPostPass)
                {
                    cmd.DisableShaderKeyword(ShaderKeywordStrings.SRGBToLinearConversion);
                    cmd.ReleaseTemporaryRT(ShaderPropertyId._FULLSIZE_GAMMA_TEX);
                }

            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
