/*
 * DESCRIPCIÓN: Director de la escena Test_Cap1. Orquesta los 14 pasos del Capítulo 1
 * (Spider-Man), conecta todos los subsistemas y cierra el capítulo al terminar.
 * Índices del checklist:
 *   0-Cómic  1-Tabla  2-Clasificación  3-ADN  4-Tritio  5-Mezcla
 *   6-Reactor  7-Analizar  8-EMP
 */
using System.Collections;
using UnityEngine;

public class Cap1Manager : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] ChapterSO capitulo;

    [Header("Subsistemas")]
    [SerializeField] ComicReader comicReader;
    [SerializeField] TablaHeroes tablaHeroes;
    [SerializeField] ClassificationTray[] bandejas;   // 3 bandejas
    [SerializeField] DNAExtractor dnAExtractor;
    [SerializeField] TritiumChamber tritiumChamber;
    [SerializeField] MixingFlask matraz;
    [SerializeField] ReactorController reactor;
    [SerializeField] RecipeValidator validator;
    [SerializeField] EventDirector eventDirector;
    [SerializeField] DronXentari dron;
    [SerializeField] QuizPanel quiz;
    [SerializeField] ChapterChecklist checklist;
    [SerializeField] BotonLab botonAnalizar;

    [Header("Puntos por tarea")]
    [SerializeField] int puntosPorTarea = 20;

    // Estado interno
    readonly bool[] completados = new bool[9];
    bool capituloTerminado;

    void OnEnable()
    {
        if (ChapterManager.Instance != null)
            ChapterManager.Instance.OnChapterStarted += AlIniciarCapitulo;

        if (tablaHeroes != null)
            tablaHeroes.OnTablaCompletada += () => Completar(1);

        foreach (var b in bandejas)
            if (b != null) b.OnClasificacionCorrecta += _ => VerificarBandejas();

        if (validator != null)
            validator.OnValidado += AlValidar;

        if (quiz != null)
            quiz.OnQuizCompletado += AlQuizCompletado;

        if (botonAnalizar != null)
            botonAnalizar.OnPressed.AddListener(validator.Validate);

        if (TimerManager.Instance != null)
            TimerManager.Instance.OnChapterTimeUp += AlTiempoAgotado;
    }

    void OnDisable()
    {
        if (ChapterManager.Instance != null)
            ChapterManager.Instance.OnChapterStarted -= AlIniciarCapitulo;
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnChapterTimeUp -= AlTiempoAgotado;
        if (botonAnalizar != null)
            botonAnalizar.OnPressed.RemoveListener(validator.Validate);
    }

    void Start()
    {
        // Iniciar el capítulo automáticamente si no hay ComicReader
        if (comicReader == null && capitulo != null && ChapterManager.Instance != null)
            ChapterManager.Instance.StartChapter(capitulo);
    }

    void AlIniciarCapitulo(ChapterSO cap)
    {
        Debug.Log("[Cap1Manager] Capítulo iniciado.");
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Capítulo 1: Sentido Arácnido. Inserta el cómic para comenzar.");
    }

    // ── Completar tareas ─────────────────────────────────────────────────────

    public void Completar(int idx)
    {
        if (capituloTerminado || idx < 0 || idx >= completados.Length || completados[idx]) return;
        completados[idx] = true;

        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.Add(puntosPorTarea, $"Cap1 tarea {idx}");

        if (checklist != null) checklist.Completar(idx);
        Debug.Log($"[Cap1Manager] Tarea {idx} completada.");

        VerificarFin();
    }

    // Cómic insertado → tarea 0
    public void AlComicInsertado() => Completar(0);

    // Bandejas: todas deben tener al menos una correcta
    void VerificarBandejas()
    {
        foreach (var b in bandejas)
            if (b == null || !b.TieneCorrecta) return;
        Completar(2);
    }

    // ADN extraído → tarea 3 (llamar desde DNAExtractor via UnityEvent o polling)
    public void AlADNExtraido() => Completar(3);

    // Tritio obtenido con pinzas → tarea 4
    public void AlTritiioObtenido() => Completar(4);

    // Mezcla insertada → tarea 5 (se llama cuando se inserta el último reactivo)
    public void AlMezclaCompleta() => Completar(5);

    // Reactor estable → tarea 6
    void OnEnable2() { if (reactor != null) reactor.OnEstable += () => Completar(6); }

    // Validación → tarea 7
    void AlValidar(ResultadoValidacion r)
    {
        if (r == ResultadoValidacion.Correcto)
        {
            Completar(7);
            // Activar dron
            if (dron != null) dron.Activar(capitulo?.receta?.displayName ?? "Suero Arácnido");
            // Mostrar quiz
            if (quiz != null) quiz.gameObject.SetActive(true);
        }
    }

    // EMP resuelto → tarea 8
    public void AlEMPResuelto() => Completar(8);

    // Quiz completado → cierre
    void AlQuizCompletado()
    {
        Debug.Log("[Cap1Manager] Quiz completado → cerrando capítulo.");
        StartCoroutine(CerrarCapitulo());
    }

    void AlTiempoAgotado()
    {
        Debug.Log("[Cap1Manager] ⏱ Tiempo agotado.");
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.RegisterError(ErrorCode.E8_Tiempo, "Tiempo del capítulo agotado");
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Tiempo agotado. Calculando puntuación final.");
        StartCoroutine(CerrarCapitulo());
    }

    void VerificarFin()
    {
        if (checklist != null && checklist.TodoCompleto())
        {
            Debug.Log("[Cap1Manager] ✅ Todos los puntos del checklist completados.");
            StartCoroutine(CerrarCapitulo());
        }
    }

    IEnumerator CerrarCapitulo()
    {
        if (capituloTerminado) yield break;
        capituloTerminado = true;

        if (ChapterManager.Instance != null) ChapterManager.Instance.EndChapter();

        int score = ScoreSystem.Instance != null ? ScoreSystem.Instance.TotalScore : 0;
        string log = Bitacora.Instance != null ? Bitacora.Instance.ObtenerResumen() : "";

        Debug.Log($"[Cap1Manager] ── CIERRE ──\nPuntuación: {score}\nBitácora:\n{log}");

        if (NOVA.Instance != null)
            NOVA.Instance.Say($"Capítulo 1 completado. Puntuación: {score} puntos. Preparando Capítulo 2.");

        yield return new WaitForSecondsRealtime(3f);

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Resultado);

        Debug.Log("[Cap1Manager] GameManager → Resultado. (Carga de escena pendiente)");
    }
}
