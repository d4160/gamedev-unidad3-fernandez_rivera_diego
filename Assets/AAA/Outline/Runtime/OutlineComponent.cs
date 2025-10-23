using UnityEngine;
using UnityEngine.Rendering;

namespace AAA.Outline
{
    /// <summary>
    /// High-level MonoBehaviour to attach outline behavior to a GameObject.
    /// It orchestrates OutlineService usage and exposes a clean API (apply/remove/update).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class OutlineComponent : MonoBehaviour
    {
        /// <summary>Optional global config; if not assigned, defaults are used.</summary>
        public OutlineConfig config;

        /// <summary>Apply outline automatically on Start.</summary>
        public bool applyOnStart = true;

        /// <summary>Color (HDR) for the outline.</summary>
        public Color color = new Color(1f, 0.6f, 0f, 1f);

        /// <summary>Thickness in pixels (used when <see cref="useWorldSpaceThickness"/> is false).</summary>
        public float thicknessPixels = 2f;

        /// <summary>Thickness in world units (used when <see cref="useWorldSpaceThickness"/> is true).</summary>
        public float thicknessWorld = 0.01f;

        /// <summary>Use world-space thickness instead of screen-space.</summary>
        public bool useWorldSpaceThickness = false;

        /// <summary>Transparency multiplier.</summary>
        [Range(0f, 1f)] public float alpha = 1f;

        /// <summary>Depth comparison function.</summary>
        public CompareFunction zTest = CompareFunction.LessEqual;

        /// <summary>Write to depth buffer from outline pass.</summary>
        public bool zWrite = false;

        /// <summary>Culling mode; Front yields a classic silhouette.</summary>
        public CullMode cullMode = CullMode.Front;

        /// <summary>Enable fresnel rim contribution.</summary>
        public bool useFresnel = false;

        /// <summary>Fresnel power.</summary>
        public float fresnelPower = 3f;

        /// <summary>Fresnel bias.</summary>
        public float fresnelBias = 0f;

        /// <summary>Enable outline pulse over time.</summary>
        public bool usePulse = false;

        /// <summary>Pulse amplitude.</summary>
        public float pulseAmplitude = 0.2f;

        /// <summary>Pulse speed.</summary>
        public float pulseSpeed = 3f;

        /// <summary>Alpha fade based on rim factor (0..1).</summary>
        [Range(0f, 1f)] public float edgeSoftness = 0f;

        private Renderer _renderer;
        private OutlineService _service;
        private bool _isApplied;

        /// <summary>Returns whether the outline is currently applied to this renderer.</summary>
        public bool IsApplied => _isApplied;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                UnityGameLogger.Error(nameof(OutlineComponent), "Renderer not found.", this);
                enabled = false;
                return;
            }

            var matTemplate = ResolveOutlineMaterialTemplate();
            _service = new OutlineService(matTemplate);
        }

        private void Start()
        {
            if (applyOnStart)
            {
                Apply();
            }
        }

        /// <summary>
        /// Creates an OutlineSettings from current public fields; used to push MPB.
        /// </summary>
        public OutlineSettings BuildSettings()
        {
            return new OutlineSettings
            {
                color = color,
                thicknessPixels = Mathf.Max(0f, thicknessPixels),
                thicknessWorld = Mathf.Max(0f, thicknessWorld),
                useWorldSpaceThickness = useWorldSpaceThickness,
                alpha = Mathf.Clamp01(alpha),
                zTest = zTest,
                zWrite = zWrite,
                cullMode = cullMode,
                useFresnel = useFresnel,
                fresnelPower = Mathf.Max(0f, fresnelPower),
                fresnelBias = fresnelBias,
                usePulse = usePulse,
                pulseAmplitude = pulseAmplitude,
                pulseSpeed = pulseSpeed,
                edgeSoftness = edgeSoftness
            };
        }

        /// <summary>Applies the outline to this GameObject's Renderer.</summary>
        public void Apply()
        {
            if (_isApplied) return;
            var settings = BuildSettings();
            _service.ApplyOutline(_renderer, settings);
            _isApplied = true;
        }

        /// <summary>Removes the outline from this GameObject's Renderer.</summary>
        public void Remove()
        {
            if (!_isApplied) return;
            _service.RemoveOutline(_renderer);
            _isApplied = false;
        }

        /// <summary>Updates outline MPB values without changing materials.</summary>
        public void Refresh()
        {
            if (!_isApplied) return;
            var settings = BuildSettings();
            _service.UpdateOutline(_renderer, settings);
        }

        /// <summary>Toggles outline on/off.</summary>
        public void Toggle()
        {
            if (_isApplied) Remove();
            else Apply();
        }

        /// <summary>Sets the outline color and refreshes if applied.</summary>
        public void SetColor(Color newColor)
        {
            color = newColor;
            Refresh();
        }

        /// <summary>Sets thickness in pixels (or world units if useWorldSpaceThickness) and refreshes.</summary>
        public void SetThickness(float value)
        {
            if (useWorldSpaceThickness) thicknessWorld = Mathf.Max(0f, value);
            else thicknessPixels = Mathf.Max(0f, value);
            Refresh();
        }

        private Material ResolveOutlineMaterialTemplate()
        {
            if (config != null && config.outlineMaterialTemplate != null)
                return config.outlineMaterialTemplate;

            // Fallback: attempt to find by shader name
            var shader = Shader.Find("AAA/Effects/URP/PerObjectOutline");
            if (shader == null)
            {
                UnityGameLogger.Error(nameof(OutlineComponent), "Shader 'AAA/Effects/URP/PerObjectOutline' not found. Did you add the shader file?", this);
                return null;
            }
            var mat = new Material(shader)
            {
                name = "Outline (Runtime Template)"
            };
            return mat;
        }
    }
}