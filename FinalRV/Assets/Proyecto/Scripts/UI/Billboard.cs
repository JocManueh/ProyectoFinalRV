/*
 * DESCRIPCIÓN: Orienta automáticamente cualquier letrero o etiqueta flotante en 3D 
 * para que siempre mire de frente hacia la cámara del jugador, facilitando su lectura 
 * desde cualquier ángulo en realidad virtual.
 */
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
    }
}