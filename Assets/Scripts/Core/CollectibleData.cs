using UnityEngine;

namespace GameJam.Core
{
    [CreateAssetMenu(fileName = "NewCollectible", menuName = "GameJam/Collectible Data")]
    public class CollectibleData : ScriptableObject
    {
        [Tooltip("Identificador único para el coleccionable (ej: 'LLAVE_ROJA', 'AMULETO_FUEGO')")]
        [SerializeField] private string id;
        public string Id => id;

        [Tooltip("Nombre visible para el jugador")]
        [SerializeField] private string displayName;
        public string DisplayName => displayName;

        [Tooltip("Icono para mostrar en UI")]
        [SerializeField] private Sprite icon;
        public Sprite Icon => icon;

        [Tooltip("Descripción opcional")]
        [TextArea]
        [SerializeField] private string description;
        public string Description => description;

        // Sobrescribimos Equals para facilitar comparaciones
        public override bool Equals(object other)
        {
            if (other is CollectibleData otherData)
            {
                return this.id == otherData.id;
            }
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return id?.GetHashCode() ?? 0;
        }
    }
}
