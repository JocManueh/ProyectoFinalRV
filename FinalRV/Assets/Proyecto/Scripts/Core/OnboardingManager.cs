using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Guía la práctica inicial, controla su tiempo y abre el laboratorio al completarla.</summary>
public class OnboardingManager : MonoBehaviour
{
    public const string PrefKey = "onboarding_completado";

    [Header("Estaciones")]
    [SerializeField] ZonaMarca zonaMarca;
    [SerializeField] XRGrabInteractable probeta;
    [SerializeField] XRSocketInteractor socketMatraz;
    [SerializeField] BotonLab botonLab;
    [SerializeField] GameObject luzBoton;
    [SerializeField] PalancaLab palanca;

    [Header("Palanca (porcentaje 0-100)")]
    [SerializeField] float zonaMin = 80f;
    [SerializeField] float zonaMax = 100f;
    [SerializeField] float segundosEnZona = 2f;

    [Header("UI")]
    [SerializeField] PanelUI panelChecklist;
    [SerializeField] InputHintUI hint;

    [Header("Flujo")]
    [SerializeField] string escenaSiguiente = "01_Laboratorio";
    [SerializeField] int puntosPorTarea = 20;
    [SerializeField] float segundosAntesDeCargar = 4f;
    [SerializeField, Min(1f)] float tiempoLimite = 300f;
    [Tooltip("Pruebas: true = siempre hace el onboarding aunque ya lo hayas completado.")]
    [SerializeField] bool ignorarBandera = true;
    [Tooltip("Desarrollo: F1 completa la tarea pendiente. Apágalo en la entrega.")]
    [SerializeField] bool permitirSaltar = true;

    static readonly string[] nombres =
    {
        "Moverte a la marca verde y girar",
        "Agarrar la probeta",
        "Colocarla en el socket",
        "Presionar el botón",
        "Palanca en la zona verde (2 s)"
    };

    static readonly string[] frasesNova =
    {
        "Bien. Para girar usa el giro por pasos. Ahora toma la probeta de la mesa.",
        "Muy bien. Ahora colócala en el socket del matraz.",
        "Perfecto. Ahora presiona el botón del panel.",
        "Excelente. Última prueba: lleva la palanca a la zona verde y mantenla.",
        ""
    };

    readonly bool[] hechas = new bool[5];
    float tiempoZona;
    TimerManager reloj;
    int ultimoSegundo = -1;
    bool intentado;
    bool terminado;

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Onboarding);
        if (ScoreSystem.Instance != null) ScoreSystem.Instance.ResetAll();
        if (luzBoton != null) luzBoton.SetActive(false);
        reloj = TimerManager.Instance;
        if (reloj != null)
        {
            reloj.IniciarGlobal();
            reloj.DetenerCapitulo();
        }

        if (!ignorarBandera && PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            terminado = true;
            Decir("Ya completaste la simulación. Entrando al laboratorio.");
            StartCoroutine(CargarSiguiente());
            return;
        }

        intentado = true;
        reloj?.IniciarCapitulo(tiempoLimite);
        Conectar();
        Refrescar();
        if (hint != null) hint.Mostrar(0);
        Decir("Bienvenido, doctor. Antes de tocar reactivos, practiquemos en el simulador. Camina hasta la marca verde y gira.");
    }

    void Conectar()
    {
        if (zonaMarca != null) zonaMarca.Completada += AlMover;
        if (probeta != null) probeta.selectEntered.AddListener(AlAgarrar);
        if (socketMatraz != null) socketMatraz.selectEntered.AddListener(AlSocket);
        if (botonLab != null) botonLab.OnPressed.AddListener(AlBoton);
        if (InputRouter.Instance != null) InputRouter.Instance.OnSaltarCap += SaltarPendiente;
    }

    void OnDestroy()
    {
        if (zonaMarca != null) zonaMarca.Completada -= AlMover;
        if (probeta != null) probeta.selectEntered.RemoveListener(AlAgarrar);
        if (socketMatraz != null) socketMatraz.selectEntered.RemoveListener(AlSocket);
        if (botonLab != null) botonLab.OnPressed.RemoveListener(AlBoton);
        if (InputRouter.Instance != null) InputRouter.Instance.OnSaltarCap -= SaltarPendiente;
    }

    void Update()
    {
        if (terminado || !intentado) return;
        if (reloj != null)
        {
            if (reloj.CapituloRestante <= 0f)
            {
                AgotarTiempo();
                return;
            }
            if (Mathf.CeilToInt(reloj.CapituloRestante) != ultimoSegundo) Refrescar();
        }
        if (palanca == null || hechas[4]) return;

        bool enZona = palanca.valor >= zonaMin && palanca.valor <= zonaMax;
        if (enZona)
        {
            tiempoZona += Time.unscaledDeltaTime;
            if (tiempoZona >= segundosEnZona) Completar(4);
        }
        else tiempoZona = 0f;
    }

    void AlMover() => Completar(0);
    void AlAgarrar(SelectEnterEventArgs args) => Completar(1);
    void AlBoton() => Completar(3);

    void AlSocket(SelectEnterEventArgs args)
    {
        if (probeta == null || args.interactableObject.transform == probeta.transform)
            Completar(2);
    }

    void SaltarPendiente()
    {
        if (!permitirSaltar || terminado) return;
        int i = Siguiente();
        if (i >= 0) Completar(i);
    }

    int Siguiente()
    {
        for (int i = 0; i < hechas.Length; i++) if (!hechas[i]) return i;
        return -1;
    }

    public void Completar(int i)
    {
        if (terminado || i < 0 || i >= hechas.Length || hechas[i]) return;
        if (reloj != null && intentado && reloj.CapituloRestante <= 0f)
        {
            AgotarTiempo();
            return;
        }
        hechas[i] = true;

        if (i == 3 && luzBoton != null) luzBoton.SetActive(true);
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.Add(puntosPorTarea, "Onboarding: " + nombres[i]);

        Debug.Log($"[Onboarding] Tarea {i + 1}/5 completada: {nombres[i]}");
        Refrescar();

        int sig = Siguiente();
        if (sig < 0)
        {
            terminado = true;
            if (hint != null) hint.Ocultar();
            reloj?.DetenerCapitulo();
            Decir("Simulación completa.");
            StartCoroutine(CargarSiguiente());
        }
        else
        {
            if (!string.IsNullOrEmpty(frasesNova[i])) Decir(frasesNova[i]);
            if (hint != null) hint.Mostrar(sig);
        }
    }

    void Refrescar()
    {
        ultimoSegundo = Mathf.CeilToInt(reloj != null ? reloj.CapituloRestante : tiempoLimite);
        var sb = new StringBuilder();
        sb.AppendLine($"Tiempo de práctica  {TimerManager.Formato(ultimoSegundo)}");
        sb.AppendLine();
        for (int i = 0; i < nombres.Length; i++)
            sb.AppendLine((hechas[i] ? "[X] " : "[  ] ") + nombres[i]);

        if (panelChecklist != null) panelChecklist.SetTexto("Simulación NOVA", sb.ToString());

    }

    void AgotarTiempo()
    {
        if (terminado) return;
        terminado = true;
        reloj?.DetenerTodo();
        if (hint != null) hint.Ocultar();
        panelChecklist?.SetTexto("Simulación NOVA", "Tiempo agotado. Reiniciando la práctica.");
        NOVA.Instance?.Limpiar();
        Decir("Se acabó el tiempo. Volvamos a intentar la práctica.");
        StartCoroutine(ReiniciarPractica());
    }

    IEnumerator ReiniciarPractica()
    {
        yield return new WaitForSecondsRealtime(segundosAntesDeCargar);
        SceneManager.LoadScene(gameObject.scene.name);
    }

    IEnumerator CargarSiguiente()
    {
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();

        yield return new WaitForSecondsRealtime(segundosAntesDeCargar);

        if (Application.CanStreamedLevelBeLoaded(escenaSiguiente))
            SceneManager.LoadScene(escenaSiguiente);
        else
            Debug.LogWarning($"[Onboarding] La escena '{escenaSiguiente}' no está en Build Profiles todavía.");
    }

    static void Decir(string t)
    {
        if (NOVA.Instance != null) NOVA.Instance.Say(t);
    }

    [ContextMenu("Borrar bandera de onboarding")]
    void BorrarBandera()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        Debug.Log("[Onboarding] Bandera borrada");
    }
}