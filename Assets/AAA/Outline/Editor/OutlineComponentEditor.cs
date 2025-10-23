#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAA.Outline.Editor
{
    /// <summary>
    /// Custom inspector to ease debugging and configuration of per-object outlines.
    /// </summary>
    [CustomEditor(typeof(AAA.Outline.OutlineComponent))]
    public class OutlineComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var comp = (AAA.Outline.OutlineComponent)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Outline Settings", EditorStyles.boldLabel);

            comp.config = (AAA.Outline.OutlineConfig)EditorGUILayout.ObjectField("Config (Optional)", comp.config, typeof(AAA.Outline.OutlineConfig), false);
            comp.applyOnStart = EditorGUILayout.Toggle("Apply On Start", comp.applyOnStart);

            comp.color = EditorGUILayout.ColorField(new GUIContent("Color (HDR)"), comp.color, true, true, true);
            comp.useWorldSpaceThickness = EditorGUILayout.Toggle("Use World Space Thickness", comp.useWorldSpaceThickness);
            if (comp.useWorldSpaceThickness)
            {
                comp.thicknessWorld = EditorGUILayout.FloatField("Thickness (World)", Mathf.Max(0f, comp.thicknessWorld));
            }
            else
            {
                comp.thicknessPixels = EditorGUILayout.FloatField("Thickness (Pixels)", Mathf.Max(0f, comp.thicknessPixels));
            }

            comp.alpha = EditorGUILayout.Slider("Alpha", comp.alpha, 0f, 1f);
            comp.zTest = (CompareFunction)EditorGUILayout.EnumPopup("ZTest", comp.zTest);
            comp.zWrite = EditorGUILayout.Toggle("ZWrite", comp.zWrite);
            comp.cullMode = (CullMode)EditorGUILayout.EnumPopup("Cull", comp.cullMode);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            comp.useFresnel = EditorGUILayout.Toggle("Use Fresnel", comp.useFresnel);
            if (comp.useFresnel)
            {
                comp.fresnelPower = EditorGUILayout.FloatField("Fresnel Power", Mathf.Max(0f, comp.fresnelPower));
                comp.fresnelBias = EditorGUILayout.FloatField("Fresnel Bias", comp.fresnelBias);
            }
            comp.edgeSoftness = EditorGUILayout.Slider("Edge Softness", comp.edgeSoftness, 0f, 1f);

            comp.usePulse = EditorGUILayout.Toggle("Use Pulse", comp.usePulse);
            if (comp.usePulse)
            {
                comp.pulseAmplitude = EditorGUILayout.FloatField("Pulse Amplitude", comp.pulseAmplitude);
                comp.pulseSpeed = EditorGUILayout.FloatField("Pulse Speed", comp.pulseSpeed);
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(comp.IsApplied ? "Refresh" : "Apply", GUILayout.Height(24)))
                {
                    if (comp.IsApplied) comp.Refresh();
                    else comp.Apply();
                }
                if (GUILayout.Button("Remove", GUILayout.Height(24)))
                {
                    comp.Remove();
                }
                if (GUILayout.Button("Toggle", GUILayout.Height(24)))
                {
                    comp.Toggle();
                }
            }

            // Validation
            var rend = comp.GetComponent<Renderer>();
            if (rend == null)
            {
                EditorGUILayout.HelpBox("Renderer not found on this GameObject.", MessageType.Error);
            }
            else if (Application.isPlaying && comp.IsApplied == false)
            {
                EditorGUILayout.HelpBox("Outline is not currently applied (Play Mode).", MessageType.Info);
            }
        }
    }
}
#endif