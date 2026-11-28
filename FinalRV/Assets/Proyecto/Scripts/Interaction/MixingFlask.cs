/*
 * DESCRIPCIÓN: Controla la zona de encaje (socket) del matraz de mezclas. Verifica que 
 * las ampollas insertadas contengan líquido; si una ampolla está vacía, la rechaza y 
 * dispara una alerta visual (luz roja), sonora, táctil y resta puntos por error E1.
 * AMPLIADO: registra el orden de inserción y expone ObtenerMezcla() para RecipeValidator.
 *           Valida orden (E5) y cantidad (E4) si se asigna una RecipeSO.
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
    [SerializeField] Renderer liquidoVisual; // cilindro que sube y cambia de color
    [SerializeField] float alturaMaxLiquido = 0.08f; // escala Y máxima del cilindro

    // Historial de inserciones en orden
    readonly List<RecipeStep> mezcla = new List<RecipeStep>();

    XRSocketInteractor socket;
    Color colorBase;
    float ultimoAviso = -99f;

    // Obligatorio en un filtro: dice si está activo
    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (piloto != null) colorBase = piloto.material.color;
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
        if (!ampolla.EstaVacia) return true; // llena: se acepta

        AvisarVacia();
        return false; // vacía: RECHAZADA
    }

    void AvisarVacia()
    {
        // Process se llama muchas veces por segundo: limitamos los avisos
        if (Time.unscaledTime - ultimoAviso < segundosEntreAvisos) return;
        ultimoAviso = Time.unscaledTime;

        Debug.Log("[E1] Ampolla vacía rechazada por el matraz");
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.RegisterError(ErrorCode.E1_Vacio, "Ampolla vacía en el matraz");

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));

        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Error();

        if (NOVA.Instance != null)
            NOVA.Instance.Say("Esa ampolla está vacía. Busca una que tenga contenido.");

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

        // ── Registro de mezcla y validación de orden/cantidad ────────────────
        var cont = args.interactableObject.transform.GetComponent<ChemicalContainer>();
        if (cont == null || cont.sustancia == null) return;

        // Verificar si es un reactivo sin procesar (E2)
        if (cont.sustancia.requiresProcessing)
        {
            Debug.Log($"[E2] {cont.sustancia.displayName} insertado sin procesar.");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E2_SinProcesar,
                    cont.sustancia.displayName);
            if (NOVA.Instance != null)
                NOVA.Instance.Say($"{cont.sustancia.displayName} requiere procesamiento previo.");
            return;
        }

        // Buscar si ya existe en la mezcla para acumular cantidad
        bool encontrado = false;
        foreach (var paso in mezcla)
        {
            if (paso.chemical == cont.sustancia)
            {
                paso.ampollas++;
                encontrado = true;
                break;
            }
        }
        if (!encontrado)
            mezcla.Add(new RecipeStep { chemical = cont.sustancia, ampollas = 1 });

        // Validar orden si hay receta
        if (receta != null)
            ValidarOrden(cont.sustancia);

        ActualizarLiquido();
        Debug.Log($"[MixingFlask] Insertado: {cont.sustancia.displayName} (total mezcla: {mezcla.Count} tipos)");
    }

    void ValidarOrden(ChemicalSO sustancia)
    {
        // El índice esperado es el número de tipos distintos ya en la mezcla - 1
        int idxActual = mezcla.Count - 1;
        if (idxActual >= receta.steps.Count) return;

        var pasoEsperado = receta.steps[idxActual];
        if (pasoEsperado.chemical != sustancia)
        {
            Debug.Log($"[E5] Orden incorrecto: se esperaba {pasoEsperado.chemical?.displayName}, llegó {sustancia.displayName}");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E5_Orden,
                    $"Esperado {pasoEsperado.chemical?.displayName}, recibido {sustancia.displayName}");
            if (NOVA.Instance != null)
                NOVA.Instance.Say($"Orden incorrecto. Primero añade {pasoEsperado.chemical?.displayName}.");
        }
    }

    void ActualizarLiquido()
    {
        if (liquidoVisual == null || receta == null) return;

        int totalAmpollas = 0;
        foreach (var p in mezcla) totalAmpollas += p.ampollas;

        int totalEsperado = 0;
        foreach (var s in receta.steps) totalEsperado += s.ampollas;
        if (totalEsperado == 0) return;

        float t = Mathf.Clamp01((float)totalAmpollas / totalEsperado);

        // Subir nivel del líquido (escala Y)
        var escala = liquidoVisual.transform.localScale;
        escala.y = Mathf.Lerp(0.01f, alturaMaxLiquido, t);
        liquidoVisual.transform.localScale = escala;

        // Color: mezcla de colores de los reactivos insertados
        Color colorMezcla = Color.black;
        int n = 0;
        foreach (var p in mezcla)
        {
            if (p.chemical != null) { colorMezcla += p.chemical.color; n++; }
        }
        if (n > 0) liquidoVisual.material.color = colorMezcla / n;
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
        ActualizarLiquido();
    }
}