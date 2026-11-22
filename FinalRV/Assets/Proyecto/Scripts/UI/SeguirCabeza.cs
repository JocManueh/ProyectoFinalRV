using UnityEngine;

public class SeguirCabeza : MonoBehaviour
{
    [SerializeField] float distancia = 1.2f;
    [SerializeField] float alturaRelativa = -0.3f;
    [SerializeField] float suavizado = 4f;

    Transform cam;

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }

        Vector3 adelante = cam.forward;
        adelante.y = 0f;
        if (adelante.sqrMagnitude < 0.001f) adelante = cam.forward;
        adelante.Normalize();

        Vector3 objetivo = cam.position + adelante * distancia + Vector3.up * alturaRelativa;

        float k = Time.unscaledDeltaTime * suavizado;
        transform.position = Vector3.Lerp(transform.position, objetivo, k);

        Quaternion mirar = Quaternion.LookRotation(transform.position - cam.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, mirar, k);
    }
}
