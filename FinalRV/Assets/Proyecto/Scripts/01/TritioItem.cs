/*
 * DESCRIPCIÓN: Un átomo de tritio de la caja. Solo puede manipularse con las pinzas:
 * si una mano intenta agarrarlo, se cancela el agarre y se penaliza con E7.
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class TritioItem : MonoBehaviour
{
    [SerializeField] float segundosEntreAvisos = 2f;

    /// <summary>True mientras lo sostiene la punta de las pinzas.</summary>
    public bool EnPinzas { get; private set; }

    XRGrabInteractable grab;
    Rigidbody rb;
    float ultimoAviso = -99f;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        if (GetComponent<Collider>() == null) gameObject.AddComponent<CapsuleCollider>();
    }

    void OnEnable() => grab.selectEntered.AddListener(AlAgarrar);
    void OnDisable() => grab.selectEntered.RemoveListener(AlAgarrar);

    void AlAgarrar(SelectEnterEventArgs args)
    {
        if (EnPinzas) return;

        // El evento real de XRI 3.6.1 se llama manager (no interactionManager).
        if (args.manager != null) args.manager.SelectCancel(args.interactorObject, grab);

        // Cancelar siempre; avisar y penalizar como máximo cada 2 s.
        if (Time.unscaledTime - ultimoAviso < segundosEntreAvisos) return;
        ultimoAviso = Time.unscaledTime;

        Debug.Log("[E7] Tritio tocado sin pinzas");
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.RegisterError(ErrorCode.E7_Radiacion, "Tritio tocado sin pinzas");
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();
        if (NOVA.Instance != null)
            NOVA.Instance.Say("¡No toques el tritio con la mano! Usa las pinzas.");
    }

    public void TomarCon(Transform punta)
    {
        EnPinzas = true;
        rb.isKinematic = true;
        transform.SetParent(punta, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Soltar()
    {
        EnPinzas = false;
        transform.SetParent(null, true);
        rb.isKinematic = false;
    }
}
