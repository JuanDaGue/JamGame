using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

public class CinemaCameraController : MonoBehaviour
{
    [System.Serializable]
    public class DatosCinematica
    {
        public string nombre = "Cinematica X";
        public SplineContainer caminoSpline;
        public float duracion = 5.0f;

        [Header("Rotaciones (X, Y, Z)")]
        [Tooltip("X, Y para rotación. Z se usará para el Dutch (Roll).")]
        public List<Vector3> rotacionesObjetivo;
    }

    [Header("Referencias")]
    public GameObject CinemachineCamera;
    public CinemachineSplineDolly splineDolly;
    public CinemachineCamera camaraCine;

    [Header("Configuración")]
    public bool reproducirAlIniciar = false;
    public List<DatosCinematica> listaCinematicas;

    private Coroutine _coroutineActual;
    private List<Vector3> _listaDeTrabajo;

    void Start()
    {
        if (splineDolly == null) splineDolly = GetComponent<CinemachineSplineDolly>();
        if (camaraCine == null) camaraCine = GetComponent<CinemachineCamera>();

        if (splineDolly == null || camaraCine == null || CinemachineCamera == null)
        {
            Debug.LogError("Faltan referencias (Dolly, Camera o el GameObject CinemachineCamera).");
            return;
        }

        splineDolly.PositionUnits = PathIndexUnit.Normalized;

        if (reproducirAlIniciar && listaCinematicas.Count > 0)
        {
            ReproducirHaciaElFinal(0);
        }
    }

    // ---------------- MÉTODOS PÚBLICOS ----------------

    public void ReproducirHaciaElFinal(int index)
    {
        IniciarCinematica(index, true);
    }

    public void ReproducirHaciaElInicio(int index)
    {
        IniciarCinematica(index, false);
    }

    // ---------------- LÓGICA INTERNA ----------------

    private void IniciarCinematica(int index, bool haciaElFinal)
    {
        if (index < 0 || index >= listaCinematicas.Count) return;

        if (_coroutineActual != null) StopCoroutine(_coroutineActual);

        DatosCinematica datos = listaCinematicas[index];

        if (datos.caminoSpline != null)
        {
            splineDolly.Spline = datos.caminoSpline;
        }

        _listaDeTrabajo = new List<Vector3>(datos.rotacionesObjetivo);

        // Configuración inicial inmediata (Frame 0)
        float startPos = haciaElFinal ? 0f : 1f;
        splineDolly.CameraPosition = startPos;

        if (_listaDeTrabajo.Count > 0)
        {
            int rotIndex = haciaElFinal ? 0 : _listaDeTrabajo.Count - 1;
            Vector3 initRot = _listaDeTrabajo[rotIndex];
            AplicarRotacion(Quaternion.Euler(initRot), initRot.z);
        }

        _coroutineActual = StartCoroutine(ProcesoSimultaneo(datos.duracion, haciaElFinal));
    }

    IEnumerator ProcesoSimultaneo(float duracion, bool haciaElFinal)
    {
        float tiempo = 0f;

        // Definimos dirección según el booleano
        float inicioPos = haciaElFinal ? 0f : 1f;
        float finPos = haciaElFinal ? 1f : 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            splineDolly.CameraPosition = Mathf.Lerp(inicioPos, finPos, t);

            if (_listaDeTrabajo != null && _listaDeTrabajo.Count > 0)
            {
                float tRotacion = haciaElFinal ? t : (1f - t);
                CalcularYAplicarRotacion(tRotacion);
            }

            yield return null;
        }

        splineDolly.CameraPosition = finPos;

        if (_listaDeTrabajo != null && _listaDeTrabajo.Count > 0)
        {
            int finalIndex = haciaElFinal ? _listaDeTrabajo.Count - 1 : 0;
            Vector3 finalRot = _listaDeTrabajo[finalIndex];
            AplicarRotacion(Quaternion.Euler(finalRot), finalRot.z);
        }

        _coroutineActual = null;
    }

    private void CalcularYAplicarRotacion(float t)
    {
        int count = _listaDeTrabajo.Count;

        if (count == 1)
        {
            AplicarRotacion(Quaternion.Euler(_listaDeTrabajo[0]), _listaDeTrabajo[0].z);
            return;
        }

        t = Mathf.Clamp01(t);
        float puntoFlotante = t * (count - 1);
        int indiceActual = Mathf.FloorToInt(puntoFlotante);
        int indiceSiguiente = Mathf.Min(indiceActual + 1, count - 1);
        float tSegmento = puntoFlotante - indiceActual;

        Vector3 eulerA = _listaDeTrabajo[indiceActual];
        Vector3 eulerB = _listaDeTrabajo[indiceSiguiente];

        Quaternion rotA = Quaternion.Euler(eulerA);
        Quaternion rotB = Quaternion.Euler(eulerB);
        Quaternion rotFinal = Quaternion.Slerp(rotA, rotB, tSegmento);

        float dutchFinal = Mathf.Lerp(eulerA.z, eulerB.z, tSegmento);

        AplicarRotacion(rotFinal, dutchFinal);
    }

    private void AplicarRotacion(Quaternion rotacion, float dutch)
    {
        CinemachineCamera.transform.rotation = rotacion;

        var lens = camaraCine.Lens;
        lens.Dutch = dutch;
        camaraCine.Lens = lens;
    }
}