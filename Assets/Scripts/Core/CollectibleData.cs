using UnityEngine;

namespace GameJam.Core
{
    [CreateAssetMenu(fileName = "NewCollectible", menuName = "GameJam/Collectible Data")]
    public class CollectibleData : ScriptableObject
    {
        [Tooltip("Identificador único para el coleccionable (ej: 'LLAVE_ROJA', 'AMULETO_FUEGO')")]
        public string id;

        [Tooltip("Nombre visible para el jugador")]
        public string displayName;

        [Tooltip("Icono para mostrar en UI")]
        public Sprite icon;

        [Tooltip("Descripción opcional")]
        [TextArea]
        public string description;

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
