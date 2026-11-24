using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class Pinzas : MonoBehaviour
{
    [SerializeField] XRSocketInteractor puntaSocket;
    XRGrabInteractable grab;

    void Awake() => grab = GetComponent<XRGrabInteractable>();
    void OnEnable() => grab.activated.AddListener(AlActivar);
    void OnDisable() => grab.activated.RemoveListener(AlActivar);

    // Gatillo con las pinzas en la mano = soltar la ampolla
    void AlActivar(ActivateEventArgs args)
    {
        if (puntaSocket == null || !puntaSocket.hasSelection) return;
        var ampolla = puntaSocket.firstInteractableSelected;
        puntaSocket.interactionManager.SelectExit(puntaSocket, ampolla);
    }
}