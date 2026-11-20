/*
 * DESCRIPCIÓN: Controla la vibración física de ambos mandos de Meta Quest. Detecta 
 * los motores hápticos del rig y envía pulsos suaves al interactuar con objetos 
 * o vibraciones intensas cuando el jugador comete un error.
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticFeedback : MonoBehaviour
{
    public static HapticFeedback Instance { get; private set; }
    HapticImpulsePlayer[] jugadores;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        // Busca los componentes que vibran cada mando (vienen en el XR Origin)
        jugadores = FindObjectsByType<HapticImpulsePlayer>(FindObjectsSortMode.None);
    }

    public void Pulse(float amplitud = 0.5f, float segundos = 0.15f)
    {
        if (jugadores == null) return;
        foreach (var j in jugadores)
            if (j != null) j.SendHapticImpulse(amplitud, segundos);
    }

    public void Suave() => Pulse(0.3f, 0.08f);
    public void Error() => Pulse(0.9f, 0.3f);
}