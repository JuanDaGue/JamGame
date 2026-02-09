using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.MiniGames.DuckHunter
{
    public class GameOverGunSequence : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Arma izquierda (Weapon_01)")]
        [SerializeField] private Transform leftWeapon;
        [Tooltip("Arma derecha (Weapon_02)")]
        [SerializeField] private Transform rightWeapon;
        [Tooltip("Prefab de VFX Sparks")]
        [SerializeField] private GameObject sparksPrefab;
        [Tooltip("Cámara objetivo (se auto-asigna a Camera.main si no se especifica)")]
        [SerializeField] private Camera targetCamera;

        [Header("Posición de Cañones")]
        [Tooltip("Hijo de Weapon_01 que marca la boca del cañón (arrastra MuzzlePoint desde la jerarquía)")]
        [SerializeField] private Transform leftMuzzlePoint;
        [Tooltip("Hijo de Weapon_02 que marca la boca del cañón (arrastra MuzzlePoint desde la jerarquía)")]
        [SerializeField] private Transform rightMuzzlePoint;

        [Header("Timing")]
        [Tooltip("Duración de la rotación de las armas hacia la cámara")]
        [SerializeField] private float rotationDuration = 0.5f;
        [Tooltip("Pausa después de rotar antes de disparar")]
        [SerializeField] private float shootDelay = 0.3f;
        [Tooltip("Pausa después del disparo antes de cambiar escena")]
        [SerializeField] private float sceneTransitionDelay = 1f;

        [Header("Eventos")]
        [Tooltip("Se dispara cuando la secuencia completa termina")]
        public UnityEvent onSequenceComplete;

        private bool isPlaying = false;

        public void TriggerGameOverSequence()
        {
            if (isPlaying) return;

            if (targetCamera == null)
            {
                // Para Cinemachine, buscamos el GameObject con tag MainCamera
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj != null)
                    targetCamera = camObj.GetComponent<Camera>();
            }

            if (leftWeapon == null || rightWeapon == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[GameOverGunSequence] Weapons not assigned!");
#endif
                onSequenceComplete?.Invoke();
                return;
            }

            StartCoroutine(GameOverSequenceRoutine());
        }

        private IEnumerator GameOverSequenceRoutine()
        {
            isPlaying = true;

#if UNITY_EDITOR
            Debug.Log("[GameOverGunSequence] Starting Game Over sequence...");
#endif

            // 1. Desactivar input del jugador (opcional, el manager ya debería haberlo hecho)
            DuckHighnoonInput playerInput = FindFirstObjectByType<DuckHighnoonInput>();
            if (playerInput != null)
                playerInput.enabled = false;

            // 2. Rotar armas hacia la cámara
            yield return StartCoroutine(RotateWeaponsToCamera());

            // 3. Pausa breve antes de disparar
            yield return new WaitForSeconds(shootDelay);

            // 4. Disparar VFX
            PlayShootVFX();

            // 5. Pausa antes de transición
            yield return new WaitForSeconds(sceneTransitionDelay);

#if UNITY_EDITOR
            Debug.Log("[GameOverGunSequence] Sequence complete. Invoking event.");
#endif

            // 6. Invocar evento de finalización
            onSequenceComplete?.Invoke();

            isPlaying = false;
        }

        private IEnumerator RotateWeaponsToCamera()
        {
            // Rotación objetivo para Weapon_01 (ajustable en código)
            Quaternion leftTarget = Quaternion.Euler(0f, 59.6f, -21.137f);

            // Rotación objetivo para Weapon_02
            Quaternion rightTarget = Quaternion.Euler(13.067f, -245.0f, -28.11f);

            // Guardar rotaciones iniciales
            Quaternion leftStart = leftWeapon.rotation;
            Quaternion rightStart = rightWeapon.rotation;

            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                leftWeapon.rotation = Quaternion.Lerp(leftStart, leftTarget, smoothT);
                rightWeapon.rotation = Quaternion.Lerp(rightStart, rightTarget, smoothT);

                yield return null;
            }

            // Asegurar rotación final exacta
            leftWeapon.rotation = leftTarget;
            rightWeapon.rotation = rightTarget;

#if UNITY_EDITOR
            Debug.Log("[GameOverGunSequence] Both weapons rotated to target positions.");
#endif
        }

        private void PlayShootVFX()
        {
            if (sparksPrefab == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[GameOverGunSequence] Sparks prefab not assigned!");
#endif
                return;
            }

            // Usar Transform hijos asignados manualmente
            Vector3 leftPos = leftMuzzlePoint != null ? leftMuzzlePoint.position : leftWeapon.position;
            Vector3 rightPos = rightMuzzlePoint != null ? rightMuzzlePoint.position : rightWeapon.position;

            Quaternion leftRot = leftMuzzlePoint != null ? leftMuzzlePoint.rotation : leftWeapon.rotation;
            Quaternion rightRot = rightMuzzlePoint != null ? rightMuzzlePoint.rotation : rightWeapon.rotation;

            GameObject leftVFX = Instantiate(sparksPrefab, leftPos, leftRot);
            GameObject rightVFX = Instantiate(sparksPrefab, rightPos, rightRot);

            // Asegurar que las partículas se reproduzcan una sola vez
            ConfigureOneTimePlayback(leftVFX);
            ConfigureOneTimePlayback(rightVFX);

#if UNITY_EDITOR
            Debug.Log($"[GameOverGunSequence] Sparks VFX spawned. Left: {leftMuzzlePoint?.name}, Right: {rightMuzzlePoint?.name}");
#endif
        }

        private void ConfigureOneTimePlayback(GameObject vfxObject)
        {
            if (vfxObject == null) return;

            // Obtener todos los sistemas de partículas (incluidos hijos)
            ParticleSystem[] particleSystems = vfxObject.GetComponentsInChildren<ParticleSystem>();

            float maxDuration = 0f;

            foreach (ParticleSystem ps in particleSystems)
            {
                // Desactivar loop
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;

                // Calcular duración total (duración + lifetime)
                float totalDuration = main.duration + main.startLifetime.constantMax;
                if (totalDuration > maxDuration)
                    maxDuration = totalDuration;
            }

            // Destruir el GameObject después de que todas las partículas terminen
            Destroy(vfxObject, maxDuration + 0.5f);
        }
    }
}
