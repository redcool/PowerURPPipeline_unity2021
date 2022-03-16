using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{



    partial class UniversalRenderPipelineCameraEditor
    {

        partial class StylesEx
        {
            public static GUIContent AMDFSR = EditorGUIUtility.TrTextContent("AMD FSR 1.0", "AMD FidelityFX Super Resolution 1.0 is a cutting edge super-optimized spatial upscaling technology that produces impressive image quality at fast framerates.");
        }

        SerializedProperty m_AdditionalCameraDataRenderAMDFSR;
        SerializedProperty colorSpaceUsage;
        SerializedProperty renderPostProcessing;


        void DrawRenderSettingsEx()
        {
            EditorGUILayout.PropertyField(colorSpaceUsage);
        }


        void InitEx(SerializedObject m_AdditionalCameraDataSO)
        {
            m_AdditionalCameraDataRenderAMDFSR = m_AdditionalCameraDataSO.FindProperty("m_AMDFSR");
            colorSpaceUsage = m_AdditionalCameraDataSO.FindProperty("colorSpaceUsage");
            renderPostProcessing = m_AdditionalCameraDataSO.FindProperty("m_RenderPostProcessing");
        }

        private void DrawPostProcessingEx()
        {
            if (renderPostProcessing.boolValue)
                EditorGUILayout.PropertyField(m_AdditionalCameraDataRenderAMDFSR, StylesEx.AMDFSR);
        }

        public static void DrawRenderSettingsEx(UniversalRenderPipelineSerializedCamera cam,Editor owner)
        {
            var e = owner as UniversalRenderPipelineCameraEditor;
            if (e == null)
                return;

            e.DrawRenderSettingsEx();
        }

        public static void DrawPostProcessingEx(Editor owner)
        {
            var e = owner as UniversalRenderPipelineCameraEditor;
            if (e == null)
                return;

            e.DrawPostProcessingEx();
        }


    }



}
