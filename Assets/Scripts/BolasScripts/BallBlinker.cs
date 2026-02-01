using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBlinker : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Renderers de las bolas que quieres hacer parpadear.")]
    public List<Renderer> ballRenderers = new List<Renderer>();

    [Header("Blink Settings")]
    public int blinkCount = 3;
    public float blinkOnTime = 0.12f;
    public float blinkOffTime = 0.12f;

    [Tooltip("Color al parpadear (rojo bomba).")]
    public Color blinkColor = Color.red;

    // cache
    private MaterialPropertyBlock _mpb;
    private readonly int _colorId = Shader.PropertyToID("_BaseColor"); // URP Lit
    private readonly int _colorIdAlt = Shader.PropertyToID("_Color");  // Built-in/Standard

    private List<Color> _originalColors = new List<Color>();

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        CacheOriginalColors();
    }

    void CacheOriginalColors()
    {
        _originalColors.Clear();

        foreach (var r in ballRenderers)
        {
            if (!r) { _originalColors.Add(Color.white); continue; }

            // intenta leer color “real”
            Color c = Color.white;
            if (r.sharedMaterial != null)
            {
                if (r.sharedMaterial.HasProperty(_colorId)) c = r.sharedMaterial.GetColor(_colorId);
                else if (r.sharedMaterial.HasProperty(_colorIdAlt)) c = r.sharedMaterial.GetColor(_colorIdAlt);
            }
            _originalColors.Add(c);
        }
    }

    public IEnumerator BlinkRoutine()
    {
        if (ballRenderers == null || ballRenderers.Count == 0)
            yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            SetColorAll(blinkColor);
            yield return new WaitForSeconds(blinkOnTime);

            RestoreOriginalColors();
            yield return new WaitForSeconds(blinkOffTime);
        }
    }

    void SetColorAll(Color c)
    {
        for (int i = 0; i < ballRenderers.Count; i++)
        {
            var r = ballRenderers[i];
            if (!r) continue;

            r.GetPropertyBlock(_mpb);

            // setea ambos por compatibilidad
            _mpb.SetColor(_colorId, c);
            _mpb.SetColor(_colorIdAlt, c);

            r.SetPropertyBlock(_mpb);
        }
    }

    void RestoreOriginalColors()
    {
        for (int i = 0; i < ballRenderers.Count; i++)
        {
            var r = ballRenderers[i];
            if (!r) continue;

            var c = (i < _originalColors.Count) ? _originalColors[i] : Color.white;

            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorId, c);
            _mpb.SetColor(_colorIdAlt, c);
            r.SetPropertyBlock(_mpb);
        }
    }
}
