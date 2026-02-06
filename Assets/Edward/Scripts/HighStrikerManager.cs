using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
// using UnityEngine.InputSystem; 

public class HighStrikerManager : MonoBehaviour
{
    [Header("Referencias Generales")]
    public Animator animatorJuego;

    [Header("UI Referencias")]
    public Slider sliderFuerza;
    public TextMeshProUGUI textoCentral;

    public RectTransform contenedorMouse; // El PADRE (Marco)
    public TextMeshProUGUI contadorMouse; // El HIJO (Texto)

    [Header("UI Tiempo")]
    public GameObject objetoTituloTiempo;
    public TextMeshProUGUI textoTiempo;

    [Header("Dificultad y Meta")]
    public float tiempoLimite = 5.0f;
    public int clicksMeta = 50;

    [Header("Ajustes de Seguridad")]
    public int capacidadMaximaBarra = 80;

    [Tooltip("Margen de error. Se ajusta con slider para evitar valores exagerados.")]
    [Range(0, 10)]
    public int tolerancia = 5;

    [Header("Configuración de Pereza")]
    public float tiempoMaxSinClick = 0.3f;
    public float periodoDeGracia = 1.0f;
    public float duracionMensajeYa = 1.0f;

    // Variables internas
    private int clicksActuales = 0;
    private float tiempoRestante;
    private float momentoUltimoClick;
    private bool juegoActivo = false;
    private bool periodoGraciaActivo = false;
    private bool descarrileActivo = false;

    private HighStriker_InputActions controls;

    private enum EstadoJuego { Idle, Countdown, Playing, Finished }
    private EstadoJuego estadoActual = EstadoJuego.Idle;

    void Awake()
    {
        controls = new HighStriker_InputActions();
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    void Start()
    {
        estadoActual = EstadoJuego.Idle;
        textoCentral.text = "Click para Iniciar";
        contadorMouse.text = "";

        // --- CAMBIO: Nos aseguramos de que empiece oculto por estética ---
        if (contenedorMouse != null)
            contenedorMouse.gameObject.SetActive(false);

        if (objetoTituloTiempo != null)
            objetoTituloTiempo.SetActive(false);

        if (textoTiempo != null)
            textoTiempo.text = tiempoLimite.ToString("F1") + "s";

        sliderFuerza.wholeNumbers = true;
        sliderFuerza.minValue = 0;
        sliderFuerza.maxValue = capacidadMaximaBarra;
        sliderFuerza.value = 0;
    }

    void Update()
    {
        switch (estadoActual)
        {
            case EstadoJuego.Idle:
                if (controls.Gameplay.Fire.triggered)
                {
                    StartCoroutine(RutinaCuentaRegresiva());
                }
                break;

            case EstadoJuego.Playing:
                LogicaDeJuego();
                break;
        }

        if (juegoActivo)
        {
            Vector2 mousePos = controls.Gameplay.Point.ReadValue<Vector2>();

            // Movemos el contenedor (el texto hijo lo seguirá)
            if (contenedorMouse != null)
            {
                contenedorMouse.position = mousePos + new Vector2(50, 50);
            }
            else
            {
                contadorMouse.transform.position = mousePos + new Vector2(50, 50);
            }
        }
    }

    IEnumerator RutinaCuentaRegresiva()
    {
        estadoActual = EstadoJuego.Countdown;

        if (animatorJuego != null) animatorJuego.SetTrigger("TriggerPreparado");

        textoCentral.text = "3";
        yield return new WaitForSeconds(1);
        textoCentral.text = "2";
        yield return new WaitForSeconds(1);
        textoCentral.text = "1";
        yield return new WaitForSeconds(1);

        EmpezarJuego();
    }

    void EmpezarJuego()
    {
        textoCentral.text = "¡YA!";
        clicksActuales = 0;
        sliderFuerza.value = 0;
        descarrileActivo = false;

        tiempoRestante = tiempoLimite;
        momentoUltimoClick = Time.time;

        estadoActual = EstadoJuego.Playing;
        juegoActivo = true;
        periodoGraciaActivo = true;

        // --- CAMBIO: Activamos el contenedor visual ahora que empieza el juego ---
        if (contenedorMouse != null)
            contenedorMouse.gameObject.SetActive(true);

        if (objetoTituloTiempo != null)
            objetoTituloTiempo.SetActive(true);

        Invoke("FinGracia", periodoDeGracia);
        Invoke("BorrarTextoCentral", duracionMensajeYa);
    }

    void FinGracia()
    {
        periodoGraciaActivo = false;
    }

    void BorrarTextoCentral()
    {
        if (estadoActual == EstadoJuego.Playing)
        {
            textoCentral.text = "";
        }
    }

    void LogicaDeJuego()
    {
        if (descarrileActivo) return;

        tiempoRestante -= Time.deltaTime;

        if (textoTiempo != null)
        {
            textoTiempo.text = Mathf.Max(0, tiempoRestante).ToString("F1") + "s";
        }

        if (tiempoRestante <= 0)
        {
            EvaluarResultado();
            return;
        }

        // --- INPUT ---
        if (controls.Gameplay.Fire.triggered)
        {
            clicksActuales++;
            momentoUltimoClick = Time.time;

            if (clicksActuales > capacidadMaximaBarra) clicksActuales = capacidadMaximaBarra;

            sliderFuerza.value = clicksActuales;
            contadorMouse.text = clicksActuales.ToString();

            int limiteSeguro = clicksMeta + tolerancia;

            if (clicksActuales > limiteSeguro && clicksActuales < capacidadMaximaBarra)
            {
                StartCoroutine(RutinaDescarrile());
            }
            else if (clicksActuales >= capacidadMaximaBarra)
            {
                EvaluarResultado();
            }
        }

        // --- PEREZA ---
        if (!periodoGraciaActivo)
        {
            if (Time.time - momentoUltimoClick > tiempoMaxSinClick)
            {
                if (clicksActuales > 0)
                {
                    Debug.Log("Reset por pereza");
                    clicksActuales = 0;
                    sliderFuerza.value = 0;
                    contadorMouse.text = "0";
                    momentoUltimoClick = Time.time;

                    // TODO: ANIMACION 
                    // if(animatorJuego != null) animatorJuego.SetTrigger("TriggerReset");
                }
            }
        }
    }

    IEnumerator RutinaDescarrile()
    {
        descarrileActivo = true;
        Debug.Log("¡MÁQUINA FUERA DE CONTROL!");

        while (clicksActuales < capacidadMaximaBarra)
        {
            clicksActuales++;
            sliderFuerza.value = clicksActuales;
            contadorMouse.text = clicksActuales.ToString();

            yield return new WaitForSeconds(0.05f);
        }

        EvaluarResultado();
    }

    void EvaluarResultado()
    {
        juegoActivo = false;
        estadoActual = EstadoJuego.Finished;
        textoCentral.text = "";

        // --- CAMBIO: Ocultamos el contenedor al terminar para limpiar la pantalla ---
        if (contenedorMouse != null)
            contenedorMouse.gameObject.SetActive(false);

        CancelInvoke("BorrarTextoCentral");
        StopAllCoroutines();

        int rangoMinimoVictoria = Mathf.Clamp(clicksMeta - tolerancia, 0, capacidadMaximaBarra);
        int rangoMaximoVictoria = Mathf.Clamp(clicksMeta + tolerancia, 0, capacidadMaximaBarra);

        Debug.Log($"Meta: {clicksMeta}, Rango Victoria: {rangoMinimoVictoria} - {rangoMaximoVictoria}");

        // TODO: ANIMACION 
        /*
        if (animatorJuego == null) 
        {
            Debug.LogError("¡Falta asignar el Animator en el Inspector!");
            return;
        }
        */

        if (descarrileActivo)
        {
            Debug.Log("RESULTADO FINAL: Accidente (Sobrecarga detectada)");
            textoCentral.text = "¡EXCESO DE FUERZA!";
            // TODO: ANIMACION
            // animatorJuego.SetTrigger("TriggerAccidente");
            return;
        }

        // Evaluación Normal
        if (clicksActuales < rangoMinimoVictoria)
        {
            Debug.Log("RESULTADO FINAL: Muy Débil");
            textoCentral.text = "¡Muy Débil!";
            // TODO: ANIMACION
            // animatorJuego.SetTrigger("TriggerDebil");
        }
        else if (clicksActuales > rangoMaximoVictoria)
        {
            Debug.Log("RESULTADO FINAL: Accidente (Exceso Manual)");
            textoCentral.text = "¡EXCESO DE FUERZA!";
            // TODO: ANIMACION
            // animatorJuego.SetTrigger("TriggerAccidente");
        }
        else
        {
            Debug.Log("RESULTADO FINAL: Victoria");
            textoCentral.text = "¡GANASTE!";
            // TODO: ANIMACION
            // animatorJuego.SetTrigger("TriggerVictoria");
        }

        //Reset del juego tras unos segundos
        Invoke("ReiniciarJuego", 3.0f);
    }

    void ReiniciarJuego()
    {
        estadoActual = EstadoJuego.Idle;
        textoCentral.text = "Click para Iniciar";
        contadorMouse.text = "";

        if (objetoTituloTiempo != null)
            objetoTituloTiempo.SetActive(false);

        if (textoTiempo != null)
            textoTiempo.text = tiempoLimite.ToString("F1") + "s";

        sliderFuerza.value = 0;
    }
}