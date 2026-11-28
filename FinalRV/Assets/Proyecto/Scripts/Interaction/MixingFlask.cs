/*
 * DESCRIPCIÓN: Controla la zona de encaje (socket) del matraz de mezclas. Rechaza ampollas
 * vacías (E1) y, si se asigna una RecipeSO, valida sustancia, orden y cantidad al insertar
 * (E3/E4/E5). Expone Completa() y el historial para RecipeValidator, que queda como
 * comprobación final sin volver a registrar esos errores.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class MixingFlask : MonoBehaviour, IXRSelectFilter
{
    [Header("Feedback")]
    [SerializeField] Renderer piloto; // la "luz" (una esfera pequeña)
    [SerializeField] AudioSource audioSource; // 3D (Spatial Blend = 1)
    [SerializeField] float segundosEntreAvisos = 2f;

    [Header("Validación de receta (opcional)")]
    [SerializeField] RecipeSO receta;
    [SerializeField] Transform liquido; // cilindro con pivote en la base
    [SerializeField] Renderer liquidoRenderer;
    [SerializeField] float alturaMax = 0.12f;

    // Campos históricos: se mantienen para no romper CrearEscenaCap1 / Test_Cap1.
    [SerializeField] Renderer liquidoVisual;
    [SerializeField] float alturaMaxLiquido = 0.08f;

    // Estado de la receta
    readonly List<RecipeStep> mezcla = new List<RecipeStep>();
    int paso;
    int enPaso;
    int totalInsertadas;
    int totalNecesarias;
    ChemicalSO ultimaSustancia;

    /// <summary>True solo cuando hay receta no vacía y se insertaron todas sus ampollas.</summary>
    public bool Completa => receta != null && receta.steps.Count > 0 && paso >= receta.steps.Count;

    XRSocketInteractor socket;
    Color colorBase;
    float ultimoAviso = -99f;

    // Obligatorio en un filtro: dice si está activo
    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (piloto != null) colorBase = piloto.material.color;

        totalNecesarias = 0;
        if (receta != null)
            foreach (var step in receta.steps)
                if (step != null) totalNecesarias += Mathf.Max(0, step.ampollas);

        ActualizarNivel();
    }

    void OnEnable()
    {
        socket.selectFilters.Add(this);
        socket.selectEntered.AddListener(AlInsertar);
        socket.selectExited.AddListener(AlSacar);
    }

    void OnDisable()
    {
        socket.selectFilters.Remove(this);
        socket.selectEntered.RemoveListener(AlInsertar);
        socket.selectExited.RemoveListener(AlSacar);
    }

    // Unity pregunta aquí: "¿puede este socket agarrar esta ampolla?"
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        var ampolla = interactable.transform.GetComponent<ChemicalContainer>();
        if (ampolla == null) return true; // no es ampolla: otras reglas deciden

        if (ampolla.EstaVacia)
        {
            Rechazar(ErrorCode.E1_Vacio, "Esa ampolla está vacía. Busca una que tenga contenido.");
            return false;
        }

        // Sin receta (o receta vacía) el matraz no valida nada y acepta todo.
        if (receta == null || receta.steps.Count == 0) return true;

        if (Completa)
        {
            Rechazar(ErrorCode.E4_Cantidad, "El matraz ya está completo. No añadas más ampollas.");
            return false;
        }

        var sustancia = ampolla.sustancia;
        var esperada = receta.steps[paso].chemical;
        if (esperada != null && sustancia == esperada) return true;

        if (!PerteneceAReceta(sustancia))
            Rechazar(ErrorCode.E3_Incorrecto, $"{Nombre(sustancia)} no está en la receta.");
        else if (ultimaSustancia != null && sustancia == ultimaSustancia)
            Rechazar(ErrorCode.E4_Cantidad, $"Ya añadiste la cantidad de {Nombre(sustancia)} que pedía la receta.");
        else
            Rechazar(ErrorCode.E5_Orden, $"Orden incorrecto. Ahora toca {Nombre(esperada)}.");

        return false;
    }

    bool PerteneceAReceta(ChemicalSO sustancia)
    {
        foreach (var step in receta.steps)
            if (step != null && step.chemical == sustancia) return true;
        return false;
    }

    static string Nombre(ChemicalSO c)
    {
        if (c == null) return "sustancia desconocida";
        return string.IsNullOrEmpty(c.displayName) ? c.name : c.displayName;
    }

    // Rechazo genérico: conserva el cooldown para no repetir la penalización.
    void Rechazar(ErrorCode codigo, string mensaje)
    {
        if (Time.unscaledTime - ultimoAviso < segundosEntreAvisos) return;
        ultimoAviso = Time.unscaledTime;

        Debug.Log($"[{codigo}] {mensaje}");
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.RegisterError(codigo, mensaje);
        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));
        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Error();
        if (NOVA.Instance != null)
            NOVA.Instance.Say(mensaje);

        StartCoroutine(ParpadeoRojo());
    }

    IEnumerator ParpadeoRojo()
    {
        Pintar(Color.red);
        yield return new WaitForSecondsRealtime(0.6f);
        Pintar(socket.hasSelection ? Color.green : colorBase);
    }

    void AlInsertar(SelectEnterEventArgs args)
    {
        Pintar(Color.green);
        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(880f, 0.12f));
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();

        var amp = args.interactableObject.transform.GetComponent<ChemicalContainer>();
        if (amp == null || amp.sustancia == null) return;
        var cont = amp;

        // Verificar si es un reactivo sin procesar (E2)
        if (cont.sustancia.requiresProcessing)
        {
            Debug.Log($"[E2] {Nombre(cont.sustancia)} insertado sin procesar.");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E2_SinProcesar, Nombre(cont.sustancia));
            if (NOVA.Instance != null)
                NOVA.Instance.Say($"{Nombre(cont.sustancia)} requiere procesamiento previo.");
            return;
        }

        // Historial en orden para RecipeValidator (red de seguridad al analizar).
        bool encontrado = false;
        foreach (var pasoMezcla in mezcla)
        {
            if (pasoMezcla.chemical == cont.sustancia)
            {
                pasoMezcla.ampollas++;
                encontrado = true;
                break;
            }
        }
        if (!encontrado)
            mezcla.Add(new RecipeStep { chemical = cont.sustancia, ampollas = 1 });

        // Avance de la receta (Process ya rechazó lo que no toca).
        if (receta != null && receta.steps.Count > 0 && !Completa)
        {
            var esperada = receta.steps[paso].chemical;
            if (esperada == null || cont.sustancia == esperada)
            {
                ultimaSustancia = cont.sustancia;
                totalInsertadas++;
                enPaso++;

                if (enPaso >= Mathf.Max(1, receta.steps[paso].ampollas))
                {
                    paso++;
                    enPaso = 0;
                }
                Debug.Log($"[MixingFlask] Insertado {Nombre(cont.sustancia)}: {totalInsertadas}/{totalNecesarias}");
            }
        }

        if (liquidoRenderer != null)
        {
            var mat = Application.isPlaying ? liquidoRenderer.material : liquidoRenderer.sharedMaterial;
            if (mat != null) mat.color = cont.sustancia.color;
        }
        ActualizarNivel();

        amp.Vaciar();
        Destroy(amp.gameObject, 0.5f);
    }

    void ActualizarNivel()
    {
        if (liquido == null) return;

        float fraccion = totalNecesarias > 0
            ? Mathf.Clamp01((float)totalInsertadas / totalNecesarias)
            : 0f;

        var escala = liquido.localScale;
        escala.y = Mathf.Max(0.001f, fraccion * alturaMax);
        liquido.localScale = escala;
    }

    void AlSacar(SelectExitEventArgs args) => Pintar(colorBase);

    void Pintar(Color c)
    {
        if (piloto != null) piloto.material.color = c;
    }

    /// <summary>Devuelve el historial de inserciones para RecipeValidator.</summary>
    public List<RecipeStep> ObtenerMezcla() => mezcla;

    /// <summary>Limpia la mezcla (para reiniciar el capítulo).</summary>
    public void LimpiarMezcla()
    {
        mezcla.Clear();
        paso = 0;
        enPaso = 0;
        totalInsertadas = 0;
        ultimaSustancia = null;
        ActualizarNivel();
    }
}
