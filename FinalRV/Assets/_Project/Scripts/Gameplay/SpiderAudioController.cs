using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Sincroniza la voz espacial de NOVA, los efectos y los subtítulos de Spider-Man.</summary>
public class SpiderAudioController : MonoBehaviour
{
    [Serializable]
    struct Linea
    {
        public AudioClip voz;
        public string subtitulo;
    }

    [SerializeField] AudioSource fuente;
    [SerializeField] TMP_Text subtitulo;
    [SerializeField] CamaraBlindada camara;
    [SerializeField] RecipeValidator analizador;
    [SerializeField] AudioClip reloj;
    [SerializeField] AudioClip error;
    [SerializeField] AudioClip confirmacion;
    [SerializeField] Linea[] lineas;

    readonly Queue<(int indice, AudioClip efecto, string texto)> cola =
        new Queue<(int, AudioClip, string)>();
    readonly Dictionary<int, float> ultimoError = new Dictionary<int, float>();
    static readonly int[] AvisosPorPaso = { 11, 12, 13, 14, 16, 17, 18 };
    AudioSource fondo;
    ScoreSystem score;
    Coroutine rutina;
    int locucionActual = -1;
    int erroresE3;
    int erroresE4E5;
    bool activo;

    public bool Reproduciendo => rutina != null;

    public void Iniciar()
    {
        if (activo || fuente == null) return;
        activo = true;
        score = ScoreSystem.Instance;
        if (score != null) score.OnErrorRegistrado += AlError;
        if (camara != null) camara.OnAbierta?.AddListener(AlAbrirCamara);
        if (analizador != null) analizador.OnValidado += AlValidar;
        if (reloj != null)
        {
            fondo = fuente.gameObject.AddComponent<AudioSource>();
            fondo.spatialBlend = fuente.spatialBlend;
            fondo.rolloffMode = fuente.rolloffMode;
            fondo.minDistance = fuente.minDistance;
            fondo.maxDistance = fuente.maxDistance;
            fondo.outputAudioMixerGroup = fuente.outputAudioMixerGroup;
            fondo.playOnAwake = false;
            fondo.dopplerLevel = 0f;
            fondo.volume = 0.18f;
            fondo.loop = true;
            fondo.clip = reloj;
            fondo.Play();
        }
        Encolar(10);
    }

    public void PasoCompletado(int paso) => Encolar(AvisosPorPaso[paso], confirmacion);
    void AlAbrirCamara() => Encolar(15);

    public void UnMinuto() => Encolar(9);

    public void FaltaPaso(string texto) => Encolar(-2, error, texto);

    public void Cerrar(bool exito)
    {
        if (!activo) return;
        Limpiar();
        if (fondo != null) fondo.Stop();
        Desuscribir();
        Encolar(exito ? 19 : 8, exito ? confirmacion : error);
        activo = false;
    }

    void AlValidar(ResultadoValidacion resultado)
    {
        if (resultado == ResultadoValidacion.Fallido) Encolar(20, error);
    }

    void AlError(ErrorLog entrada)
    {
        int aviso;
        switch (entrada.codigo)
        {
            case nameof(ErrorCode.E1_Vacio): aviso = 0; break;
            case nameof(ErrorCode.E2_SinProcesar): aviso = 1; break;
            case nameof(ErrorCode.E3_Incorrecto): aviso = entrada.detalle.Contains(" en bandeja ") ? 3 : 2; break;
            case nameof(ErrorCode.E4_Cantidad): aviso = entrada.detalle == "Faltan ampollas" ? 5 : 4; break;
            case nameof(ErrorCode.E5_Orden): aviso = 6; break;
            case nameof(ErrorCode.E6_FueraDeRango): aviso = entrada.detalle == "Reactor inestable al validar" ? 24 : 7; break;
            case nameof(ErrorCode.E7_Radiacion): aviso = 21; break;
            default: return;
        }

        if (!ultimoError.TryGetValue(aviso, out float anterior) || Time.unscaledTime - anterior >= 1.2f)
        {
            ultimoError[aviso] = Time.unscaledTime;
            Encolar(aviso, error);
        }
        if ((aviso == 2 || aviso == 3) && ++erroresE3 == 3) Encolar(22);
        if ((aviso == 4 || aviso == 5 || aviso == 6) && ++erroresE4E5 == 3) Encolar(23);
    }

    void Encolar(int indice, AudioClip efecto = null, string texto = null)
    {
        if (!activo || indice == locucionActual) return;
        foreach (var pendiente in cola)
            if (pendiente.indice == indice) return;
        cola.Enqueue((indice, efecto, texto));
        if (rutina == null) rutina = StartCoroutine(Reproducir());
    }

    IEnumerator Reproducir()
    {
        while (cola.Count > 0)
        {
            var aviso = cola.Dequeue();
            locucionActual = aviso.indice;
            if (aviso.efecto != null)
            {
                fuente.PlayOneShot(aviso.efecto);
                yield return new WaitForSecondsRealtime(aviso.efecto.length);
            }
            Linea linea = aviso.indice >= 0 && aviso.indice < lineas.Length ? lineas[aviso.indice] : default;
            if (subtitulo != null) subtitulo.text = "NOVA · " + (aviso.texto ?? linea.subtitulo);
            if (linea.voz != null) fuente.PlayOneShot(linea.voz);
            yield return new WaitForSecondsRealtime(linea.voz != null ? Mathf.Max(1.5f, linea.voz.length) : 3f);
            if (subtitulo != null) subtitulo.text = "";
        }
        locucionActual = -1;
        rutina = null;
    }

    void Desuscribir()
    {
        if (score != null) score.OnErrorRegistrado -= AlError;
        if (camara != null) camara.OnAbierta?.RemoveListener(AlAbrirCamara);
        if (analizador != null) analizador.OnValidado -= AlValidar;
    }

    void Limpiar()
    {
        cola.Clear();
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        locucionActual = -1;
        if (fuente != null) fuente.Stop();
        if (subtitulo != null) subtitulo.text = "";
    }

    void OnDisable()
    {
        activo = false;
        Desuscribir();
        Limpiar();
        if (fondo != null) Destroy(fondo);
    }
}