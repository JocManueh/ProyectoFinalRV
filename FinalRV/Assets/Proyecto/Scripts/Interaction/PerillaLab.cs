using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PerillaLab : MonoBehaviour
{
    [SerializeField] Transform pieza;
    [SerializeField] float anguloMin = -135f;
    [SerializeField] float anguloMax = 135f;

    [Range(0f, 100f)] public float valor;
    public UnityEvent<float> OnValueChanged;

    XRSimpleInteractable interactable;
    float angulo;
    float anguloManoPrevio;
    bool siguiendo;
    float ultimoValor = -1f;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (pieza == null) pieza = transform;
        angulo = anguloMin;
        Aplicar();
    }

    void Update()
    {
        if (!interactable.isSelected)
        {
            siguiendo = false;
            return;
        }

        var interactor = interactable.interactorsSelecting[0];
        Vector3 mano = interactor.GetAttachTransform(interactable).position;

        Transform padre = pieza.parent;
        Vector3 d = padre != null
            ? padre.InverseTransformPoint(mano) - pieza.localPosition
            : mano - pieza.position;

        float anguloMano = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;

        if (!siguiendo)
        {
            siguiendo = true;
            anguloManoPrevio = anguloMano;
            return;
        }

        float delta = Mathf.DeltaAngle(anguloManoPrevio, anguloMano);
        anguloManoPrevio = anguloMano;

        angulo = Mathf.Clamp(angulo + delta, anguloMin, anguloMax);
        Aplicar();
    }

    void Aplicar()
    {
        pieza.localRotation = Quaternion.Euler(0f, angulo, 0f);
        valor = Mathf.InverseLerp(anguloMin, anguloMax, angulo) * 100f;

        if (!Mathf.Approximately(valor, ultimoValor))
        {
            ultimoValor = valor;
            OnValueChanged?.Invoke(valor);
        }
    }
}
