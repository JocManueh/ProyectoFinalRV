/*
 * DESCRIPCIÓN: Director de capítulo. Recibe un ChapterSO, arranca el timer,
 * expone el estado del capítulo al resto del juego y avisa cuando termina.
 * EventDirector lo escucha para disparar el EMP al 50 % del tiempo.
 */
using System;
using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager Instance { get; private set; }

    public ChapterSO CapituloActual { get; private set; }
    public bool Activo { get; private set; }

    public event Action<ChapterSO> OnChapterStarted;
    public event Action             OnChapterEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void StartChapter(ChapterSO cap)
    {
        if (cap == null) { Debug.LogWarning("[ChapterManager] ChapterSO nulo."); return; }
        CapituloActual = cap;
        Activo = true;

        if (TimerManager.Instance != null)
            TimerManager.Instance.IniciarCapitulo(cap.tiempoSegundos);

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Capitulo);

        Debug.Log($"[ChapterManager] Capítulo iniciado: {cap.titulo} ({cap.tiempoSegundos}s)");
        OnChapterStarted?.Invoke(cap);
    }

    public void EndChapter()
    {
        if (!Activo) return;
        Activo = false;
        if (TimerManager.Instance != null) TimerManager.Instance.DetenerCapitulo();
        Debug.Log($"[ChapterManager] Capítulo terminado: {CapituloActual?.titulo}");
        OnChapterEnded?.Invoke();
    }
}
