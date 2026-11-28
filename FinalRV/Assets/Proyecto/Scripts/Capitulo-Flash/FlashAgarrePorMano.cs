using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Devuelve el anclaje de la mano antes de que XR capture la pose de agarre.</summary>
public class FlashAgarrePorMano : XRGrabInteractable
{
    [SerializeField] Transform izquierda;

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        if (interactor is XRSocketInteractor) return transform;
        if (interactor is XRBaseInteractor mano &&
            mano.handedness == InteractorHandedness.Left && izquierda != null)
            return izquierda;

        return base.GetAttachTransform(interactor);
    }
}

