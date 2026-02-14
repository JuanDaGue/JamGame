using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HighStrikerManager : MonoBehaviour
{
    [Header("Referencias de Animadores")]
    public Animator animatorMartillo;
    public Animator animatorBarra;
    public Animator animatorCarro;

    [Header("Referencias de Efectos")]
    public VolumeModifier volumeModifier;

    [Header("Referencias de Cinemática")]
    public CinemaCameraController cinemaCameraController;

    [Header("Referencias de UI")]
    public Button botonDeInicio;
    public Slider sliderFuerza;
    public TextMeshProUGUI textoCentral;
    public Image botonVolver;
    public RectTransform contenedorMouse;
    public TextMeshProUGUI contadorMouse;
    public GameObject objetoTituloTiempo;
    public TextMeshProUGUI textoTiempo;

    [Header("Referencias de UIAnimations")]
    public GameObject SliderObject;
    public GameObject TextoCentralObject;

    [Header("Dificultad y Meta")]
    public float tiempoLimite = 5.0f;
    public int clicksMeta = 50;
    public int tiempoParaReiniciar = 7;

    [Header("Ajustes de Seguridad")]
    public float tiempoParaIniciar = 3.5f;
    public int capacidadMaximaBarra = 80;

    [Tooltip("Margen de error.")]
    [Range(0, 10)]
    public int tolerancia = 5;

    [Header("Configuración de Pereza")]
    [Range(0.1f, 1.0f)]
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
    private bool accidenteOcurrido = false;
    private bool victoriaObtenida = false;
    private bool golpeDebilOcurrido = false;

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

    IEnumerator Start()
    {
        yield return new WaitForSeconds(tiempoParaIniciar);

        estadoActual = EstadoJuego.Idle;

        textoCentral.text = "Click para iniciar";
        contadorMouse.text = "";

        if (contenedorMouse != null)
            contenedorMouse.gameObject.SetActive(false);

        if (objetoTituloTiempo != null)
            objetoTituloTiempo.SetActive(false);

        sliderFuerza.wholeNumbers = true;
        sliderFuerza.minValue = 0;
        sliderFuerza.maxValue = capacidadMaximaBarra;
        sliderFuerza.value = 0;

        botonVolver.enabled = true;

        if (botonDeInicio != null) botonDeInicio.enabled = true;

        if (SliderObject != null) UIAnimationHelper.Instance.PlayPopAnimation(SliderObject, 0.5f, Vector3.one * 4, 1.2f);
        if (TextoCentralObject != null) UIAnimationHelper.Instance.PlayPopAnimation(TextoCentralObject, 0.5f, Vector3.one, 1.2f);
    }

    void Update()
    {
        switch (estadoActual)
        {
            case EstadoJuego.Idle:
                break;

            case EstadoJuego.Playing:
                LogicaDeJuego();
                break;
        }

        if (juegoActivo)
        {
            Vector2 mousePos = controls.Gameplay.Point.ReadValue<Vector2>();

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

    public void IniciarSecuencia()
    {
        if (estadoActual == EstadoJuego.Idle)
        {
            if (botonDeInicio != null) botonDeInicio.enabled = false;
            StartCoroutine(RutinaCuentaRegresiva());
        }
    }

    IEnumerator RutinaCuentaRegresiva()
    {
        cinemaCameraController.ReproducirHaciaElFinal(0);

        estadoActual = EstadoJuego.Countdown;
        botonVolver.enabled = false;

        if (animatorMartillo != null) animatorMartillo.SetTrigger("TriggerPreparado");

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

                    if (animatorMartillo != null) animatorMartillo.SetTrigger("TriggerStumble");
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

        if (contenedorMouse != null)
            contenedorMouse.gameObject.SetActive(false);

        CancelInvoke(nameof(BorrarTextoCentral));

        int rangoMinimoVictoria = Mathf.Clamp(clicksMeta - tolerancia, 0, capacidadMaximaBarra);
        int rangoMaximoVictoria = Mathf.Clamp(clicksMeta + tolerancia, 0, capacidadMaximaBarra);

        if (clicksActuales > rangoMaximoVictoria)
        {
            accidenteOcurrido = true;

            textoCentral.text = "¡EXCESO DE FUERZA!";
            animatorMartillo.SetTrigger("TriggerAccidente");
            animatorBarra.SetTrigger("TriggerGolpeFuerte");
            animatorCarro.SetTrigger("TriggerRun");
        }
        else if (clicksActuales < rangoMinimoVictoria)
        {
            golpeDebilOcurrido = true;

            textoCentral.text = "¡Muy Débil!";
            animatorMartillo.SetTrigger("TriggerDebil");
        }
        else
        {
            victoriaObtenida = true;

            textoCentral.text = "¡GANASTE!";
            animatorMartillo.SetTrigger("TriggerVictoria");
            animatorBarra.SetTrigger("TriggerGolpeExacto");
            Invoke(nameof(ReiniciarJuego), tiempoParaReiniciar + 2f);
            return;
        }

        Invoke(nameof(ReiniciarJuego), tiempoParaReiniciar);
    }

    void ReiniciarJuego()
    {
        estadoActual = EstadoJuego.Idle;

        if (accidenteOcurrido && volumeModifier != null)
        {
            volumeModifier.ResetEfecto();
            animatorCarro.SetTrigger("TriggerReset");
            accidenteOcurrido = false;
            cinemaCameraController.ReproducirHaciaElInicio(3);
        }
        else if (victoriaObtenida)
        {
            cinemaCameraController.ReproducirHaciaElFinal(4);
        }
        else if (golpeDebilOcurrido)
        {
            cinemaCameraController.ReproducirHaciaElInicio(0);
        }

        textoCentral.text = "Click para iniciar";
        contadorMouse.text = "";

        if (objetoTituloTiempo != null)
            objetoTituloTiempo.SetActive(false);

        if (textoTiempo != null)
            textoTiempo.text = tiempoLimite.ToString("F1") + "s";

        sliderFuerza.value = 0;

        animatorMartillo.SetTrigger("TriggerReset");
        animatorBarra.SetTrigger("TriggerReset");

        botonVolver.enabled = true;

        if (botonDeInicio != null) botonDeInicio.enabled = true;
    }
}