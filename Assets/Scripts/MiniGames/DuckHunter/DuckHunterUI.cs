using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.MiniGames.DuckHunter
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DuckHunterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private Image targetIconDisplay; // Para mostrar qué buscar (con el engaño)

        [Header("Iconos (Placeholders)")]
        [SerializeField] private Sprite iconTargetA;
        [SerializeField] private Sprite iconTargetB;
        [SerializeField] private Sprite iconTargetC;

        [Header("Fade Settings")]
        [Range(0.1f, 2f)]
        public float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[DuckHunterUI] CanvasGroup missing. Auto-adding.");
#endif
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void SetInstruction(EnemyType decoyType)
        {
            if (instructionText != null)
            {
                // Mensaje MENTIROSO (La UI dice el color incorrecto)
                string name = decoyType switch
                {
                    EnemyType.Duck => "PATOS",
                    EnemyType.Balloon => "GLOBOS",
                    EnemyType.Bird => "AVES",
                    _ => "OBJETIVOS"
                };

                instructionText.text = $"OBJETIVO: ELIMINA A LOS {name}";
            }

            if (targetIconDisplay != null)
            {
                // Icono del tipo mentiroso
                targetIconDisplay.color = decoyType switch
                {
                    EnemyType.Duck => Color.green,
                    EnemyType.Balloon => Color.red,
                    EnemyType.Bird => Color.blue,
                    _ => Color.white
                };
            }
        }

        public void UpdateScore()
        {
            // Feedback opcional (podrías añadir un texto de puntuación aquí)
        }

        public void UpdateWave(int current, int total)
        {
            if (waveText != null)
            {
                waveText.text = $"Oleada {current}/{total}";
            }
        }

        /// <summary>
        /// Muestra la UI con fade in
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            FadeTo(1f);
        }

        /// <summary>
        /// Oculta la UI con fade out
        /// </summary>
        public void Hide()
        {
            FadeTo(0f, () => gameObject.SetActive(false));
        }

        /// <summary>
        /// Fade a un valor de alpha específico
        /// </summary>
        public void FadeTo(float targetAlpha, System.Action onComplete = null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
        }

        private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Habilitar/deshabilitar interacción
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }
}
