using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Selecciona el anclaje de cada mano y centra la ampolla en su bandeja.</summary>
public class FlashAgarrePorMano : XRGrabInteractable
{
    [SerializeField] Transform izquierda;

    Transform centroBandeja;
    Transform bocaVertido;
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

        if (!TryGetComponent(out CapsuleCollider capsula) || capsula.direction != 1) return;

        bocaVertido = new GameObject("Attach_BocaVertido").transform;
        bocaVertido.SetParent(transform, false);
        float extremoSuperior = Mathf.Max(capsula.height * 0.5f, capsula.radius);
        bocaVertido.localPosition = capsula.center + Vector3.up * extremoSuperior;
        bocaVertido.localRotation = Quaternion.Euler(180f, 0f, 0f);
    }

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        if (interactor is XRSocketInteractor socket)
        {
            if (socket.TryGetComponent(out MatrazFlash _))
                return bocaVertido != null ? bocaVertido : transform;

            bool esBandeja = (socket.interactionLayers.value & capasBandejas) != 0 &&
                (socket.interactionLayers.value & ~capasBandejas) == 0;

            return esBandeja && centroBandeja != null ? centroBandeja : transform;
        }

        if (interactor is XRBaseInteractor mano &&
            mano.handedness == InteractorHandedness.Left && izquierda != null)
            return izquierda;

        return base.GetAttachTransform(interactor);
    }
}

