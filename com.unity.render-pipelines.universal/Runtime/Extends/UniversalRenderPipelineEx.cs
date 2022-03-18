using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal
{

    public partial class UniversalRenderPipeline
    {
        private struct AMDFSRSettings
        {
            public readonly float m_RenderScale;
            public readonly float m_MipmapBias;
            public AMDFSRSettings(in float render_scale, in float mipmap_bias)
            {
                m_RenderScale = render_scale;
                m_MipmapBias = mipmap_bias;
            }
        };
        private static readonly AMDFSRSettings[] amdFSRSettingsPreset = new AMDFSRSettings[]
        {
            new AMDFSRSettings(.77f, -.38f),
            new AMDFSRSettings(.67f, -.58f),
            new AMDFSRSettings(.59f, -.79f),
            new AMDFSRSettings(.50f, -1.0f)
        };

        static void InitialCameraDataEx(UniversalAdditionalCameraData additionalCameraData, ref CameraData cameraData, bool resolveFinalTarget)
        {
            InitialCameraDataFsr(additionalCameraData, ref cameraData, resolveFinalTarget);
        }
        
        static void InitialCameraDataFsr(UniversalAdditionalCameraData additionalCameraData, ref CameraData cameraData, bool resolveFinalTarget)
        {
            if (additionalCameraData != null)
            {
                cameraData.exData.colorSpaceUsage = additionalCameraData.ColorSpaceUsage;
                cameraData.exData.renderType = additionalCameraData.renderType;

                if (cameraData.renderType == CameraRenderType.Base)
                    cameraData.exData.baseCameraAmdFSR = additionalCameraData.renderPostProcessing ? additionalCameraData.amdFSR : UniversalAdditionalCameraData.AMDFSR.Disabled;

                if (resolveFinalTarget && cameraData.exData.baseCameraAmdFSR != UniversalAdditionalCameraData.AMDFSR.Disabled)
                {
                    //asset.msaaSampleCount = 8; // NOTE! You can also use some other AA solutions.
                    var amdFSRSetting = amdFSRSettingsPreset[(int)cameraData.exData.baseCameraAmdFSR];
                    cameraData.renderScale = amdFSRSetting.m_RenderScale;
                    cameraData.exData.enableFSR = true;
                    Shader.SetGlobalFloat("amd_fsr_mipmap_bias", amdFSRSetting.m_MipmapBias);
                }
            }
        }
    }
}
