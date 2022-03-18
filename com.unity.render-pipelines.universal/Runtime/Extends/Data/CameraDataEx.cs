using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal
{
    public struct CameraDataEx
    {
        public bool enableFSR;
        public UniversalAdditionalCameraData.AMDFSR baseCameraAmdFSR;
        public ColorSpace colorSpaceUsage;
        public CameraRenderType renderType;

        public bool FsrNeedFinalBlit()
        {
            return renderType == CameraRenderType.Base
                || colorSpaceUsage != ColorSpace.Gamma
                || QualitySettings.activeColorSpace == ColorSpace.Gamma;
            ;
        }

        public bool NeedLinearToSRGB()
        {
            return colorSpaceUsage == ColorSpace.Gamma
                && QualitySettings.activeColorSpace == ColorSpace.Linear;
        }
    }
}
