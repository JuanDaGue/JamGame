using UnityEngine;
using GameJam.Core;

namespace GameJam.Systems
{
    public class PlayerLifeSystem : MonoBehaviour
    {
        private static PlayerLifeSystem _instance;
        public static PlayerLifeSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<PlayerLifeSystem>();
                return _instance;
            }
        }

        [SerializeField] private int maxLives = 3;
        [SerializeField] private int currentLives = 3;

        public int MaxLives => maxLives;
        public int CurrentLives => currentLives;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            currentLives = Mathf.Clamp(currentLives, 0, maxLives);

            // Notifica estado inicial
            GameEvents.OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void SetMaxLives(int newMax, bool refill = true)
        {
            maxLives = Mathf.Max(1, newMax);
            if (refill) currentLives = maxLives;
            else currentLives = Mathf.Clamp(currentLives, 0, maxLives);

            GameEvents.OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void TakeDamage(int amount = 1)
        {
            if (amount <= 0) return;
            if (currentLives <= 0) return;

            currentLives = Mathf.Max(0, currentLives - amount);
            GameEvents.OnLivesChanged?.Invoke(currentLives, maxLives);

            if (currentLives == 0)
            {
                GameEvents.OnPlayerDied?.Invoke();
            }
        }

        public void Heal(int amount = 1)
        {
            if (amount <= 0) return;
            if (currentLives <= 0) return; // opcional: no curar si está muerto

            currentLives = Mathf.Min(maxLives, currentLives + amount);
            GameEvents.OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void Respawn(int livesToRestore = -1)
        {
            currentLives = (livesToRestore < 0) ? maxLives : Mathf.Clamp(livesToRestore, 1, maxLives);
            GameEvents.OnLivesChanged?.Invoke(currentLives, maxLives);
        }
    }
}
