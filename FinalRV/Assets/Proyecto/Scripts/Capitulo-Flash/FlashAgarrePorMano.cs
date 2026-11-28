using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Selecciona el anclaje de cada mano y centra la ampolla en su bandeja.</summary>
public class FlashAgarrePorMano : XRGrabInteractable
{
    [SerializeField] Transform izquierda;

    Transform centroBandeja;
    int capasBandejas;

    protected override void Awake()
    {
        base.Awake();
        capasBandejas = InteractionLayerMask.GetMask("Compuesto", "Carga", "Elemento");
        if (attachTransform == null || izquierda == null) return;

        centroBandeja = new GameObject("Attach_CentroBandeja").transform;
        centroBandeja.SetParent(transform, false);
        Vector3 puntoMedio = (attachTransform.position + izquierda.position) * 0.5f;
        centroBandeja.localPosition = transform.InverseTransformPoint(puntoMedio);
    }

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        if (interactor is XRSocketInteractor socket)
        {
            bool esBandeja = (socket.interactionLayers.value & capasBandejas) != 0 &&
                socket.GetComponent<MatrazFlash>() == null &&
                (socket.interactionLayers.value & ~capasBandejas) == 0;

            return esBandeja && centroBandeja != null ? centroBandeja : transform;
        }

        if (interactor is XRBaseInteractor mano &&
            mano.handedness == InteractorHandedness.Left && izquierda != null)
            return izquierda;

        return base.GetAttachTransform(interactor);
    }
}

