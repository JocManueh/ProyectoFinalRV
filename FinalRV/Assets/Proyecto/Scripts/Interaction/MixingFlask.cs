/*
 * DESCRIPCIÓN: Controla la zona de encaje (socket) del matraz de mezclas. Verifica que 
 * las ampollas insertadas contengan líquido; si una ampolla está vacía, la rechaza y 
 * dispara una alerta visual (luz roja), sonora, táctil y resta puntos por error E1.
 */
using System.Collections;
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
    }

    void AlSacar(SelectExitEventArgs args) => Pintar(colorBase);

    void Pintar(Color c)
    {
        if (piloto != null) piloto.material.color = c;
    }
}