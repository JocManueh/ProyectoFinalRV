using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using AvisoNova = FlashAudioController.Aviso;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Coordina las estaciones de Flash y actualiza el progreso y el tiempo de la escena.</summary>
public class FlashNivel : MonoBehaviour, IXRSelectFilter
{
    enum Paso { Clasificar, Bobina, ClasificarCarga, Mezclar, Estabilizar, Pico, Confirmar, Final, TiempoAgotado }

    static readonly string[] Objetivos =
    {
        "Preparar los compuestos", "Cargar la bobina",
        "Clasificar la carga", "Preparar la mezcla", "Estabilizar con la palanca",
        "Responder al pico de energía", "Confirmar el resultado"
    };

    [Header("Datos y estaciones de la escena")]
    [SerializeField] ChapterSO capitulo;
    [SerializeField] XRSocketInteractor[] socketsCompuestos;
    [SerializeField] XRSocketInteractor socketCarga;
    [SerializeField] BobinaFlash bobina;
    [SerializeField] MatrazFlash matraz;
    [SerializeField] XRSocketInteractor socketMatraz;

    [Header("Retroalimentación")]
    [SerializeField] TMP_Text panel;
    [SerializeField] FlashAudioController audioFlash;
    [SerializeField] TMP_Text tiempoNivel;
    [SerializeField] TMP_Text tiempoGlobal;
    [SerializeField] TMP_Text listaPasos;
    [SerializeField, Min(1f)] float segundosParaResponder = 8f;
    [SerializeField, Min(0)] int puntosAlCompletar = 100;

    bool recetaRevelada;
    bool avisadoUltimoMinuto;
    readonly List<XRGrabInteractable> ampollasObservadas = new List<XRGrabInteractable>();
    int segundosMostrados = -1;
    int segundosGlobalesMostrados = -1;
    int pasoMostrado = -1;

    readonly Dictionary<ChemicalSO, int> requeridas = new Dictionary<ChemicalSO, int>();
    readonly Dictionary<ChemicalSO, int> enBandeja = new Dictionary<ChemicalSO, int>();
    readonly HashSet<ChemicalContainer> ampollasContadas = new HashSet<ChemicalContainer>();
    ChemicalContainer carga;
    int totalCompuestos;
    int colocadas;
    bool filtrosInstalados;
    public bool canProcess => isActiveAndEnabled;
    TimerManager reloj;
    Paso paso;
    string aviso;
    bool palancaActiva;
    bool debeBajarPalanca;
    bool poseeCuenta;
    float estableDesde;
    float limitePico;

    float SegundosEstable => Mathf.Max(0.1f, capitulo.receta.segundosEstable);
    bool EnCurso => poseeCuenta;

    void Start()
    {
        reloj = TimerManager.Instance;
        if (reloj == null || capitulo == null || capitulo.receta == null || panel == null ||
            socketCarga == null || socketsCompuestos == null ||
            bobina == null || bobina.ampollaCarga == null || bobina.socket == null ||
            bobina.condensador == null || bobina.textoCarga == null || matraz == null || socketMatraz == null ||
            !LeerReceta())
        {
            Debug.LogError("[Flash] Revisa los campos de FlashNivel y la instancia de _Managers.", this);
            if (panel != null) panel.text = "Flash: falta configurar la escena.";
            enabled = false;
            return;
        }

        socketMatraz.enabled = false;
        foreach (var socket in socketsCompuestos) socket.selectFilters.Add(this);
        socketCarga.selectFilters.Add(this);
        filtrosInstalados = true;
        reloj.OnChapterTimeUp += AgotarTiempo;
        if (!reloj.GlobalIniciado) reloj.IniciarGlobal();
        RefrescarTiempoGlobal();
        reloj.IniciarCapitulo(capitulo.tiempoSegundos);
        poseeCuenta = true;
        matraz.ErrorDosis += AvisarNova;
        bobina.CargaInterrumpida += AvisarCargaInterrumpida;
        foreach (var ampolla in FindObjectsByType<ChemicalContainer>(FindObjectsInactive.Include))
        {
            if (ampolla.gameObject.scene != gameObject.scene ||
                !ampolla.TryGetComponent(out XRGrabInteractable agarre)) continue;
            agarre.selectExited.AddListener(AlSoltarAmpolla);
            ampollasObservadas.Add(agarre);
        }
        audioFlash?.Iniciar();
        GameManager.Instance?.SetState(GameState.Capitulo);
        CambiarPaso(Paso.Clasificar, "Lee el cómic para descubrir los ingredientes.");
        RefrescarPanel();
    }

    void Update()
    {
        RefrescarTiempoGlobal();
        if (!EnCurso) return;

        if (!avisadoUltimoMinuto && reloj.CapituloRestante > 0f && reloj.CapituloRestante <= 60f)
        {
            avisadoUltimoMinuto = true;
            AvisarNova(AvisoNova.UnMinuto);
        }

        float ahora = Time.unscaledTime;
        if (paso <= Paso.ClasificarCarga)
            ActualizarPreparacion();

        if (paso == Paso.Mezclar && matraz.Completa)
        {
            socketMatraz.enabled = false;
            debeBajarPalanca = palancaActiva;
            CambiarPaso(Paso.Estabilizar, "Enciende la palanca para estabilizar el reactor.");
            AvisarNova(AvisoNova.Estabilizar);
        }

        if (paso == Paso.Estabilizar && palancaActiva && !debeBajarPalanca &&
            ahora - estableDesde >= SegundosEstable)
        {
            limitePico = ahora + segundosParaResponder;
            CambiarPaso(Paso.Pico, "¡Pico de energía! Pulsa EMERGENCIA.");
            AvisarNova(AvisoNova.Pico);
        }

        if (paso == Paso.Pico && ahora >= limitePico)
            FallarPico();

        RefrescarPanel();
    }

    bool LeerReceta()
    {
        carga = bobina.ampollaCarga.GetComponent<ChemicalContainer>();
        if (carga == null || carga.sustancia == null) return false;

        int dosisCarga = 0;
        foreach (var pasoReceta in capitulo.receta.steps)
        {
            if (pasoReceta == null || pasoReceta.chemical == null || pasoReceta.ampollas < 1)
                return false;
            ChemicalSO sustancia = pasoReceta.chemical;
            if (sustancia == carga.sustancia)
            {
                dosisCarga += pasoReceta.ampollas;
                continue;
            }
            if (sustancia.type != ChemicalType.Compuesto) return false;
            requeridas.TryGetValue(sustancia, out int cantidad);
            requeridas[sustancia] = cantidad + pasoReceta.ampollas;
            totalCompuestos += pasoReceta.ampollas;
        }

        var unicos = new HashSet<XRSocketInteractor>(socketsCompuestos);
        return dosisCarga == 1 && totalCompuestos > 0 &&
            socketsCompuestos.Length == totalCompuestos &&
            unicos.Count == socketsCompuestos.Length &&
            !unicos.Contains(null) && !unicos.Contains(socketCarga);
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!EnCurso) return false;
        if (paso > Paso.ClasificarCarga) return interactor.IsSelecting(interactable);
        if (!ObtenerDosis(interactable, out ChemicalContainer ampolla)) return false;
        if (ReferenceEquals(interactor, socketCarga)) return ampolla == carga;
        if (!requeridas.TryGetValue(ampolla.sustancia, out int necesarias)) return false;

        int otrasDosis = 0;
        foreach (var socket in socketsCompuestos)
        {
            foreach (var seleccion in socket.interactablesSelected)
            {
                if (ObtenerDosis(seleccion, out ChemicalContainer otra) &&
                    otra != ampolla && otra.sustancia == ampolla.sustancia)
                    otrasDosis++;
            }
        }
        return otrasDosis < necesarias;
    }

    static bool ObtenerDosis(IXRSelectInteractable interactable, out ChemicalContainer ampolla)
    {
        return interactable.transform.TryGetComponent(out ampolla) &&
            !ampolla.EstaVacia && ampolla.volumen == 10;
    }

    void ActualizarPreparacion()
    {
        colocadas = 0;
        enBandeja.Clear();
        ampollasContadas.Clear();
        foreach (var socket in socketsCompuestos)
        {
            foreach (var seleccion in socket.interactablesSelected)
            {
                if (!ObtenerDosis(seleccion, out ChemicalContainer ampolla) ||
                    !requeridas.TryGetValue(ampolla.sustancia, out int necesarias) ||
                    !ampollasContadas.Add(ampolla))
                    continue;
                enBandeja.TryGetValue(ampolla.sustancia, out int cantidad);
                if (cantidad >= necesarias) continue;
                enBandeja[ampolla.sustancia] = cantidad + 1;
                colocadas++;
            }
        }

        if (colocadas < totalCompuestos)
        {
            if (paso != Paso.Clasificar)
                CambiarPaso(Paso.Clasificar, "Repón las ampollas que faltan en Compuesto.");
            return;
        }

        if (!carga.gameObject.activeInHierarchy)
        {
            if (paso != Paso.Bobina)
            {
                CambiarPaso(Paso.Bobina, "Coloca el condensador en Tesla y pulsa CARGAR.");
                AvisarNova(AvisoNova.Tesla);
            }
            return;
        }

        foreach (var seleccion in socketCarga.interactablesSelected)
        {
            if (!ObtenerDosis(seleccion, out ChemicalContainer ampolla) || ampolla != carga) continue;
            CambiarPaso(Paso.Mezclar, "Matraz desbloqueado. Retira las ampollas y mézclalas en orden.");
            AvisarNova(AvisoNova.Matraz);
            socketMatraz.enabled = true;
            return;
        }

        if (paso != Paso.ClasificarCarga)
        {
            CambiarPaso(Paso.ClasificarCarga, "Carga lista. Colócala en su bandeja.");
            AvisarNova(AvisoNova.Carga);
        }
    }

    public void PulsarTesla()
    {
        if (!EnCurso) return;
        if (paso <= Paso.ClasificarCarga) ActualizarPreparacion();
        if (paso != Paso.Bobina)
        {
            if (paso == Paso.Clasificar)
            {
                aviso = "Coloca todos los compuestos de la receta antes de usar Tesla.";
                AvisarNova(AvisoNova.Compuestos);
            }
            return;
        }

        bobina.IniciarCarga();
        aviso = bobina.socket.IsSelecting(bobina.condensador)
            ? "Deja el condensador en el socket hasta completar la carga."
            : "Coloca el condensador antes de pulsar CARGAR.";
        if (!bobina.socket.IsSelecting(bobina.condensador))
            AvisarNova(AvisoNova.Condensador);
    }

    public void SubirPalanca()
    {
        palancaActiva = true;
        if (EnCurso && paso == Paso.Estabilizar && !debeBajarPalanca)
            estableDesde = Time.unscaledTime;
    }

    public void BajarPalanca()
    {
        palancaActiva = false;
        debeBajarPalanca = false;
    }

    public void PulsarEmergencia()
    {
        if (!EnCurso) return;
        if (paso != Paso.Pico)
        {
            aviso = "Todavía no hay un pico que resolver.";
            return;
        }

        if (Time.unscaledTime >= limitePico)
        {
            FallarPico();
            return;
        }

        CambiarPaso(Paso.Confirmar, "Pico resuelto. Pulsa CONFIRMAR.");
        AvisarNova(AvisoNova.Confirmar);
    }

    void FallarPico()
    {
        ScoreSystem.Instance?.RegisterError(ErrorCode.E8_Tiempo, "Flash: pico sin resolver");
        debeBajarPalanca = palancaActiva;
        CambiarPaso(Paso.Estabilizar, "Se agotó la ventana. Baja y sube la palanca para reintentar.");
        AvisarNova(AvisoNova.PicoFallido);
    }

    public void PulsarConfirmar()
    {
        if (!EnCurso) return;
        if (paso != Paso.Confirmar)
        {
            aviso = "Completa la secuencia antes de confirmar.";
            return;
        }

        ScoreSystem.Instance?.Add(puntosAlCompletar, "Flash completado");
        Terminar(Paso.Final, $"Reacción confirmada. Flash completado. +{puntosAlCompletar} puntos.");
    }

    void AgotarTiempo()
    {
        ScoreSystem.Instance?.RegisterError(ErrorCode.E8_Tiempo, "Flash: tiempo de capítulo agotado");
        Terminar(Paso.TiempoAgotado, "Tiempo de Flash agotado.");
    }

    void Terminar(Paso resultado, string mensaje)
    {
        poseeCuenta = false;
        reloj.OnChapterTimeUp -= AgotarTiempo;
        reloj.DetenerCapitulo();
        socketMatraz.enabled = false;
        bobina.enabled = false;
        CambiarPaso(resultado, mensaje);
        audioFlash?.Terminar(resultado == Paso.Final);
        GameManager.Instance?.SetState(GameState.Resultado);
        RefrescarPanel();
    }

    public string TextoReloj()
    {
        if (reloj == null) return "FLASH  --:--";

        var texto = new StringBuilder("FLASH  ").Append(FormatearTiempo());
        if (paso == Paso.TiempoAgotado) texto.Append("  ·  Tiempo agotado");
        for (int i = 0; i < Objetivos.Length; i++)
        {
            int actual = paso == Paso.TiempoAgotado ? ultimoPasoJugado : (int)paso;
            string marca = i < actual ? "[✓] " : i == actual ? "[>] " : "[ ] ";
            texto.Append('\n').Append(marca).Append(Objetivos[i]);
        }
        return texto.ToString();
    }

    int ultimoPasoJugado;

    void CambiarPaso(Paso nuevo, string mensaje)
    {
        ultimoPasoJugado = (int)paso;
        paso = nuevo;
        aviso = mensaje;
    }

    string FormatearTiempo() => TimerManager.Formato(Mathf.Ceil(reloj.CapituloRestante));

    public void RevelarReceta()
    {
        if (recetaRevelada) return;
        recetaRevelada = true;
        if (paso == Paso.Clasificar)
        {
            aviso = "Deja los compuestos de la receta en su bandeja.";
            AvisarNova(AvisoNova.Ingredientes);
        }
        if (isActiveAndEnabled) RefrescarPanel();
    }

    void RefrescarTiempoGlobal()
    {
        if (reloj == null || tiempoGlobal == null) return;

        int segundos = Mathf.CeilToInt(reloj.GlobalRestante);
        if (segundosGlobalesMostrados == segundos) return;
        segundosGlobalesMostrados = segundos;
        tiempoGlobal.text = TimerManager.Formato(segundos);
    }

    void RefrescarMenuMano()
    {
        int segundos = Mathf.CeilToInt(reloj.CapituloRestante);
        if (segundosMostrados != segundos && tiempoNivel != null)
        {
            segundosMostrados = segundos;
            tiempoNivel.text = TimerManager.Formato(segundos);
        }

        int actual = paso == Paso.TiempoAgotado ? ultimoPasoJugado : (int)paso;
        if (pasoMostrado == actual || listaPasos == null) return;
        pasoMostrado = actual;
        var lista = new StringBuilder();
        for (int i = 0; i < Objetivos.Length; i++)
        {
            if (i > 0) lista.Append('\n');
            string renglon = $"{i + 1}. {Objetivos[i]}";
            if (i == actual)
                lista.Append("<b>").Append(renglon).Append("</b>");
            else if (i == actual - 1)
                lista.Append("<u>").Append(renglon).Append("</u>");
            else
                lista.Append(renglon);
        }
        listaPasos.text = lista.ToString();
    }

    void RefrescarPanel()
    {
        if (reloj == null) return;
        RefrescarMenuMano();
        var receta = new StringBuilder();
        if (!recetaRevelada)
            receta.Append("\nIngredientes sin descubrir");
        else
        {
            receta.Append("\nReceta");
            for (int i = 0; i < capitulo.receta.steps.Count; i++)
            {
                var ingrediente = capitulo.receta.steps[i];
                receta.Append($"\n{i + 1}. {ingrediente.chemical.displayName} ×{ingrediente.ampollas}");
            }
        }

        string detalle = "";
        if (paso <= Paso.ClasificarCarga)
            detalle = $"\nCompuestos en bandeja: {colocadas}/{totalCompuestos}";
        else if (paso == Paso.Estabilizar && palancaActiva && !debeBajarPalanca)
            detalle = $"\nEstabilidad: {Mathf.Min(SegundosEstable, Time.unscaledTime - estableDesde):0.0}/{SegundosEstable:0.0} s";
        else if (paso == Paso.Pico)
            detalle = $"\nTiempo para responder: {Mathf.CeilToInt(Mathf.Max(0f, limitePico - Time.unscaledTime))} s";

        string titulo = (int)paso < Objetivos.Length ? Objetivos[(int)paso] : "Resultado";
        string texto = $"{titulo}\n{aviso}{detalle}{receta}";
        if (panel.text != texto) panel.text = texto;
    }

    void AvisarNova(AvisoNova avisoNova)
    {
        if (EnCurso && audioFlash != null) audioFlash.Avisar(avisoNova);
    }

    void AvisarCargaInterrumpida() => AvisarNova(AvisoNova.CargaInterrumpida);

    void AlSoltarAmpolla(SelectExitEventArgs evento)
    {
        if (!EnCurso || paso > Paso.ClasificarCarga || evento.isCanceled ||
            evento.interactorObject is XRSocketInteractor ||
            !(evento.interactableObject is XRGrabInteractable ampolla) || ampolla.isSelected)
            return;

        XRSocketInteractor bandeja = null;
        float menorDistancia = float.PositiveInfinity;
        RevisarCercania(socketCarga, ampolla, ref bandeja, ref menorDistancia);
        foreach (var socket in socketsCompuestos)
            RevisarCercania(socket, ampolla, ref bandeja, ref menorDistancia);

        if (bandeja == null || Process(bandeja, ampolla)) return;
        aviso = "Esa ampolla no va aquí. Revisa la receta.";
        AvisarNova(AvisoNova.Bandeja);
    }

    static void RevisarCercania(XRSocketInteractor socket, XRGrabInteractable ampolla,
        ref XRSocketInteractor mejor, ref float distancia)
    {
        Collider zona = socket.GetComponent<Collider>();
        if (zona == null) return;

        foreach (var collider in ampolla.colliders)
        {
            if (collider == null || !zona.bounds.Intersects(collider.bounds)) continue;
            float cerca = (zona.bounds.center - collider.bounds.center).sqrMagnitude;
            if (cerca >= distancia) continue;
            distancia = cerca;
            mejor = socket;
        }
    }

    void OnDestroy()
    {
        if (matraz != null) matraz.ErrorDosis -= AvisarNova;
        if (bobina != null) bobina.CargaInterrumpida -= AvisarCargaInterrumpida;
        foreach (var ampolla in ampollasObservadas)
            if (ampolla != null) ampolla.selectExited.RemoveListener(AlSoltarAmpolla);
        if (filtrosInstalados)
        {
            foreach (var socket in socketsCompuestos)
                if (socket != null) socket.selectFilters.Remove(this);
            if (socketCarga != null) socketCarga.selectFilters.Remove(this);
        }
        if (reloj == null) return;
        reloj.OnChapterTimeUp -= AgotarTiempo;
        if (poseeCuenta) reloj.DetenerCapitulo();
    }
}
