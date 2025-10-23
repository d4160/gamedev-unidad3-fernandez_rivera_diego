using System;
using UnityEngine;

namespace AAA.Outline
{
    /// <summary>
    /// Value object for per-instance outline parameters.
    /// It is designed to be applied via MaterialPropertyBlock to avoid material instancing.
    /// </summary>
    [Serializable]
    public struct OutlineSettings
    {
        /// <summary>Outline color (HDR supported).</summary>
        public Color color;
        /// <summary>Thickness in screen pixels (used if useWorldSpaceThickness is false).</summary>
        [Min(0f)] public float thicknessPixels;
        /// <summary>Thickness in world units when useWorldSpaceThickness is true.</summary>
        [Min(0f)] public float thicknessWorld;
        /// <summary>Use world-space thickness instead of screen-space thickness.</summary>
        public bool useWorldSpaceThickness;
        /// <summary>Alpha multiplier (0..1).</summary>
        [Range(0,1)] public float alpha;
        /// <summary>Depth test compare function. 4 = LEqual, 8 = Always, etc.</summary>
        public UnityEngine.Rendering.CompareFunction zTest;
        /// <summary>ZWrite control. Usually Off for outlines.</summary>
        public bool zWrite;
        /// <summary>Cull mode for the outline pass. Usually Front.</summary>
        public UnityEngine.Rendering.CullMode cullMode;
        /// <summary>Enable fresnel contribution.</summary>
        public bool useFresnel;
        /// <summary>Fresnel power (higher = sharper rim).</summary>
        [Min(0f)] public float fresnelPower;
        /// <summary>Fresnel bias (baseline rim intensity).</summary>
        public float fresnelBias;
        /// <summary>Enable pulsating intensity.</summary>
        public bool usePulse;
        /// <summary>Pulse amplitude multiplier.</summary>
        public float pulseAmplitude;
        /// <summary>Pulse speed factor.</summary>
        public float pulseSpeed;
        /// <summary>Edge softness (alpha fade based on rim factor).</summary>
        [Range(0,1)] public float edgeSoftness;

        /// <summary>Returns a good default set for most cases.</summary>
        public static OutlineSettings Default => new OutlineSettings
        {
            color = new Color(1f, 0.6f, 0f, 1f),
            thicknessPixels = 2f,
            thicknessWorld = 0.01f,
            useWorldSpaceThickness = false,
            alpha = 1f,
            zTest = UnityEngine.Rendering.CompareFunction.LessEqual,
            zWrite = false,
            cullMode = UnityEngine.Rendering.CullMode.Front,
            useFresnel = false,
            fresnelPower = 3f,
            fresnelBias = 0f,
            usePulse = false,
            pulseAmplitude = 0.2f,
            pulseSpeed = 3f,
            edgeSoftness = 0f
        };
    }

    /// <summary>
    /// ScriptableObject to store default outline configuration project-wide.
    /// Optional, but useful to quickly tune global style.
    /// </summary>
    [CreateAssetMenu(fileName = "OutlineConfig", menuName = "AAA/Outline/OutlineConfig")]
    public class OutlineConfig : ScriptableObject
    {
        /// <summary>Default settings used if a component does not override per-instance values.</summary>
        public OutlineSettings defaultSettings = OutlineSettings.Default;

        /// <summary>Material template using "AAA/Effects/URP/PerObjectOutline" shader.</summary>
        public Material outlineMaterialTemplate;
    }
}
