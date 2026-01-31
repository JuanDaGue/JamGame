using UnityEngine;
using TMPro; // Asumiendo TextMeshPro
using UnityEngine.UI;

namespace GameJam.MiniGames.DuckHunter
{
    public class DuckHunterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI instructionText;
        public Image targetIconDisplay; // Para mostrar qué buscar (con el engaño)
        
        [Header("Iconos (Placeholders)")]
        // Podrías usar Sprites reales aquí
        public Sprite iconTargetA;
        public Sprite iconTargetB;
        public Sprite iconTargetC;

        public void SetInstruction(TargetType decoyType)
        {
            if (instructionText != null)
            {
                // Este es el mensaje MENTIROSO
                // "Tus objetivos son [X]" -> Donde X es el señuelo
                instructionText.text = $"OBJETIVO: ELIMINA A LOS {decoyType.ToString().ToUpper()}";
            }

            if (targetIconDisplay != null)
            {
                // Cambiar color o sprite según el tipo
                switch (decoyType)
                {
                    case TargetType.Real: targetIconDisplay.color = Color.green; break;
                    case TargetType.Decoy: targetIconDisplay.color = Color.red; break;
                    case TargetType.Neutral: targetIconDisplay.color = Color.gray; break;
                }
            }
        }

        public void UpdateScore(int score)
        {
            // Feedback opcional
        }
    }
}
