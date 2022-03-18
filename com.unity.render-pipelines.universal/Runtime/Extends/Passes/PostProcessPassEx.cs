using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal.Internal
{
    public partial class PostProcessPass
    {
        ComputeBuffer easuCB;
        ComputeBuffer rcasCB;

        ProfilingSampler fsrSampler = new ProfilingSampler(FsrShaderConstants.FSR_PROFILE_ID);

        void CleanupFSRBuffers()
        {
            if (easuCB != null)
            {
                easuCB.Release();
                easuCB = null;
            }
            if (rcasCB != null)
            {
                rcasCB.Release();
                rcasCB = null;
            }
        }

        void SetupFSRBuffers(bool enableFSR)
        {
            if (enableFSR)
            {
                if (easuCB == null)
                    easuCB = new ComputeBuffer(4, sizeof(uint) * 4);
                if (rcasCB == null)
                    rcasCB = new ComputeBuffer(4, sizeof(uint) * 4);
            }
        }

        public void SetupFinalPass(in RenderTargetHandle source,bool useSwapBuffer, in RenderTextureDescriptor finalDesc = new RenderTextureDescriptor())
        {
            m_Source = source.id;
            m_Destination = RenderTargetHandle.CameraTarget;
            m_IsFinalPass = true;
            m_HasFinalPass = false;
            m_EnableSRGBConversionIfNeeded = true;
            m_Descriptor = finalDesc;
            m_UseSwapBuffer = useSwapBuffer;
        }

        RenderTextureDescriptor GetUAVCompatibleDescriptor(int width, int height)
        {
            var desc = m_Descriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.width = width;
            desc.height = height;
            desc.enableRandomWrite = true;
            return desc;
        }

        bool TryRenderFSR(CommandBuffer cmd, ref CameraData cameraData, Material material, RenderBufferLoadAction colorLoadAction, RenderTargetHandle cameraTargetHandle)
        {
            if (! cameraData.exData.enableFSR)
                return false;

            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : cameraTargetHandle.Identifier();
            cmd.GetTemporaryRT(FsrShaderConstants._EASUInputTexture, GetCompatibleDescriptor());
            cmd.SetRenderTarget(FsrShaderConstants._EASUInputTexture, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
            cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);

            DoFSR(cmd, ref cameraData, cameraTarget);
            return true;
        }

        #region EASU
        void EdgeAdaptiveSpatialUpsampling(CommandBuffer cmd, CameraData cameraData, bool needs_convert_to_srgb)
        {
            var easuCS = m_Data.shaders.easuCS;
            if (needs_convert_to_srgb)
            {
                easuCS.EnableKeyword("_AMD_FSR_NEEDS_CONVERT_TO_SRGB");
            }
            else
            {
                easuCS.DisableKeyword("_AMD_FSR_NEEDS_CONVERT_TO_SRGB");
            }
            int viewCount = 1;
            int kinitialize_idx = easuCS.FindKernel("KInitialize");
            int kmain_idx = easuCS.FindKernel("KMain");
            cmd.SetComputeTextureParam(easuCS, kmain_idx, FsrShaderConstants._EASUInputTexture, FsrShaderConstants._EASUInputTexture);
            int srcWidth = m_Descriptor.width;
            int srcHeight = m_Descriptor.height;
            int dstWidth = cameraData.pixelWidth;
            int dstHeight = cameraData.pixelHeight;
            cmd.SetComputeVectorParam(easuCS, FsrShaderConstants._EASUViewportSize, new Vector4(srcWidth, srcHeight));
            cmd.SetComputeVectorParam(easuCS, FsrShaderConstants._EASUInputImageSize, new Vector4(srcWidth, srcHeight));
            cmd.GetTemporaryRT(FsrShaderConstants._EASUOutputTexture, GetUAVCompatibleDescriptor(dstWidth, dstHeight));
            cmd.SetComputeTextureParam(easuCS, kmain_idx, FsrShaderConstants._EASUOutputTexture, FsrShaderConstants._EASUOutputTexture);
            cmd.SetComputeVectorParam(easuCS, FsrShaderConstants._EASUOutputSize, new Vector4(dstWidth, dstHeight, 1.0f / dstWidth, 1.0f / dstHeight));
            cmd.SetComputeBufferParam(easuCS, kinitialize_idx, FsrShaderConstants._EASUParameters, easuCB);
            cmd.SetComputeBufferParam(easuCS, kmain_idx, FsrShaderConstants._EASUParameters, easuCB);
            cmd.DispatchCompute(easuCS, kinitialize_idx, 1, 1, 1);
            int DivRoundUp(int x, int y) => (x + y - 1) / y;
            int dispatchX = DivRoundUp((int)dstWidth, 8);
            int dispatchY = DivRoundUp((int)dstHeight, 8);

            cmd.DispatchCompute(easuCS, kmain_idx, dispatchX, dispatchY, viewCount);
        }
        #endregion

        #region RCAS
        void RobustContrastAdaptiveSharpening(CommandBuffer cmd, CameraData cameraData, bool needs_convert_to_srgb)
        {
            var rcasCS = m_Data.shaders.rcasCS;
            if (needs_convert_to_srgb)
            {
                rcasCS.EnableKeyword("_AMD_FSR_NEEDS_CONVERT_TO_SRGB");
            }
            else
            {
                rcasCS.DisableKeyword("_AMD_FSR_NEEDS_CONVERT_TO_SRGB");
            }
            int viewCount = 1;
            int kinitialize_idx = rcasCS.FindKernel("KInitialize");
            int kmain_idx = rcasCS.FindKernel("KMain");

            cmd.SetComputeFloatParam(rcasCS, FsrShaderConstants._RCASScale, 1.0f);
            cmd.SetComputeTextureParam(rcasCS, kmain_idx, FsrShaderConstants._RCASInputTexture, FsrShaderConstants._EASUOutputTexture);
            int dstWidth = cameraData.pixelWidth;
            int dstHeight = cameraData.pixelHeight;
            cmd.GetTemporaryRT(FsrShaderConstants._RCASOutputTexture, GetUAVCompatibleDescriptor(dstWidth, dstHeight));
            cmd.SetComputeTextureParam(rcasCS, kmain_idx, FsrShaderConstants._RCASOutputTexture, FsrShaderConstants._RCASOutputTexture);
            cmd.SetComputeBufferParam(rcasCS, kinitialize_idx, FsrShaderConstants._RCASParameters, rcasCB);
            cmd.SetComputeBufferParam(rcasCS, kmain_idx, FsrShaderConstants._RCASParameters, rcasCB);
            cmd.DispatchCompute(rcasCS, kinitialize_idx, 1, 1, 1);

            int DivRoundUp(int x, int y) => (x + y - 1) / y;
            int dispatchX = DivRoundUp((int)dstWidth, 8);
            int dispatchY = DivRoundUp((int)dstHeight, 8);

            cmd.DispatchCompute(rcasCS, kmain_idx, dispatchX, dispatchY, viewCount);
        }
        #endregion

        #region FSR
        void DoFSR(CommandBuffer cmd, ref CameraData cameraData, RenderTargetIdentifier dst)
        {
            bool needs_convert_to_srgb = !(cameraData.isHdrEnabled || QualitySettings.activeColorSpace == ColorSpace.Gamma);
            needs_convert_to_srgb = needs_convert_to_srgb || cameraData.exData.NeedLinearToSRGB();
            
            using (new ProfilingScope(cmd, fsrSampler))
            {
                EdgeAdaptiveSpatialUpsampling(cmd, cameraData, needs_convert_to_srgb);
                //RobustContrastAdaptiveSharpening(cmd, cameraData, needs_convert_to_srgb);
            }
            // gamma camera need continue rendering ui.
            if (cameraData.exData.FsrNeedFinalBlit())
            {
                cmd.Blit(FsrShaderConstants._EASUOutputTexture, dst);
                //cmd.Blit(FsrShaderConstants._RCASOutputTexture, dst);
            }
        }
        #endregion



        public static class FsrShaderConstants
        {
            // Edge Adaptive Spatial Upsampling
            public static readonly int _EASUInputTexture = Shader.PropertyToID("_EASUInputTexture");
            public static readonly int _EASUOutputTexture = Shader.PropertyToID("_EASUOutputTexture");
            public static readonly int _EASUViewportSize = Shader.PropertyToID("_EASUViewportSize");
            public static readonly int _EASUInputImageSize = Shader.PropertyToID("_EASUInputImageSize");
            public static readonly int _EASUOutputSize = Shader.PropertyToID("_EASUOutputSize");
            public static readonly int _EASUParameters = Shader.PropertyToID("_EASUParameters");

            // Robust Contrast Adaptive Sharpening
            public static readonly int _RCASInputTexture = Shader.PropertyToID("_RCASInputTexture");
            public static readonly int _RCASScale = Shader.PropertyToID("_RCASScale");
            public static readonly int _RCASParameters = Shader.PropertyToID("_RCASParameters");
            public static readonly int _RCASOutputTexture = Shader.PropertyToID("_RCASOutputTexture");

            public const string FSR_PROFILE_ID = "FSR";
            public const string AMD_FSR_MIPMAP_BIAS = "amd_fsr_mipmap_bias";
        }

    }
}
