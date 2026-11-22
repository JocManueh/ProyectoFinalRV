using System;
using UnityEngine;

public class ZonaMarca : MonoBehaviour
{
    [SerializeField] float radio = 0.6f;
    [SerializeField] float giroMinimo = 45f;

    public event Action Completada;
    public bool EnZona { get; private set; }

    Transform cam;
    float yawInicial;
    float maxGiro;
    bool listo;

    void Update()
    {
        if (listo) return;

        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
            yawInicial = cam.eulerAngles.y;
        }

        float giro = Mathf.Abs(Mathf.DeltaAngle(yawInicial, cam.eulerAngles.y));
        if (giro > maxGiro) maxGiro = giro;

        Vector3 d = cam.position - transform.position;
        d.y = 0f;
        EnZona = d.magnitude <= radio;

        if (EnZona && maxGiro >= giroMinimo)
        {
            listo = true;
            Debug.Log("[Onboarding] Estación 1 completada (llegó y giró)");
            Completada?.Invoke();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}