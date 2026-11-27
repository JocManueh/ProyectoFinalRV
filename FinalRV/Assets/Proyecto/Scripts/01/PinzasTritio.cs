/*
 * DESCRIPCIÓN: Pinzas que recogen y sueltan tritio con el gatillo/clic (Activate).
 * El primer clic recoge el tritio más cercano a la punta; el segundo lo suelta.
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PinzasTritio : MonoBehaviour
{
    [SerializeField] Transform punta;
    [SerializeField] float radio = 0.06f;

    XRGrabInteractable grab;
    TritioItem sostenido;

    void Awake() => grab = GetComponent<XRGrabInteractable>();

    void OnEnable()
    {
        grab.activated.AddListener(AlActivar);
    }

    void OnDisable()
    {
        grab.activated.RemoveListener(AlActivar);
        if (sostenido != null) sostenido.Soltar();
    }

    void AlActivar(ActivateEventArgs args)
    {
        if (sostenido != null)
        {
            sostenido.Soltar();
            sostenido = null;
            return;
        }

        if (punta == null) return;

        foreach (var col in Physics.OverlapSphere(punta.position, radio))
        {
            var item = col.GetComponentInParent<TritioItem>();
            if (item == null || item.EnPinzas) continue;

            item.TomarCon(punta);
            sostenido = item;
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
            Debug.Log($"[PinzasTritio] Tritio tomado: {item.name}");
            return;
        }
    }
}
