using System.Linq;
using UnityEngine;

namespace AAA.Outline
{
    /// <summary>
    /// Draws helpful gizmos for outline debugging: inflated bounds approximation and sampled normals.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class OutlineGizmoDrawer : MonoBehaviour
    {
        public OutlineComponent outline;

        /// <summary>Number of normals to sample from the mesh for visualization.</summary>
        [Min(0)] public int normalSamples = 24;

        /// <summary>Length of normal rays in world units.</summary>
        [Min(0f)] public float normalRayLength = 0.05f;

        /// <summary>Color for normal rays gizmos.</summary>
        public Color normalsColor = new Color(0f, 1f, 1f, 0.8f);

        /// <summary>Color for inflated bounds gizmo.</summary>
        public Color boundsColor = new Color(1f, 0.2f, 0f, 0.6f);

        private void OnDrawGizmosSelected()
        {
            if (outline == null) outline = GetComponent<OutlineComponent>();
            if (outline == null) return;

            var rend = outline.GetComponent<Renderer>();
            if (rend == null) return;

            // Draw bounds inflated approximation by "thicknessWorld" or by pixels approx using distance.
            Bounds b = rend.bounds;
            float inflate = outline.useWorldSpaceThickness ? outline.thicknessWorld : PixelToWorldApprox(rend, outline.thicknessPixels);
            inflate = Mathf.Max(0f, inflate);
            var inflated = new Bounds(b.center, b.size + Vector3.one * inflate * 2f);

            Gizmos.color = boundsColor;
            Gizmos.DrawWireCube(inflated.center, inflated.size);

            // Sample some normals if mesh present
            var mf = rend.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && normalSamples > 0)
            {
                var mesh = mf.sharedMesh;
                var verts = mesh.vertices;
                var norms = mesh.normals;
                if (verts != null && norms != null && verts.Length == norms.Length && verts.Length > 0)
                {
                    Gizmos.color = normalsColor;
                    int step = Mathf.Max(1, verts.Length / Mathf.Max(1, normalSamples));
                    Matrix4x4 localToWorld = rend.localToWorldMatrix;
                    for (int i = 0; i < verts.Length; i += step)
                    {
                        Vector3 p = localToWorld.MultiplyPoint3x4(verts[i]);
                        Vector3 n = localToWorld.MultiplyVector(norms[i]).normalized;
                        Gizmos.DrawRay(p, n * normalRayLength);
                    }
                }
            }
        }

        private float PixelToWorldApprox(Renderer rend, float pixels)
        {
            if (Camera.current == null) return 0f;
            // Approx: project 1-pixel vertical span at renderer center depth to world units
            Vector3 center = rend.bounds.center;
            float dist = Vector3.Dot(center - Camera.current.transform.position, Camera.current.transform.forward);
            dist = Mathf.Abs(dist);
            if (dist < 1e-4f) return 0f;

            float fovY = Camera.current.fieldOfView * Mathf.Deg2Rad;
            float worldPerPixelY = 2f * dist * Mathf.Tan(fovY * 0.5f) / Mathf.Max(1, Camera.current.pixelHeight);
            return pixels * worldPerPixelY;
        }
    }
}
