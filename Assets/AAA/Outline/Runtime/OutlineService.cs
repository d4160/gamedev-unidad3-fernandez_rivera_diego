using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAA.Outline
{
    /// <summary>
    /// Service that applies/removes the outline material to a Renderer without altering its base material(s).
    /// It adds an extra material slot (outline-only shader) and controls its properties via MaterialPropertyBlock.
    /// This design decouples interaction logic from physical representation (mesh/collider).
    /// </summary>
    public sealed class OutlineService
    {
        private static readonly int _ColorId          = Shader.PropertyToID("_OutlineColor");
        private static readonly int _ThickPxId        = Shader.PropertyToID("_OutlineThickness");
        private static readonly int _ThickWorldId     = Shader.PropertyToID("_WorldThickness");
        private static readonly int _AlphaId          = Shader.PropertyToID("_Alpha");
        private static readonly int _ZTestId          = Shader.PropertyToID("_ZTest");
        private static readonly int _ZWriteId         = Shader.PropertyToID("_ZWrite");
        private static readonly int _CullId           = Shader.PropertyToID("_Cull");
        private static readonly int _FresnelPowerId   = Shader.PropertyToID("_FresnelPower");
        private static readonly int _FresnelBiasId    = Shader.PropertyToID("_FresnelBias");
        private static readonly int _PulseAmpId       = Shader.PropertyToID("_PulseAmplitude");
        private static readonly int _PulseSpeedId     = Shader.PropertyToID("_PulseSpeed");
        private static readonly int _EdgeSoftnessId   = Shader.PropertyToID("_EdgeSoftness");

        private const string _Category = "OutlineService";

        private readonly Dictionary<Renderer, Material[]> _originalMats = new();
        private readonly Dictionary<Renderer, MaterialPropertyBlock> _mpbCache = new();

        private readonly Material _outlineMaterialTemplate;

        /// <summary>
        /// Creates a new OutlineService with a material template.
        /// The template must use shader "AAA/Effects/URP/PerObjectOutline".
        /// </summary>
        public OutlineService(Material outlineMaterialTemplate)
        {
            _outlineMaterialTemplate = outlineMaterialTemplate;
        }

        /// <summary>
        /// Applies outline by adding an additional material to the renderer if not already added.
        /// Then pushes the desired settings via MaterialPropertyBlock on that additional slot.
        /// </summary>
        public void ApplyOutline(Renderer target, OutlineSettings settings)
        {
            if (target == null)
            {
                UnityGameLogger.Error(_Category, "ApplyOutline called with null Renderer.");
                return;
            }

            if (_outlineMaterialTemplate == null)
            {
                UnityGameLogger.Error(_Category, "Outline material template is null. Provide a valid material using 'AAA/Effects/URP/PerObjectOutline'.", target);
                return;
            }

            // Cache original materials when the first time we apply
            if (!_originalMats.ContainsKey(target))
            {
                _originalMats[target] = target.sharedMaterials;
            }

            // Ensure outline material present
            var mats = new List<Material>(target.sharedMaterials);
            bool hasOutline = false;
            foreach (var m in mats)
            {
                if (m != null && m.shader != null && m.shader.name == _outlineMaterialTemplate.shader.name)
                {
                    hasOutline = true; break;
                }
            }
            if (!hasOutline)
            {
                // Use the template directly; as we plan to override via MPB per renderer
                mats.Add(_outlineMaterialTemplate);
                target.sharedMaterials = mats.ToArray();
                UnityGameLogger.Debug(_Category, $"Added outline material to '{target.name}'.", target);
            }

            // Set properties via MPB on the LAST slot (the outline mat we just added or found)
            var mpb = GetOrCreateMPB(target);
            PushSettingsToMPB(mpb, settings);
            target.SetPropertyBlock(mpb, mats.Count - 1);

            // Enable/disable features via keywords (affects material, but safe on template since MPB covers values)
            SetupKeywordsOnMaterial(_outlineMaterialTemplate, settings);
        }

        /// <summary>
        /// Removes the outline material and restores original materials if they were cached.
        /// </summary>
        public void RemoveOutline(Renderer target)
        {
            if (target == null) return;

            if (_originalMats.TryGetValue(target, out var originals))
            {
                target.sharedMaterials = originals;
                _originalMats.Remove(target);
                _mpbCache.Remove(target);
                UnityGameLogger.Debug(_Category, $"Removed outline material from '{target.name}'.", target);
            }
            else
            {
                // Try best-effort: remove last outline slot if it's our shader
                var mats = new List<Material>(target.sharedMaterials);
                if (mats.Count > 0)
                {
                    var last = mats[^1];
                    if (last != null && last.shader != null && last.shader.name == _outlineMaterialTemplate.shader.name)
                    {
                        mats.RemoveAt(mats.Count - 1);
                        target.sharedMaterials = mats.ToArray();
                        UnityGameLogger.Warn(_Category, $"Removed outline material (no originals cached) from '{target.name}'.", target);
                    }
                }
            }
        }

        /// <summary>
        /// Updates outline settings (MPB) without altering materials array.
        /// </summary>
        public void UpdateOutline(Renderer target, OutlineSettings settings)
        {
            if (target == null) return;

            var mats = target.sharedMaterials;
            if (mats == null || mats.Length == 0)
            {
                UnityGameLogger.Warn(_Category, "UpdateOutline called but renderer has no materials.", target);
                return;
            }

            int outlineIndex = -1;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m != null && m.shader != null && m.shader.name == _outlineMaterialTemplate.shader.name)
                {
                    outlineIndex = i;
                    break;
                }
            }
            if (outlineIndex < 0)
            {
                UnityGameLogger.Warn(_Category, "UpdateOutline called but outline material not found on renderer.", target);
                return;
            }

            var mpb = GetOrCreateMPB(target);
            PushSettingsToMPB(mpb, settings);
            target.SetPropertyBlock(mpb, outlineIndex);
            SetupKeywordsOnMaterial(_outlineMaterialTemplate, settings);
        }

        private MaterialPropertyBlock GetOrCreateMPB(Renderer r)
        {
            if (!_mpbCache.TryGetValue(r, out var mpb))
            {
                mpb = new MaterialPropertyBlock();
                _mpbCache[r] = mpb;
            }
            return mpb;
        }

        private void PushSettingsToMPB(MaterialPropertyBlock mpb, OutlineSettings s)
        {
            mpb.SetColor(_ColorId, s.color);
            mpb.SetFloat(_ThickPxId, Mathf.Max(0f, s.thicknessPixels));
            mpb.SetFloat(_ThickWorldId, Mathf.Max(0f, s.thicknessWorld));
            mpb.SetFloat(_AlphaId, Mathf.Clamp01(s.alpha));
            mpb.SetFloat(_ZTestId, (float)s.zTest);
            mpb.SetFloat(_ZWriteId, s.zWrite ? 1f : 0f);
            mpb.SetFloat(_CullId, (float)s.cullMode);
            mpb.SetFloat(_FresnelPowerId, Mathf.Max(0f, s.fresnelPower));
            mpb.SetFloat(_FresnelBiasId, s.fresnelBias);
            mpb.SetFloat(_PulseAmpId, s.pulseAmplitude);
            mpb.SetFloat(_PulseSpeedId, s.pulseSpeed);
            mpb.SetFloat(_EdgeSoftnessId, Mathf.Clamp01(s.edgeSoftness));
        }

        private void SetupKeywordsOnMaterial(Material mat, OutlineSettings s)
        {
            if (mat == null) return;

            SetKeyword(mat, "_OUTLINE_WORLD_SPACE", s.useWorldSpaceThickness);
            SetKeyword(mat, "_OUTLINE_FRESNEL", s.useFresnel);
            SetKeyword(mat, "_OUTLINE_PULSE", s.usePulse);
        }

        private void SetKeyword(Material mat, string keyword, bool enabled)
        {
            if (enabled) mat.EnableKeyword(keyword);
            else mat.DisableKeyword(keyword);
        }
    }
}