using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal
{
    public partial class UniversalAdditionalCameraData
    {
        public enum AMDFSR
        {
            Disabled = -1,
            UltraQuality = 0,
            Quality,
            Balanced,
            Performance
        }
        [SerializeField] AMDFSR m_AMDFSR = AMDFSR.Disabled;

        public AMDFSR amdFSR
        {
            get => m_AMDFSR;
            set => m_AMDFSR = value;
        }


        [SerializeField] ColorSpace colorSpaceUsage = ColorSpace.Linear;
        public ColorSpace ColorSpaceUsage
        {
            get => colorSpaceUsage;
            set => colorSpaceUsage = value;
        }
    }
}
