using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Registra los pasos de Spider-Man y abre Flash tras un análisis correcto.</summary>
public class Cap1Manager : MonoBehaviour
{
    [SerializeField] ChapterSO capitulo;
    [SerializeField] XRSocketInteractor socketComic;
    [SerializeField] TablaHeroes tablaHeroes;
    [SerializeField] ClassificationTray[] bandejas;
    [SerializeField] ChemicalSO[] reactivosPorClasificar;
    [SerializeField] DNAExtractor dnAExtractor;
    [SerializeField] PinzasTritio pinzas;
    [SerializeField] MixingFlask matraz;
    [SerializeField] ReactorController reactor;
    [SerializeField] RecipeValidator validator;
    [SerializeField] DronXentari dron;
    [SerializeField] ChapterChecklist checklist;
    [SerializeField] BotonLab botonAnalizar;
    [SerializeField] TMP_Text tiempoNivel;
    [SerializeField] TMP_Text globalEnPanel;
    [SerializeField] PanelUI panelNivel;
    [SerializeField] SpiderAudioController audioSpider;
    bool avisoUnMinuto;

    const string TituloNivel = "Nivel 1 · Spider-Man";
    const string DescripcionNivel = "Doctor, investiga el origen del poder arácnido. Coloca el cómic, revela las pistas y prepara el suero. Cuando el reactor esté estable, pulsa ANALIZAR.";
    int ultimoSegundoNivel = -1;
    int ultimoSegundoGlobal = -1;
    [SerializeField] int puntosPorTarea = 20;
    [SerializeField] string escenaFlash = "02-Flash";

    readonly bool[] completados = new bool[7];
    readonly HashSet<ChemicalSO> clasificados = new HashSet<ChemicalSO>();
    TimerManager reloj;
    bool iniciado;
    bool tiempoAgotado;
    bool analisisSolicitado;
    bool cambiandoEscena;

    void OnEnable()
    {
        if (socketComic != null) socketComic.selectEntered.AddListener(AlAbrirComic);
        if (tablaHeroes != null) tablaHeroes.OnTablaCompletada += AlRevelarPistas;
        foreach (var bandeja in bandejas)
            if (bandeja != null) bandeja.OnReactivoClasificado += AlClasificar;
        if (dnAExtractor != null) dnAExtractor.OnADNExtraido += AlADNExtraido;
        if (pinzas != null) pinzas.OnTritioRecogido += AlTritiioObtenido;
        if (reactor != null) reactor.OnEstable += AlEstabilizar;
        if (validator != null) validator.OnValidado += AlValidar;
        if (botonAnalizar != null) botonAnalizar.OnPressed.AddListener(Analizar);
    }

    void OnDisable()
    {
        if (socketComic != null) socketComic.selectEntered.RemoveListener(AlAbrirComic);
        if (tablaHeroes != null) tablaHeroes.OnTablaCompletada -= AlRevelarPistas;
        foreach (var bandeja in bandejas)
            if (bandeja != null) bandeja.OnReactivoClasificado -= AlClasificar;
        if (dnAExtractor != null) dnAExtractor.OnADNExtraido -= AlADNExtraido;
        if (pinzas != null) pinzas.OnTritioRecogido -= AlTritiioObtenido;
        if (reactor != null) reactor.OnEstable -= AlEstabilizar;
        if (validator != null) validator.OnValidado -= AlValidar;
        if (botonAnalizar != null) botonAnalizar.OnPressed.RemoveListener(Analizar);
    }

    void Start()
    {
        reloj = TimerManager.Instance;
        if (socketComic != null)
        {
            if (reloj != null && !reloj.GlobalIniciado) reloj.IniciarGlobal();
            ScoreSystem.Instance?.IniciarSpider();
            audioSpider?.Iniciar();
        }
        ActualizarRelojes();
    }

    void Update()
    {
        ActualizarRelojes();
        if (tiempoAgotado || cambiandoEscena) return;
        if (SinTiempo())
        {
            AgotarTiempo();
            return;
        }
        if (!avisoUnMinuto && reloj != null &&
            ((iniciado && reloj.CapituloRestante <= 60f) ||
             (reloj.GlobalIniciado && reloj.GlobalRestante <= 60f)))
        {
            avisoUnMinuto = true;
            audioSpider?.UnMinuto();
        }
        if (!iniciado) return;

        if (matraz != null && matraz.Completa) AlMezclaCompleta();
        if (reactor != null && reactor.Estable) AlEstabilizar();
    }

    bool SinTiempo() => reloj != null &&
        ((reloj.GlobalIniciado && reloj.GlobalRestante <= 0f) ||
         (iniciado && reloj.CapituloRestante <= 0f));

    void ActualizarRelojes()
    {
        float segundosNivel = iniciado && reloj != null
            ? reloj.CapituloRestante
            : capitulo != null ? capitulo.tiempoSegundos : 0f;
        int nivel = Mathf.CeilToInt(segundosNivel);
        int global = reloj != null ? Mathf.CeilToInt(reloj.GlobalRestante) : 0;
        if (nivel == ultimoSegundoNivel && global == ultimoSegundoGlobal) return;

        ultimoSegundoNivel = nivel;
        ultimoSegundoGlobal = global;
        string capituloTexto = $"Nivel {TimerManager.Formato(nivel)}";
        string globalTexto = $"Global {TimerManager.Formato(global)}";
        if (tiempoNivel != null) tiempoNivel.text = $"{capituloTexto} | {globalTexto}";
        if (globalEnPanel != null) globalEnPanel.text = globalTexto;
        panelNivel?.SetTexto(TituloNivel, $"{DescripcionNivel}\n\nTiempo de capítulo  {TimerManager.Formato(nivel)}");
    }

    void AlAbrirComic(SelectEnterEventArgs args)
    {
        if (iniciado || tiempoAgotado || SinTiempo()) return;
        reloj = TimerManager.Instance;
        if (capitulo != null && reloj != null)
            reloj.IniciarCapitulo(capitulo.tiempoSegundos);
        GameManager.Instance?.SetState(GameState.Capitulo);
        AlComicInsertado();
    }

    public void AlComicInsertado()
    {
        iniciado = true;
        Completar(0);
    }

    void AlRevelarPistas() => Completar(1);

    void AlClasificar(ChemicalSO reactivo)
    {
        if (tiempoAgotado || cambiandoEscena || completados[2] ||
            System.Array.IndexOf(reactivosPorClasificar, reactivo) < 0 ||
            !clasificados.Add(reactivo)) return;

        checklist?.ActualizarClasificados(clasificados.Count, reactivosPorClasificar.Length);
        if (clasificados.Count == reactivosPorClasificar.Length) Completar(2);
    }

    public void AlADNExtraido() => Completar(3);
    public void AlTritiioObtenido() => Completar(4);
    public void AlMezclaCompleta() => Completar(5);

    void AlEstabilizar()
    {
        if (completados[5]) Completar(6);
    }

    public void Completar(int indice)
    {
        if (tiempoAgotado || cambiandoEscena || indice < 0 ||
            indice >= completados.Length || completados[indice]) return;
        completados[indice] = true;
        if (indice == 5 && reactor != null) reactor.HabilitarEstabilizacion();
        ScoreSystem.Instance?.Add(puntosPorTarea, $"Cap1 tarea {indice}");
        checklist?.Completar(indice);
        audioSpider?.PasoCompletado(indice);
    }

    public void Analizar()
    {
        if (cambiandoEscena || validator == null) return;
        if (tiempoAgotado || SinTiempo())
        {
            AgotarTiempo();
            validator.MostrarAviso("Tiempo agotado.");
            return;
        }

        // El matraz y el reactor conservan su aviso de error y su penalización.
        if (matraz != null && matraz.Completa) AlMezclaCompleta();
        if (reactor != null && reactor.Estable) AlEstabilizar();
        for (int i = 0; i < 5; i++)
        {
            if (completados[i]) continue;
            string aviso = $"Falta: {ChapterChecklist.NombrePaso(i)}.";
            validator.MostrarAviso(aviso);
            if (audioSpider != null) audioSpider.FaltaPaso(aviso);
            else NOVA.Instance?.Say(aviso);
            return;
        }

        analisisSolicitado = true;
        validator.Validate();
        analisisSolicitado = false;
    }

    void AlValidar(ResultadoValidacion resultado)
    {
        if (!analisisSolicitado || resultado != ResultadoValidacion.Correcto || cambiandoEscena)
            return;
        if (!completados[5] || !completados[6] || reactor == null || !reactor.Estable)
            return;
        if (!Application.CanStreamedLevelBeLoaded(escenaFlash))
        {
            Debug.LogError($"[Cap1] Agrega '{escenaFlash}' a la lista de escenas del perfil activo.");
            validator.MostrarAviso("Análisis correcto. Flash no está disponible en este perfil.");
            return;
        }

        cambiandoEscena = true;
        reloj?.DetenerCapitulo();
        ScoreSystem.Instance?.Add(puntosPorTarea, "Cap1 análisis correcto");
        float segundos = capitulo != null && reloj != null
            ? capitulo.tiempoSegundos - reloj.CapituloRestante
            : 0f;
        ScoreSystem.Instance?.GuardarSpider(segundos);
        audioSpider?.Cerrar(true);
        dron?.Activar(capitulo?.receta?.displayName ?? "Suero Arácnido");
        StartCoroutine(AbrirFlash());
    }

    void AgotarTiempo()
    {
        if (tiempoAgotado) return;
        tiempoAgotado = true;
        reloj?.DetenerCapitulo();
        ScoreSystem.Instance?.RegisterError(ErrorCode.E8_Tiempo, "Tiempo del capítulo agotado");
        validator?.MostrarAviso("Tiempo agotado.");
        if (audioSpider != null) audioSpider.Cerrar(false);
        else NOVA.Instance?.Say("Tiempo agotado. Reinicia el capítulo para volver a intentarlo.");
    }

    IEnumerator AbrirFlash()
    {
        yield return new WaitForSecondsRealtime(3f);
        while (audioSpider != null && audioSpider.Reproduciendo) yield return null;
        SceneManager.LoadSceneAsync(escenaFlash, LoadSceneMode.Single);
    }
}
