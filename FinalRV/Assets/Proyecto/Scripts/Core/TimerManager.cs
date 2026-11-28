/*
 * DESCRIPCIÓN: Controla los dos cronómetros del juego: el tiempo límite global de 15 minutos 
 * y el reloj de cada capítulo. Mide el tiempo real sin verse afectado cuando el juego 
 * entra en cámara lenta y avisa cuando el tiempo llega a cero.
 */
using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Tiempo global (la Cosecha)")]
    public float segundosGlobal = 900f; // 15:00
    public float GlobalRestante { get; private set; }
    public float CapituloRestante { get; private set; }
    public bool GlobalCorriendo { get; private set; }
    public bool GlobalIniciado { get; private set; }
    public bool CapituloCorriendo { get; private set; }
    public bool Pausado { get; set; } // se usará al quitarse las gafas

    public event Action<float, float> OnTick; // (global, capítulo)
    public event Action OnGlobalTimeUp;
    public event Action OnChapterTimeUp;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Pausado) return;
        // unscaledDeltaTime: NO depende de Time.timeScale (cámara lenta)
        float dt = Time.unscaledDeltaTime;

        if (GlobalCorriendo)
        {
            GlobalRestante -= dt;
            if (GlobalRestante <= 0f)
            {
                GlobalRestante = 0f;
                GlobalCorriendo = false;
                OnGlobalTimeUp?.Invoke();
            }
        }

        if (CapituloCorriendo)
        {
            CapituloRestante -= dt;
            if (CapituloRestante <= 0f)
            {
                CapituloRestante = 0f;
                CapituloCorriendo = false;
                OnChapterTimeUp?.Invoke();
            }
        }

        if (GlobalCorriendo || CapituloCorriendo)
            OnTick?.Invoke(GlobalRestante, CapituloRestante);
    }

    public void IniciarGlobal()
    {
        GlobalIniciado = true;
        GlobalRestante = segundosGlobal;
        GlobalCorriendo = true;
    }

    public void IniciarCapitulo(float segundos)
    {
        CapituloRestante = segundos;
        CapituloCorriendo = true;
    }

    public void DetenerCapitulo() => CapituloCorriendo = false;
    public void DetenerTodo() { GlobalCorriendo = false; CapituloCorriendo = false; }

    public static string Formato(float segundos)
    {
        segundos = Mathf.Max(0f, segundos);
        int m = (int)(segundos / 60f);
        int s = (int)(segundos % 60f);
        return $"{m:00}:{s:00}";
    }
}