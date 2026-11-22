using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PalancaLab : MonoBehaviour
{
    [Header("Geometría")]
    [SerializeField] Transform pivote;
    [SerializeField] float anguloMin = -45f;
    [SerializeField] float anguloMax = 45f;

    [Header("Estado (solo lectura)")]
    [Range(0f, 100f)] public float valor; // 0-100 %
    public UnityEvent<float> OnValueChanged;

    XRSimpleInteractable interactable;
    float ultimoValor = -1f;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (pivote == null) pivote = transform.parent;
        Aplicar(anguloMin);
    }

    void Update()
    {
        if (!interactable.isSelected) return;

        var interactor = interactable.interactorsSelecting[0];
        Vector3 mano = interactor.GetAttachTransform(interactable).position;

        Transform padre = pivote.parent;
        Vector3 d = padre != null
            ? padre.InverseTransformPoint(mano) - pivote.localPosition
            : mano - pivote.position;

        float angulo = Mathf.Atan2(d.z, d.y) * Mathf.Rad2Deg;
        Aplicar(Mathf.Clamp(angulo, anguloMin, anguloMax));
    }

    void Aplicar(float angulo)
    {
        pivote.localRotation = Quaternion.Euler(angulo, 0f, 0f);
        valor = Mathf.InverseLerp(anguloMin, anguloMax, angulo) * 100f;

        if (!Mathf.Approximately(valor, ultimoValor))
        {
            ultimoValor = valor;
            OnValueChanged?.Invoke(valor);
        }
    }
}
