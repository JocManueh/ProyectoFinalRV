using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class AmpollaRadiactiva : MonoBehaviour, IXRSelectFilter
{
    [SerializeField] float segundosEntreAvisos = 2f;
    public UnityEvent OnExtraidaConPinzas;

    XRGrabInteractable grab;
    float ultimoAviso = -99f;
    bool extraida;

    public bool canProcess => isActiveAndEnabled;

    void Awake() => grab = GetComponent<XRGrabInteractable>();
    void OnEnable()
    {
        grab.selectFilters.Add(this);
        grab.selectEntered.AddListener(AlAgarrar);
    }
    void OnDisable()
    {
        grab.selectFilters.Remove(this);
        grab.selectEntered.RemoveListener(AlAgarrar);
    }

    // Solo los sockets (punta de las pinzas, matraz) pueden agarrarla
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (interactor is XRSocketInteractor) return true;
        AvisarMano();
        return false;
    }

    void AvisarMano()
    {
        if (Time.unscaledTime - ultimoAviso < segundosEntreAvisos) return;
        ultimoAviso = Time.unscaledTime;

        Debug.Log("[E7] Ampolla radiactiva tocada con la mano");
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.RegisterError(ErrorCode.E7_Radiacion, "Tritio tocado sin pinzas");
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();
        if (NOVA.Instance != null) NOVA.Instance.Say("¡Usa las pinzas!");
    }

    void AlAgarrar(SelectEnterEventArgs args)
    {
        if (extraida) return;
        extraida = true;
        Debug.Log("[Tritio] Extraída con pinzas");
        OnExtraidaConPinzas?.Invoke();
    }
}