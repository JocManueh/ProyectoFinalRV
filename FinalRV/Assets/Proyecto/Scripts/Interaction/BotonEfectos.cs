using UnityEngine;

public class BotonEfectos : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] float segundosBloqueo = 0.3f; // evita doble pulsación
    float ultimaPulsacion = -99f;

    public void Efectos()
    {
        if (Time.unscaledTime - ultimaPulsacion < segundosBloqueo) return;
        ultimaPulsacion = Time.unscaledTime;

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(520f, 0.08f));
        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Suave();
    }
}