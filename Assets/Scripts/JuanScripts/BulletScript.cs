using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private float lifetime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}