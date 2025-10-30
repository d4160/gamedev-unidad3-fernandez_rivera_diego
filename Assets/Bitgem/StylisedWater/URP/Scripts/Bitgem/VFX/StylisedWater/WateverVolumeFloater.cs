using UnityEngine;

namespace Bitgem.VFX.StylisedWater
{
    [RequireComponent(typeof(Rigidbody))]
    public class WateverVolumeFloater : MonoBehaviour
    {
        public WaterVolumeHelper WaterVolumeHelper = null;
        public float BuoyancyForce = 10f;
        public float WaterDrag = 1f;
        public float WaveScaleMultiplier = 2f;

        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            var instance = WaterVolumeHelper ? WaterVolumeHelper : WaterVolumeHelper.Instance;
            if (!instance) return;

            var _WaveFrequency = instance.MeshRenderer.sharedMaterial.GetFloat("_WaveFrequency");
            var _WaveScale = instance.MeshRenderer.sharedMaterial.GetFloat("_WaveScale");
            var _WaveSpeed = instance.MeshRenderer.sharedMaterial.GetFloat("_WaveSpeed");

            float time = Time.time * _WaveSpeed;
            float baseHeight = instance.GetHeight(transform.position) ?? transform.position.y;

            // Altura + ola
            float waveHeight = baseHeight + GetWaveHeight(transform.position, time, _WaveFrequency, _WaveScale);

            // Desplazamiento horizontal
            Vector3 horizOffset = GetWaveDisplacement(transform.position, time, _WaveFrequency, _WaveScale, 0.5f);

            // Nueva posición objetivo
            Vector3 targetPos = new Vector3(
                transform.position.x + horizOffset.x,
                waveHeight,
                transform.position.z + horizOffset.z
            );

            // Fuerza de flotación
            float depth = waveHeight - transform.position.y;
            if (depth > 0f)
            {
                rb.AddForce(Vector3.up * BuoyancyForce * depth, ForceMode.Acceleration);
            }

            // Oscilación lateral
            Vector3 normal = GetWaveNormal(transform.position, time, _WaveFrequency, _WaveScale);
            Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, normal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 2f));

            // Movimiento horizontal suave
            rb.MovePosition(Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime));
        }
        
        float GetWaveHeight(Vector3 pos, float time, float freq, float scale)
        {
            return (Mathf.Sin(pos.x * freq + time) + Mathf.Cos(pos.z * freq + time)) * scale;
        }

        Vector3 GetWaveDisplacement(Vector3 pos, float time, float freq, float scale, float horizScale)
        {
            // Derivadas parciales de la ola
            float dx = Mathf.Cos(pos.x * freq + time) * scale * freq;
            float dz = -Mathf.Sin(pos.z * freq + time) * scale * freq;

            // Usamos la pendiente como desplazamiento horizontal
            return new Vector3(dx, 0f, dz) * horizScale;
        }

        Vector3 GetWaveNormal(Vector3 pos, float time, float freq, float scale)
        {
            float dx = Mathf.Cos(pos.x * freq + time) * scale * freq;
            float dz = -Mathf.Sin(pos.z * freq + time) * scale * freq;
            return new Vector3(-dx, 1f, -dz).normalized;
        }
    }
}
