using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameJam.Core;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private List<Image> lifeIcons = new List<Image>(); // arrastra corazones aquí

    private void OnEnable()
    {
        GameEvents.OnLivesChanged += OnLivesChanged;
        GameEvents.OnPlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.OnLivesChanged -= OnLivesChanged;
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }

    private void OnLivesChanged(int current, int max)
    {
        // Si tienes más icons que max, las sobrantes se apagan.
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] == null) continue;

            bool shouldShow = i < max;
            lifeIcons[i].gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                // lleno si i < current
                lifeIcons[i].enabled = (i < current);
            }
        }
    }

    private void OnPlayerDied()
    {
        // Aquí puedes mostrar panel de muerte, animación, etc.
        // Ejemplo simple: log
        Debug.Log("[LivesUI] Player died!");
    }
}
