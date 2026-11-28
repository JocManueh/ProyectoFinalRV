using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

/// <summary>Detecta el gesto de beber y aplica temporalmente el poder de velocidad.</summary>
public class FlashBeberSuero : MonoBehaviour
{
    [SerializeField] XRGrabInteractable agarre;
    [SerializeField] Transform bocaMatraz;
    [SerializeField] SphereCollider cabeza;
    [SerializeField] ContinuousMoveProvider movimiento;
    [SerializeField] float segundosParaBeber = 3f;
    [SerializeField] float alcance = 0.20f;
    [SerializeField] float tolerancia = 0.02f;
    [SerializeField] float multiplicadorVelocidad = 1.5f;
    [SerializeField] float segundosDePoder = 4f;

    float tiempoBebiendo;
    float velocidadOriginal;
    float tiempoPoder;
    bool poderActivo;

    public string Progreso => $"Bebiendo {tiempoBebiendo:0.0}/{segundosParaBeber:0} s";

    public void HabilitarAgarre(bool habilitado)
    {
        tiempoBebiendo = 0f;
        agarre.enabled = habilitado;
    }

    public bool ActualizarGesto(float dt)
    {
        if (!agarre.isSelected || !ApuntaACabeza())
        {
            tiempoBebiendo = 0f;
            return false;
        }

        tiempoBebiendo += dt;
        return tiempoBebiendo >= segundosParaBeber;
    }

    bool ApuntaACabeza()
    {
        if (!cabeza.enabled || !cabeza.gameObject.activeInHierarchy) return false;

        Vector3 origen = bocaMatraz.position;
        Vector3 direccion = bocaMatraz.forward;
        Vector3 centro = cabeza.transform.TransformPoint(cabeza.center);
        Vector3 escala = cabeza.transform.lossyScale;
        float radio = cabeza.radius * Mathf.Max(Mathf.Abs(escala.x),
            Mathf.Max(Mathf.Abs(escala.y), Mathf.Abs(escala.z))) + tolerancia;

        // Detecta la esfera incluso si la boca del matraz ya entró en ella.
        float distancia = Mathf.Clamp(Vector3.Dot(centro - origen, direccion), 0f, alcance);
        Vector3 puntoCercano = origen + direccion * distancia;
        return (puntoCercano - centro).sqrMagnitude <= radio * radio;
    }

    public void IniciarPoder()
    {
        if (movimiento == null || poderActivo) return;

        velocidadOriginal = movimiento.moveSpeed;
        tiempoPoder = 0f;
        poderActivo = true;
    }

    void Update()
    {
        if (!poderActivo) return;

        tiempoPoder += Time.unscaledDeltaTime;
        if (tiempoPoder >= segundosDePoder)
        {
            RestaurarVelocidad();
            return;
        }

        float transicion = Mathf.Clamp01(tiempoPoder / 0.35f);
        movimiento.moveSpeed = Mathf.Lerp(velocidadOriginal,
            velocidadOriginal * multiplicadorVelocidad, transicion);
    }

    public void RestaurarVelocidad()
    {
        if (!poderActivo) return;
        poderActivo = false;
        if (movimiento != null) movimiento.moveSpeed = velocidadOriginal;
    }

    void OnDisable() => RestaurarVelocidad();
}

