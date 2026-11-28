/*
 * DESCRIPCIÓN: Guía de NOVA para el Capítulo 1. Habla al completar cada paso del
 * checklist, da una pista si el jugador se atasca, ayuda cuando repite un mismo
 * error y avisa del tiempo. Centraliza los mensajes: los errores puntuales ya
 * los dicen MixingFlask, TritioItem y RecipeValidator, aquí solo se refuerzan.
 */
using UnityEngine;

public class NovaGuia : MonoBehaviour
{
    [System.Serializable]
    public class Linea
    {
        [TextArea(1, 3)] public string texto;
        public AudioClip clip;   // opcional: si está vacío solo se oye el pitido
    }

    [SerializeField] ChapterChecklist checklist;

    [Header("Al completar cada paso (mismo orden del checklist, 9)")]
    [SerializeField] Linea[] alCompletar = new Linea[9];

    [Header("Pista si se atasca (una por paso pendiente, 9)")]
    [SerializeField] Linea[] pistaPorPaso = new Linea[9];
    [SerializeField] float segundosSinProgreso = 45f;

    [Header("Ayuda al repetir un error (orden: E1..E8)")]
    [SerializeField] Linea[] ayudaPorError = new Linea[8];
    [SerializeField] int erroresParaAyuda = 3;

    [Header("Tiempo")]
    [SerializeField] Linea unMinuto;
    [SerializeField] Linea tiempoAgotado;

    float ultimoProgreso;
    int ultimoLog;
    bool avisoMinuto;
    readonly int[] conteoErrores = new int[8];

    void Start()
    {
        ultimoProgreso = Time.unscaledTime;
        if (checklist != null) checklist.OnCompletado += AlCompletar;
        if (ScoreSystem.Instance != null) ScoreSystem.Instance.OnScoreChanged += AlCambiarPuntaje;
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTick += AlTick;
            TimerManager.Instance.OnChapterTimeUp += AlAgotarse;
        }
    }

    void OnDestroy()
    {
        if (checklist != null) checklist.OnCompletado -= AlCompletar;
        if (ScoreSystem.Instance != null) ScoreSystem.Instance.OnScoreChanged -= AlCambiarPuntaje;
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTick -= AlTick;
            TimerManager.Instance.OnChapterTimeUp -= AlAgotarse;
        }
    }

    void Update()
    {
        if (checklist == null || checklist.TodoCompleto()) return;
        if (Time.unscaledTime - ultimoProgreso < segundosSinProgreso) return;
        if (NOVA.Instance != null && NOVA.Instance.EstaHablando) return;

        ultimoProgreso = Time.unscaledTime;   // repite la pista cada N segundos
        int pendiente = checklist.SiguientePendiente();
        if (pendiente >= 0 && pendiente < pistaPorPaso.Length) Decir(pistaPorPaso[pendiente]);
    }

    void AlCompletar(int idx)
    {
        ultimoProgreso = Time.unscaledTime;
        if (idx >= 0 && idx < alCompletar.Length) Decir(alCompletar[idx]);
    }

    void AlCambiarPuntaje(int _)
    {
        var log = ScoreSystem.Instance.Log;
        if (log.Count < ultimoLog)   // se reinició el score
        {
            ultimoLog = 0;
            System.Array.Clear(conteoErrores, 0, conteoErrores.Length);
        }
        while (ultimoLog < log.Count)
        {
            var e = log[ultimoLog++];
            if (!System.Enum.TryParse(e.codigo, out ErrorCode c)) continue;
            int i = (int)c;
            if (i < 0 || i >= conteoErrores.Length) continue;
            conteoErrores[i]++;
            if (conteoErrores[i] == erroresParaAyuda && i < ayudaPorError.Length)
                Decir(ayudaPorError[i]);
        }
    }

    void AlTick(float global, float capitulo)
    {
        if (avisoMinuto || capitulo > 60f || capitulo <= 0f) return;
        avisoMinuto = true;
        Decir(unMinuto);
    }

    void AlAgotarse() => Decir(tiempoAgotado);

    void Decir(Linea l)
    {
        if (l == null || string.IsNullOrWhiteSpace(l.texto) || NOVA.Instance == null) return;
        NOVA.Instance.Say(l.texto, l.clip);
    }

    // Se rellena solo al añadir el componente en el Editor. Edita los textos a tu gusto.
    void Reset()
    {
        alCompletar = new[]
        {
            L("Cómic leído. Revisa la tabla de héroes."),
            L("Entendido: la fórmula necesita algo de una araña."),
            L("Reactivos clasificados. Ahora extrae el ADN."),
            L("ADN de araña obtenido."),
            L("Bien hecho con las pinzas. El tritio va siempre al final."),
            L("Mezcla lista. Ajusta la temperatura entre 35 y 40 grados."),
            L("Reactor estable. Pulsa ANALIZAR."),
            L("¡Fórmula correcta! El suero arácnido está listo."),
            L("Sistemas restaurados.")
        };
        pistaPorPaso = new[]
        {
            L("Inserta el cómic en el lector para empezar."),
            L("Toca las palabras clave de la tabla para revelar las pistas."),
            L("Cada ampolla va a la bandeja que corresponde a su tipo."),
            L("La araña entera no sirve. Extrae su ADN primero."),
            L("Abre la cámara blindada y usa las pinzas. Nunca la mano."),
            L("Salina dos veces, luego ADN de araña, y al final el tritio."),
            L("Lleva la palanca del reactor entre 35 y 40 grados."),
            L("Pulsa ANALIZAR para validar la mezcla."),
            L("Pulsa el botón rojo del generador.")
        };
        ayudaPorError = new[]
        {
            L("Revisa la estantería: algunas ampollas están vacías a propósito."),
            L("Una sustancia necesita procesarse antes de usarla."),
            L("Mercurio, cloro y cafeína no van en la fórmula."),
            L("Recuerda: salina dos veces, ADN una vez, tritio una vez."),
            L("El orden es salina, ADN de araña y tritio al final."),
            L("Mantén la temperatura entre 35 y 40 grados cinco segundos."),
            L("El tritio solo se toca con las pinzas."),
            L("Date prisa o se agotará el tiempo.")
        };
        unMinuto = L("Queda un minuto.");
        tiempoAgotado = L("Se acabó el tiempo. Revisemos qué falló.");
    }

    static Linea L(string t) => new Linea { texto = t };
}