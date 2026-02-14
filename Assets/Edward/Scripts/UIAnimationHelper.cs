using System.Collections;
using UnityEngine;

public class UIAnimationHelper : MonoBehaviour
{
    private static UIAnimationHelper _instance;

    public static UIAnimationHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UIAnimationHelper>();

                if (_instance == null)
                {
                    Debug.LogError("UIAnimationHelper no encontrado en la escena. Asegúrate de colocarlo en un GameObject.");
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayPopAnimation(GameObject target, float duracion, Vector3 escalaOriginal, float multiplicadorEscala)
    {
        if (target == null) return;
        StopAllCoroutines();
        StartCoroutine(AnimPopRoutine(target.transform, duracion, escalaOriginal, multiplicadorEscala));
    }

    private IEnumerator AnimPopRoutine(Transform trans, float duracion, Vector3 escalaOriginal, float multiplicadorEscala)
    {
        float mitadTiempo = duracion / 2f;
        float tiempo = 0f;

        Vector3 escalaPico = escalaOriginal * multiplicadorEscala;

        // Fase 1: Hacia el pico
        while (tiempo < mitadTiempo)
        {
            if (trans == null) yield break;
            tiempo += Time.deltaTime;
            float t = tiempo / mitadTiempo;
            trans.localScale = Vector3.Lerp(escalaOriginal, escalaPico, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        tiempo = 0f;

        // Fase 2: De vuelta a la base
        while (tiempo < mitadTiempo)
        {
            if (trans == null) yield break;
            tiempo += Time.deltaTime;
            float t = tiempo / mitadTiempo;
            trans.localScale = Vector3.Lerp(escalaPico, escalaOriginal, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        if (trans != null) trans.localScale = escalaOriginal;
    }
}