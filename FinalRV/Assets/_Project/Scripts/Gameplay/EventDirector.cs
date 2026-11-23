/*
 * DESCRIPCIÓN: Dispara el evento EMP al 50 % del tiempo del capítulo.
 * Apaga las luces con fade, activa luz roja de emergencia, habilita la linterna
 * agarrable y un botón rojo del generador que reactiva la luz normal.
 * Penalización E8 si no se resuelve en 10 s.
 */
using System.Collections;
using UnityEngine;

public class EventDirector : MonoBehaviour
{
    [Header("Luces")]
    [SerializeField] Light[] lucesNormales;
    [SerializeField] Light luzEmergencia;   // luz roja
    [SerializeField] float fadeDuracion = 1.5f;

    [Header("Linterna")]
    [SerializeField] GameObject linternaGO;  // XRGrabInteractable con SpotLight
    [SerializeField] Light luzLinterna;

    [Header("Botón generador")]
    [SerializeField] BotonLab botonGenerador;

    [Header("Tiempo para resolver EMP")]
    [SerializeField] float segundosParaResolver = 10f;

    bool empDisparado;
    bool empResuelto;

    void OnEnable()
    {
        if (ChapterManager.Instance != null)
            ChapterManager.Instance.OnChapterStarted += AlIniciarCapitulo;
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnLinternaToggle += ToggleLinterna;
        if (botonGenerador != null)
            botonGenerador.OnPressed.AddListener(ResolverEMP);
    }

    void OnDisable()
    {
        if (ChapterManager.Instance != null)
            ChapterManager.Instance.OnChapterStarted -= AlIniciarCapitulo;
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnLinternaToggle -= ToggleLinterna;
        if (botonGenerador != null)
            botonGenerador.OnPressed.RemoveListener(ResolverEMP);
    }

    void AlIniciarCapitulo(ChapterSO cap)
    {
        StartCoroutine(EsperarMitad(cap.tiempoSegundos * 0.5f));
    }

    IEnumerator EsperarMitad(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        if (!empDisparado) DispararEMP();
    }

    void DispararEMP()
    {
        empDisparado = true;
        Debug.Log("[EventDirector] ⚡ EMP disparado al 50% del tiempo.");
        if (NOVA.Instance != null)
            NOVA.Instance.Say("¡Pulso EMP detectado! Fallo de energía. Activa el generador de emergencia.");
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();

        StartCoroutine(FadeApagar());
        StartCoroutine(TemporizadorEMP());
    }

    IEnumerator FadeApagar()
    {
        float t = 0f;
        float[] intensidadesOriginales = new float[lucesNormales.Length];
        for (int i = 0; i < lucesNormales.Length; i++)
            if (lucesNormales[i] != null) intensidadesOriginales[i] = lucesNormales[i].intensity;

        while (t < fadeDuracion)
        {
            t += Time.unscaledDeltaTime;
            float ratio = 1f - t / fadeDuracion;
            for (int i = 0; i < lucesNormales.Length; i++)
                if (lucesNormales[i] != null)
                    lucesNormales[i].intensity = intensidadesOriginales[i] * ratio;
            yield return null;
        }

        foreach (var l in lucesNormales)
            if (l != null) l.enabled = false;

        // Activar luz roja y linterna
        if (luzEmergencia != null) luzEmergencia.enabled = true;
        if (linternaGO    != null) linternaGO.SetActive(true);
    }

    IEnumerator TemporizadorEMP()
    {
        yield return new WaitForSecondsRealtime(segundosParaResolver);
        if (!empResuelto)
        {
            Debug.Log("[E8] EMP no resuelto en tiempo.");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E8_Tiempo, "EMP no resuelto en 10s");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("Tiempo agotado. Penalización aplicada. El generador se reinicia solo.");
            ResolverEMP(); // auto-resolver para no bloquear el juego
        }
    }

    public void ResolverEMP()
    {
        if (empResuelto) return;
        empResuelto = true;
        Debug.Log("[EventDirector] ✅ EMP resuelto.");

        foreach (var l in lucesNormales)
            if (l != null) { l.enabled = true; l.intensity = 1f; }

        if (luzEmergencia != null) luzEmergencia.enabled = false;
        if (linternaGO    != null) linternaGO.SetActive(false);

        if (NOVA.Instance != null)
            NOVA.Instance.Say("Energía restaurada. Continúa con el experimento.");
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
    }

    void ToggleLinterna()
    {
        if (luzLinterna == null) return;
        luzLinterna.enabled = !luzLinterna.enabled;
        Debug.Log($"[EventDirector] Linterna: {(luzLinterna.enabled ? "ON" : "OFF")}");
    }
}
