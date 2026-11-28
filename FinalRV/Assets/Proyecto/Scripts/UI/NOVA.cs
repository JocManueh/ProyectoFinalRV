/*
 * DESCRIPCIÓN: Gestiona la voz y los subtítulos de la inteligencia artificial NOVA. 
 * Organiza sus mensajes en una cola ordenada para evitar que se superpongan, 
 * muestra el texto en pantalla y reproduce el clip de voz o un tono de aviso.
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Ordena la voz, los avisos sonoros y subtítulos de NOVA.</summary>
public class NOVA : MonoBehaviour
{
    public static NOVA Instance { get; private set; }

    [SerializeField] TMP_Text subtitulo;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float segundosMinimosPorLinea = 3f;

    struct Linea
    {
        public string texto;
        public AudioClip clip;
        public AudioClip efecto;
    }

    readonly Queue<Linea> cola = new Queue<Linea>();
    Coroutine rutina;
    AudioSource fuenteAmbiente;

    public bool EstaHablando => isActiveAndEnabled && rutina != null;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    // Si no pasas un clip de audio, suena un "blip" y se ven los subtítulos
    public void Say(string texto, AudioClip clip = null) => Say(texto, clip, null);

    public void Say(string texto, AudioClip clip, AudioClip efecto)
    {
        if (!isActiveAndEnabled || (string.IsNullOrEmpty(texto) && clip == null && efecto == null))
            return;

        cola.Enqueue(new Linea { texto = texto ?? "", clip = clip, efecto = efecto });
        if (rutina == null) rutina = StartCoroutine(Reproducir());
    }

    public void IniciarAmbiente(AudioClip clip, float volumen)
    {
        if (audioSource == null || clip == null || !isActiveAndEnabled) return;

        if (fuenteAmbiente == null)
            fuenteAmbiente = audioSource.gameObject.AddComponent<AudioSource>();
        fuenteAmbiente.playOnAwake = false;
        fuenteAmbiente.spatialBlend = audioSource.spatialBlend;
        fuenteAmbiente.rolloffMode = audioSource.rolloffMode;
        fuenteAmbiente.minDistance = audioSource.minDistance;
        fuenteAmbiente.maxDistance = audioSource.maxDistance;
        fuenteAmbiente.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        fuenteAmbiente.dopplerLevel = 0f;
        fuenteAmbiente.loop = true;
        fuenteAmbiente.volume = Mathf.Clamp01(volumen);
        fuenteAmbiente.clip = clip;
        fuenteAmbiente.Play();
    }

    public void DetenerAmbiente()
    {
        if (fuenteAmbiente != null) fuenteAmbiente.Stop();
    }

    void OnDisable()
    {
        Limpiar();
        DetenerAmbiente();
    }

    public void Limpiar()
    {
        cola.Clear();
        if (rutina != null)
        {
            StopCoroutine(rutina);
            if (audioSource != null) audioSource.Stop();
        }
        rutina = null;
        if (subtitulo != null) subtitulo.text = "";
    }

    IEnumerator Reproducir()
    {
        while (cola.Count > 0)
        {
            Linea l = cola.Dequeue();
            if (l.efecto != null && audioSource != null)
            {
                if (subtitulo != null) subtitulo.text = "";
                audioSource.PlayOneShot(l.efecto);
                yield return new WaitForSecondsRealtime(l.efecto.length);
            }
            if (string.IsNullOrEmpty(l.texto) && l.clip == null)
            {
                yield return null;
                continue;
            }
            if (subtitulo != null) subtitulo.text = "NOVA: " + l.texto;

            float duracion = Mathf.Max(segundosMinimosPorLinea, l.texto.Length * 0.06f);
            if (audioSource != null)
            {
                if (l.clip != null)
                {
                    audioSource.PlayOneShot(l.clip);
                    duracion = Mathf.Max(segundosMinimosPorLinea, l.clip.length);
                }
                else
                {
                    audioSource.PlayOneShot(SimpleTones.Beep(660f, 0.08f));
                }
            }

            yield return new WaitForSecondsRealtime(duracion);
        }

        if (subtitulo != null) subtitulo.text = "";
        rutina = null;
    }
}