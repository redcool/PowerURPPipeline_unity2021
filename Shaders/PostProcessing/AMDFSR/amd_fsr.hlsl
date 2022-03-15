#define A_GPU 1
#define A_HLSL 1

#ifdef _AMD_FSR_HALF
#define A_HALF 1
#define FSR_EASU_H 1
#define FSR_RCAS_H 1
#define AREAL half
#else
#define FSR_EASU_F 1
#define FSR_RCAS_F 1
#define AREAL float
#endif

#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/AMDFSR/ffx_a.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/AMDFSR/ffx_fsr1.hlsl"


#ifdef _AMD_FSR_NEEDS_CONVERT_TO_SRGB
#define AMD_FSR_TO_SRGB(color) AToSrgbF4(color)
#else
#define AMD_FSR_TO_SRGB(color) (color)
#endif
