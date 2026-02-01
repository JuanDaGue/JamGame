using System.Collections;
using UnityEngine;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHunterVFX : MonoBehaviour
    {
        [Header("Particle Prefabs")]
        [SerializeField] private GameObject hitRealVFX;    // Confeti / Estrellas
        [SerializeField] private GameObject hitDecoyVFX;   // Humo / Calavera
        [SerializeField] private GameObject hitNeutralVFX; // Chispas genéricas

        [Header("Screen Shake")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 0.3f;
        [Tooltip("Tiempo en segundos antes de destruir las partículas")]
        [SerializeField] private float vfxLifetime = 2.0f;

        public void PlayHitVFX(Vector3 position, TargetType type)
        {
            GameObject prefabToSpawn = type switch
            {
                TargetType.Real => hitRealVFX,
                TargetType.Decoy => hitDecoyVFX,
                TargetType.Neutral => hitNeutralVFX,
                _ => null
            };

            if (prefabToSpawn != null)
            {
                GameObject instance = Instantiate(prefabToSpawn, position, Quaternion.identity);
                Destroy(instance, vfxLifetime);
            }

            // Shake fuerte si es error, suave si es acierto
            if (type == TargetType.Decoy || type == TargetType.Neutral)
            {
                StartCoroutine(Shake(shakeDuration, shakeMagnitude));
            }
            else
            {
                StartCoroutine(Shake(shakeDuration * 0.5f, shakeMagnitude * 0.3f));
            }
        }

        private IEnumerator Shake(float duration, float magnitude)
        {
            if (cameraTransform == null) yield break;

            Vector3 originalPos = cameraTransform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                cameraTransform.localPosition = originalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraTransform.localPosition = originalPos;
        }
    }
}
