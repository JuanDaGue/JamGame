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
                Debug.LogError("[DuckHunterUI] No se encontró CanvasGroup. Agregándolo automáticamente.");
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void SetInstruction(TargetColor decoyColor)
        {
            if (instructionText != null)
            {
                // Este es el mensaje MENTIROSO - La UI dice que dispares al color equivocado
                string colorName = decoyColor switch
                {
                    TargetColor.Green => "VERDES",
                    TargetColor.Red => "ROJOS",
                    TargetColor.Blue => "AZULES",
                    _ => "OBJETIVOS"
                };

                instructionText.text = $"OBJETIVO: ELIMINA A LOS {colorName}";
            }

            if (targetIconDisplay != null)
            {
                // Cambiar color del icono según el tipo mentiroso
                targetIconDisplay.color = decoyColor switch
                {
                    TargetColor.Green => Color.green,
                    TargetColor.Red => Color.red,
                    TargetColor.Blue => Color.blue,
                    _ => Color.white
                };
            }
        }

        public void UpdateScore(int score)
        {
            // Feedback opcional (podrías añadir un texto de puntuación aquí)
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
