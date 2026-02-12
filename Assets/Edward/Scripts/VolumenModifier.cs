using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VolumeModifier : MonoBehaviour
{
    [Header("Configuración")]
    public Volume globalVolume;
    public float duracionEfecto = 2.0f;

    private ChromaticAberration _chromaticAberration;
    private ChannelMixer _channelMixer;

    private Coroutine _coroutineActual;

    void Start()
    {
        if (globalVolume == null) globalVolume = GetComponent<Volume>();

        // Obtenemos las referencias una sola vez al inicio
        globalVolume.profile.TryGet(out _chromaticAberration);
        globalVolume.profile.TryGet(out _channelMixer);
    }

    // ---------------- MÉTODOS PÚBLICOS ----------------
    public void ActivarEfecto()
    {
        if (_coroutineActual != null) StopCoroutine(_coroutineActual);

        _coroutineActual = StartCoroutine(TransicionValores(0f, 1f, 100f, 200f));
    }

    public void ResetEfecto()
    {
        if (_coroutineActual != null) StopCoroutine(_coroutineActual);

        _coroutineActual = StartCoroutine(TransicionValores(1f, 0f, 200f, 100f));
    }


    IEnumerator TransicionValores(float startChrom, float endChrom, float startRed, float endRed)
    {
        if (_chromaticAberration == null || _channelMixer == null)
        {
            Debug.LogWarning("Faltan componentes en el Volume Profile");
            yield break;
        }

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionEfecto)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionEfecto;

            t = Mathf.SmoothStep(0f, 1f, t);

            _chromaticAberration.intensity.value = Mathf.Lerp(startChrom, endChrom, t);

            _channelMixer.redOutRedIn.value = Mathf.Lerp(startRed, endRed, t);

            yield return null;
        }

        _chromaticAberration.intensity.value = endChrom;
        _channelMixer.redOutRedIn.value = endRed;

        _coroutineActual = null;
    }
}