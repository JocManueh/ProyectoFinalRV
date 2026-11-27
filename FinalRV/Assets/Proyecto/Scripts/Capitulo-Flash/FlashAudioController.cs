using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Reproduce efectos y voz espacial de NOVA, con subtítulos y reloj de fondo para Flash.</summary>
public class FlashAudioController : MonoBehaviour
{
    public enum Aviso
    {
        Bienvenida, Ingredientes, Tesla, Carga, Matraz, Estabilizar, Pico, Confirmar, Exito,
        Bandeja, Compuestos, Condensador, CargaInterrumpida, Vacia, Reactivo,
        CargaAnticipada, Cantidad, Orden, PicoFallido, UnMinuto, TiempoAgotado
    }

    static readonly string[] Subtitulos =
    {
        "Bienvenido, doctor. Necesitamos un suero de velocidad para superar a los raptores de Lane Tat. Lee el cómic para descubrir la fórmula.",
        "Ya tienes la receta. Elige las cinco ampollas de compuesto y déjalas en su bandeja.",
        "Compuestos listos. Encaja el condensador en Tesla y pulsa Cargar.",
        "Carga lista. Coloca la ampolla en la bandeja de Carga.",
        "Matraz desbloqueado. Retira las ampollas y añádelas una por una, en el orden de la receta.",
        "Mezcla completa. Activa la palanca y mantenla arriba cinco segundos.",
        "¡Pulsa Emergencia! Tienes ocho segundos.",
        "Pico controlado. Pulsa Confirmar para terminar.",
        "Suero de velocidad listo. Misión cumplida, doctor.",
        "Esa ampolla no va aquí. Revisa la receta.",
        "Completa la bandeja de Compuesto antes de usar Tesla.",
        "Coloca el condensador en la bobina.",
        "Carga interrumpida. Vuelve a colocar el condensador y pulsa Cargar.",
        "Esa ampolla está vacía.",
        "Ese reactivo no pertenece a la fórmula.",
        "La carga va al final. Añade primero los demás ingredientes.",
        "Cantidad incorrecta. Revisa las dosis de la receta.",
        "Esa dosis va después. Mira qué ingrediente pide el matraz.",
        "Se acabó la ventana. Baja y vuelve a subir la palanca.",
        "Doctor, queda un minuto para terminar.",
        "Tiempo agotado, doctor. La misión queda incompleta."
    };

    [SerializeField] AudioSource fuenteNova;
    [SerializeField] TMP_Text subtitulo;
    [SerializeField] AudioClip pasoConfirmado;
    [SerializeField] AudioClip efectoError;
    [SerializeField] AudioClip relojNormal;
    [SerializeField, Range(0f, 1f)] float volumenReloj = 0.18f;
    [SerializeField] AudioClip[] voces;

    readonly Queue<Aviso> pendientes = new Queue<Aviso>();
    readonly Dictionary<Aviso, float> ultimoError = new Dictionary<Aviso, float>();
    AudioSource fuenteReloj;
    Coroutine reproduccion;
    Aviso? actual;
    bool activo;

    public void Iniciar()
    {
        if (activo || fuenteNova == null) return;
        activo = true;
        if (subtitulo != null) subtitulo.text = "";
        if (relojNormal != null)
        {
            if (fuenteReloj == null)
                fuenteReloj = fuenteNova.gameObject.AddComponent<AudioSource>();
            fuenteReloj.playOnAwake = false;
            fuenteReloj.spatialBlend = fuenteNova.spatialBlend;
            fuenteReloj.rolloffMode = fuenteNova.rolloffMode;
            fuenteReloj.minDistance = fuenteNova.minDistance;
            fuenteReloj.maxDistance = fuenteNova.maxDistance;
            fuenteReloj.outputAudioMixerGroup = fuenteNova.outputAudioMixerGroup;
            fuenteReloj.dopplerLevel = 0f;
            fuenteReloj.loop = true;
            fuenteReloj.volume = volumenReloj;
            fuenteReloj.clip = relojNormal;
            fuenteReloj.Play();
        }
        Avisar(Aviso.Bienvenida);
    }

    public void Avisar(Aviso aviso)
    {
        if (!activo) return;
        if (aviso == Aviso.Pico || aviso == Aviso.PicoFallido ||
            (aviso == Aviso.Confirmar && actual == Aviso.Pico))
            LimpiarLocucion();

        if (actual == aviso || pendientes.Contains(aviso)) return;
        if (EsError(aviso))
        {
            if (ultimoError.TryGetValue(aviso, out float ultimo) &&
                Time.unscaledTime - ultimo < 1.2f) return;
            ultimoError[aviso] = Time.unscaledTime;
        }
        pendientes.Enqueue(aviso);
        if (reproduccion == null) reproduccion = StartCoroutine(Reproducir());
    }

    public void Terminar(bool exito)
    {
        if (!activo) return;
        if (fuenteReloj != null) fuenteReloj.Stop();
        LimpiarLocucion();
        Avisar(exito ? Aviso.Exito : Aviso.TiempoAgotado);
        activo = false;
    }

    static bool EsError(Aviso aviso) =>
        (aviso >= Aviso.Bandeja && aviso <= Aviso.PicoFallido) || aviso == Aviso.TiempoAgotado;

    IEnumerator Reproducir()
    {
        while (pendientes.Count > 0)
        {
            Aviso aviso = pendientes.Dequeue();
            actual = aviso;
            bool paso = aviso >= Aviso.Ingredientes && aviso <= Aviso.Exito;
            AudioClip efecto = EsError(aviso) ? efectoError : paso ? pasoConfirmado : null;
            if (efecto != null)
            {
                fuenteNova.PlayOneShot(efecto);
                yield return new WaitForSecondsRealtime(efecto.length);
            }

            int indice = (int)aviso;
            AudioClip voz = voces != null && indice < voces.Length ? voces[indice] : null;
            if (subtitulo != null) subtitulo.text = "NOVA · " + Subtitulos[indice];
            if (voz != null) fuenteNova.PlayOneShot(voz);
            yield return new WaitForSecondsRealtime(voz != null ? Mathf.Max(1.5f, voz.length) : 3f);
            if (subtitulo != null) subtitulo.text = "";
        }
        actual = null;
        reproduccion = null;
    }

    void LimpiarLocucion()
    {
        pendientes.Clear();
        if (reproduccion != null) StopCoroutine(reproduccion);
        reproduccion = null;
        actual = null;
        if (fuenteNova != null) fuenteNova.Stop();
        if (subtitulo != null) subtitulo.text = "";
    }

    void OnDisable()
    {
        activo = false;
        LimpiarLocucion();
        if (fuenteReloj != null) fuenteReloj.Stop();
    }
}

