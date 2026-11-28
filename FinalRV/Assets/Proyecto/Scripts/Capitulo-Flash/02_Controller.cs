// DESCRIPCIÓN:Crea y controla el capítulo independiente de Flash. Construye el laboratorio,
// permite agarrar y clasificar ampollas, cargar la bobina, preparar la mezcla,
// resolver la sobrecarga, responder el quiz y pasar al menú o al siguiente nivel.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Controller : MonoBehaviour
{
    [Header("Escenas")]
    public string escenaMenu = "00_Menu";
    public string escenaHulk = "03_Hulk";

    [Serializable]
    class Resultado
    {
        public string capitulo = "The Flash";
        public bool completado;
        public int puntos;
        public float segundosUsados;
        public List<string> errores = new List<string>();
    }

    [Serializable]
    class Historial
    {
        public List<Resultado> intentos = new List<Resultado>();
    }

    readonly Resultado resultado = new Resultado();
    readonly Dictionary<Transform, string> sustancias = new Dictionary<Transform, string>();
    readonly HashSet<Transform> vacias = new HashSet<Transform>();
    readonly HashSet<string> clasificados = new HashSet<string>();
    readonly HashSet<int> pistas = new HashSet<int>();
    readonly string[] receta = { "Agua", "Agua", "NaCl", "KCl", "Glucosa", "Carga" };

    TMP_Text pantalla;
    TMP_Text aviso;
    TMP_Text indicador;
    XRSocketInteractor socketTesla;
    XRGrabInteractable condensador;
    XRSimpleInteractable botonCarga;
    Transform carga;
    GameObject quiz;
    GameObject final;
    GameObject siguiente;
    ParticleSystem chispas;
    ParticleSystem burbujas;
    ParticleSystem humo;
    AudioSource alarma;

    float reloj = 240f;
    float temperatura = 20f;
    float voltaje;
    float estable;
    float tiempoCarga;
    float tiempoAlto;
    float tiempoEmergencia;
    float escalaAnterior;
    float pasoAnterior;
    int paso;
    bool eventoIniciado;
    bool eventoTerminado;
    bool eventoGanado;
    bool explosion;
    bool lenta;
    bool analizado;
    bool cerrado;
    bool esperarSoltar;

    void Start()
    {
        Caja("Piso", transform, new Vector3(0, -0.05f, 1),
            new Vector3(5, 0.1f, 5), Color.gray);
        Caja("Mesa", transform, new Vector3(0, 0.78f, 0.95f),
            new Vector3(2.25f, 0.08f, 0.75f), new Color(0.18f, 0.22f, 0.28f));

        pantalla = Texto("Reloj", transform, new Vector3(0, 1.80f, 1.42f), "", 0.10f);
        aviso = Texto("Mensaje", transform, new Vector3(0, 1.62f, 1.42f),
            "Lee las tres pistas. Después clasifica los reactivos.", 0.075f);

        string[] ingredientes = { "Agua", "Agua", "NaCl", "KCl", "Glucosa", "Cobre", "Azufre" };
        for (int i = 0; i < ingredientes.Length; i++)
            Ampolla(ingredientes[i], new Vector3(-0.92f + i * 0.14f, 0.90f, 0.68f));

        string[] categorias = { "Elemento", "Compuesto", "Carga" };
        for (int i = 0; i < categorias.Length; i++)
        {
            string categoria = categorias[i];
            var bandeja = Socket(categoria, new Vector3(-0.88f + i * 0.25f, 0.84f, 1.10f));
            bandeja.selectEntered.AddListener(e => Clasificar(e.interactableObject.transform, categoria));
        }

        var matraz = Socket("MATRAZ", new Vector3(0.12f, 0.84f, 1.10f));
        matraz.selectEntered.AddListener(e => Mezclar(e.interactableObject.transform));

        socketTesla = Socket("TESLA", new Vector3(0.56f, 0.84f, 1.10f));
        condensador = Agarrable("Condensador", new Vector3(0.77f, 0.90f, 0.70f), Color.yellow);
        botonCarga = Boton("CARGAR", transform, new Vector3(0.86f, 0.87f, 1.10f), null);
        indicador = Texto("Indicador", transform, new Vector3(0.72f, 1.05f, 1.18f), "Carga 0%", 0.06f);

        Boton("T −5", transform, new Vector3(-0.90f, 1.22f, 1.22f),
            () => temperatura = Mathf.Max(20f, temperatura - 5f));
        Boton("T +5", transform, new Vector3(-0.66f, 1.22f, 1.22f),
            () => temperatura = Mathf.Min(80f, temperatura + 5f));
        Boton("V −10", transform, new Vector3(-0.40f, 1.22f, 1.22f),
            () => voltaje = Mathf.Max(0f, voltaje - 10f));
        Boton("V +10", transform, new Vector3(-0.14f, 1.22f, 1.22f),
            () => voltaje = Mathf.Min(100f, voltaje + 10f));
        Boton("EMERGENCIA", transform, new Vector3(0.28f, 1.22f, 1.22f), ResolverEmergencia);
        Boton("ANALIZAR", transform, new Vector3(0.76f, 1.22f, 1.22f), Analizar);

        string[] textos =
        {
            "Barry Allen adquiere velocidad en un accidente ficticio de laboratorio.",
            "El agua destilada conduce muy poco. Los iones de las sales transportan carga.",
            "Fórmula: agua x2, NaCl, KCl, glucosa y carga. Reactor: 20–30 °C y 40–60%."
        };
        for (int i = 0; i < textos.Length; i++)
        {
            int indice = i;
            Boton("PISTA " + (i + 1), transform,
                new Vector3(-0.55f + i * 0.55f, 1.42f, 1.30f), () =>
                {
                    pistas.Add(indice);
                    aviso.text = textos[indice];
                });
        }

        quiz = Grupo("PanelQuiz");
        Boton("Iones", quiz.transform, new Vector3(-0.42f, 1.05f, 0.68f), () => Responder(true));
        Boton("Calor", quiz.transform, new Vector3(0, 1.05f, 0.68f), () => Responder(false));
        Boton("Luz", quiz.transform, new Vector3(0.42f, 1.05f, 0.68f), () => Responder(false));
        quiz.SetActive(false);

        final = Grupo("PanelFinal");
        siguiente = Boton("SIGUIENTE", final.transform, new Vector3(-0.44f, 1.36f, 0.72f),
            () => Abrir(escenaHulk)).gameObject;
        Boton("MENÚ", final.transform, new Vector3(0, 1.36f, 0.72f), () => Abrir(escenaMenu));
        Boton("REINTENTAR", final.transform, new Vector3(0.44f, 1.36f, 0.72f),
            () => Abrir(SceneManager.GetActiveScene().name));
        final.SetActive(false);

        chispas = Efecto("Chispas", new Vector3(0.56f, 0.95f, 1.10f), Color.cyan);
        burbujas = Efecto("Burbujas", new Vector3(0.12f, 0.95f, 1.10f), Color.white);
        humo = Efecto("Humo", new Vector3(0.12f, 0.95f, 1.10f), Color.gray);

        alarma = gameObject.AddComponent<AudioSource>();
        float[] muestras = new float[11025];
        for (int i = 0; i < muestras.Length; i++)
            muestras[i] = 0.12f * Mathf.Sin(2f * Mathf.PI * 440f * i / 22050f);
        alarma.clip = AudioClip.Create("Alarma Tesla", muestras.Length, 1, 22050, false);
        alarma.clip.SetData(muestras, 0);
        alarma.loop = true;
    }

    void Update()
    {
        if (cerrado) return;
        float dt = Time.unscaledDeltaTime;
        reloj = Mathf.Max(0f, reloj - dt);
        int segundos = Mathf.CeilToInt(reloj);
        bool enRango = temperatura >= 20 && temperatura <= 30 && voltaje >= 40 && voltaje <= 60;
        estable = enRango ? estable + dt : 0f;
        pantalla.text = $"{segundos / 60:00}:{segundos % 60:00}   {temperatura:0} °C   " +
                        $"{voltaje:0}%   Estable {Mathf.Min(5, estable):0.0}/5";
        if (reloj <= 0f) { Cerrar(false, false); return; }

        if (!botonCarga.isSelected) esperarSoltar = false;
        if (carga == null && !esperarSoltar)
        {
            tiempoCarga = socketTesla.IsSelecting(condensador) && botonCarga.isSelected
                ? tiempoCarga + dt : 0f;
            indicador.text = $"Carga {Mathf.Clamp01(tiempoCarga / 3f):P0}";
            if (tiempoCarga >= 3f)
            {
                carga = Ampolla("Carga", new Vector3(0.93f, 0.92f, 0.72f));
                aviso.text = "Carga lista. Clasifícala antes de añadirla al matraz.";
            }
        }

        if (voltaje > 70)
        {
            if (!burbujas.isPlaying) burbujas.Play();
            tiempoAlto += dt;
            if (tiempoAlto >= 3f && !explosion)
            {
                explosion = true;
                humo.Emit(30);
                Error("E6", "Voltaje demasiado alto.");
            }
        }
        else
        {
            if (burbujas.isPlaying) burbujas.Stop();
            tiempoAlto = 0;
            explosion = false;
        }

        if (!eventoIniciado && reloj <= 120f)
        {
            eventoIniciado = true;
            tiempoEmergencia = 8f;
            chispas.Play();
            alarma.Play();
            aviso.text = "¡Sobrecarga! Pulsa EMERGENCIA en ocho segundos.";
        }
        if (eventoIniciado && !eventoTerminado)
        {
            tiempoEmergencia -= dt;
            if (tiempoEmergencia <= 0f)
            {
                FinEvento();
                if (paso == receta.Length) paso--;
                if (carga != null)
                {
                    sustancias.Remove(carga);
                    vacias.Remove(carga);
                    Destroy(carga.gameObject);
                }
                carga = null;
                tiempoCarga = 0;
                esperarSoltar = true;
                indicador.text = "Carga 0%";
                Error("E8", "Se perdió la carga. Carga la bobina otra vez.");
            }
        }
    }

    void Clasificar(Transform objeto, string bandeja)
    {
        if (cerrado || !sustancias.TryGetValue(objeto, out string nombre)) return;
        string correcta = nombre == "Carga" ? "Carga" :
            (nombre == "Cobre" || nombre == "Azufre" ? "Elemento" : "Compuesto");
        if (bandeja != correcta) { Error("E3", "Bandeja incorrecta."); return; }
        clasificados.Add(nombre);
        aviso.text = $"Correcto: {nombre}. Tipos clasificados: {clasificados.Count}/7.";
    }

    void Mezclar(Transform objeto)
    {
        if (cerrado || analizado || !sustancias.TryGetValue(objeto, out string nombre)) return;
        if (vacias.Contains(objeto)) { Error("E1", "Ampolla vacía."); return; }
        if (nombre == "Carga" && paso < 4) { Error("E3", "Añade los dos electrolitos primero."); return; }
        if (Array.IndexOf(receta, nombre) < 0) { Error("E3", "Ese reactivo no pertenece a la fórmula."); return; }
        if (paso >= receta.Length || nombre != receta[paso])
        {
            bool sobranDosis = Array.LastIndexOf(receta, nombre) < paso;
            Error(sobranDosis ? "E4" : "E5", "Revisa la cantidad y el orden.");
            return;
        }
        vacias.Add(objeto);
        objeto.GetComponent<Renderer>().material.color = Color.gray;
        paso++;
        aviso.text = $"Dosis aceptada: {paso}/6. Retira la ampolla del socket.";
    }

    void ResolverEmergencia()
    {
        if (cerrado || !eventoIniciado || eventoTerminado) return;
        eventoGanado = true;
        FinEvento();
        aviso.text = "Sobrecarga resuelta.";
    }

    void FinEvento()
    {
        eventoTerminado = true;
        alarma.Stop();
        chispas.Stop();
    }

    void Analizar()
    {
        if (cerrado || analizado) return;
        if (pistas.Count < 3) { aviso.text = "Lee las tres pistas."; return; }
        if (clasificados.Count < 7) { aviso.text = "Clasifica los siete tipos de reactivo."; return; }
        if (paso != receta.Length) { aviso.text = "Faltan dosis en el matraz."; return; }
        if (!eventoTerminado) { aviso.text = "Atiende la sobrecarga de mitad del capítulo."; return; }
        if (estable < 5f) { aviso.text = "Mantén temperatura y voltaje en rango cinco segundos."; return; }
        analizado = true;
        StartCoroutine(Poder());
    }

    IEnumerator Poder()
    {
        aviso.text = "Reflejos acelerados.";
        escalaAnterior = Time.timeScale;
        pasoAnterior = Time.fixedDeltaTime;
        lenta = true;
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = pasoAnterior * 0.5f;
        yield return new WaitForSecondsRealtime(5f);
        RestaurarTiempo();
        if (cerrado) yield break;
        aviso.text = "Quiz: ¿qué transporta carga en las sales disueltas?";
        quiz.SetActive(true);
    }

    void Responder(bool correcta)
    {
        if (!cerrado && quiz.activeSelf) Cerrar(true, correcta);
    }

    void Cerrar(bool completo, bool quizCorrecto)
    {
        cerrado = true;
        RestaurarTiempo();
        alarma.Stop();
        chispas.Stop();
        burbujas.Stop();
        quiz.SetActive(false);
        resultado.completado = completo;
        resultado.segundosUsados = 240f - reloj;

        int penalizacion = 0;
        foreach (string error in resultado.errores)
            penalizacion += error.StartsWith("E6 ") || error.StartsWith("E5 ") ? 25 :
                            error.StartsWith("E8 ") ? 0 : 10;
        resultado.puntos = Mathf.Max(0, (analizado ? 180 : 0) + (completo ? 50 : 0) +
            (eventoGanado ? 40 : 0) + (quizCorrecto ? 30 : 0) - penalizacion);

        aviso.text = completo ? $"Flash completado. {resultado.puntos} puntos." : "Tiempo agotado.";
        siguiente.SetActive(completo);
        final.SetActive(true);

        string ruta = Path.Combine(Application.persistentDataPath, "resultados_flash.json");
        try
        {
            Historial historial = File.Exists(ruta)
                ? JsonUtility.FromJson<Historial>(File.ReadAllText(ruta)) : new Historial();
            if (historial == null) historial = new Historial();
            if (historial.intentos == null) historial.intentos = new List<Resultado>();
            historial.intentos.Add(resultado);
            File.WriteAllText(ruta, JsonUtility.ToJson(historial, true));
            Debug.Log("[Flash] Resultado guardado en " + ruta);
        }
        catch (Exception error)
        {
            Debug.LogError("[Flash] No se pudo guardar el resultado: " + error.Message);
        }
    }

    void Error(string codigo, string detalle)
    {
        resultado.errores.Add($"{codigo} · {240f - reloj:0.0}s · {detalle}");
        aviso.text = codigo + " · " + detalle;
    }

    void RestaurarTiempo()
    {
        if (!lenta) return;
        Time.timeScale = escalaAnterior;
        Time.fixedDeltaTime = pasoAnterior;
        lenta = false;
    }

    void OnDisable() => RestaurarTiempo();

    void Abrir(string escena)
    {
        if (string.IsNullOrWhiteSpace(escena) || !Application.CanStreamedLevelBeLoaded(escena))
        {
            aviso.text = $"Falta agregar '{escena}' a la lista de escenas de Unity.";
            return;
        }
        SceneManager.LoadScene(escena);
    }

    GameObject Grupo(string nombre)
    {
        var grupo = new GameObject(nombre);
        grupo.transform.SetParent(transform, false);
        return grupo;
    }

    GameObject Caja(string nombre, Transform padre, Vector3 pos, Vector3 tam, Color color)
    {
        var caja = GameObject.CreatePrimitive(PrimitiveType.Cube);
        caja.name = nombre;
        caja.transform.SetParent(padre, false);
        caja.transform.localPosition = pos;
        caja.transform.localScale = tam;
        caja.GetComponent<Renderer>().material.color = color;
        return caja;
    }

    TMP_Text Texto(string nombre, Transform padre, Vector3 pos, string contenido, float tam)
    {
        var objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        objeto.transform.localPosition = pos;
        var texto = objeto.AddComponent<TextMeshPro>();
        texto.alignment = TextAlignmentOptions.Center;
        texto.fontSize = 5f;
        texto.text = contenido;
        texto.rectTransform.sizeDelta = new Vector2(24, 3);
        objeto.transform.localScale = Vector3.one * tam;
        return texto;
    }

    XRSimpleInteractable Boton(string nombre, Transform padre, Vector3 pos, Action accion)
    {
        var baseBoton = Caja(nombre, padre, pos, new Vector3(0.19f, 0.065f, 0.075f), Color.blue);
        Texto(nombre + " texto", padre, pos + new Vector3(0, 0.055f, -0.03f), nombre, 0.055f);
        var boton = baseBoton.AddComponent<XRSimpleInteractable>();
        if (accion != null) boton.selectEntered.AddListener(_ => accion());
        return boton;
    }

    XRGrabInteractable Agarrable(string nombre, Vector3 pos, Color color)
    {
        var objeto = Caja(nombre, transform, pos, new Vector3(0.065f, 0.11f, 0.065f), color);
        var cuerpo = objeto.AddComponent<Rigidbody>();
        cuerpo.mass = 0.1f;
        var agarre = objeto.AddComponent<XRGrabInteractable>();
        var etiqueta = Texto(nombre + " texto", transform, pos + Vector3.up * 0.085f, nombre, 0.05f);
        etiqueta.transform.SetParent(objeto.transform, true);
        return agarre;
    }

    Transform Ampolla(string nombre, Vector3 pos)
    {
        var objeto = Agarrable(nombre, pos, nombre == "Carga" ? Color.yellow : Color.cyan);
        sustancias.Add(objeto.transform, nombre);
        return objeto.transform;
    }

    XRSocketInteractor Socket(string nombre, Vector3 pos)
    {
        Caja("Base " + nombre, transform, pos, new Vector3(0.16f, 0.025f, 0.16f), Color.black);
        Texto(nombre + " texto", transform, pos + new Vector3(0, 0.045f, -0.09f), nombre, 0.05f);
        var zona = new GameObject("Socket " + nombre);
        zona.transform.SetParent(transform, false);
        zona.transform.localPosition = pos + Vector3.up * 0.085f;
        var esfera = zona.AddComponent<SphereCollider>();
        esfera.radius = 0.075f;
        esfera.isTrigger = true;
        return zona.AddComponent<XRSocketInteractor>();
    }

    ParticleSystem Efecto(string nombre, Vector3 pos, Color color)
    {
        var objeto = Grupo(nombre);
        objeto.transform.localPosition = pos;
        var efecto = objeto.AddComponent<ParticleSystem>();
        var ajustes = efecto.main;
        ajustes.playOnAwake = false;
        ajustes.startColor = color;
        ajustes.startSpeed = 0.25f;
        ajustes.startSize = 0.025f;
        ajustes.startLifetime = 0.5f;
        efecto.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return efecto;
    }
}
