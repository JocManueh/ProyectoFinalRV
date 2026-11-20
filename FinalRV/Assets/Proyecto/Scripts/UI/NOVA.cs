/*
 * DESCRIPCIÓN: Gestiona la voz y los subtítulos de la inteligencia artificial NOVA. 
 * Organiza sus mensajes en una cola ordenada para evitar que se superpongan, 
 * muestra el texto en pantalla y reproduce el clip de voz o un tono de aviso.
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    }

    readonly Queue<Linea> cola = new Queue<Linea>();
    Coroutine rutina;

    public bool EstaHablando => rutina != null;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    // Si no pasas un clip de audio, suena un "blip" y se ven los subtítulos
    public void Say(string texto, AudioClip clip = null)
    {
        cola.Enqueue(new Linea { texto = texto, clip = clip });
        if (rutina == null) rutina = StartCoroutine(Reproducir());
    }

    public void Limpiar()
    {
        cola.Clear();
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        if (subtitulo != null) subtitulo.text = "";
    }

    IEnumerator Reproducir()
    {
        while (cola.Count > 0)
        {
            Linea l = cola.Dequeue();
            if (subtitulo != null) subtitulo.text = "NOVA: " + l.texto;

            float duracion = Mathf.Max(segundosMinimosPorLinea, l.texto.Length * 0.06f);
            if (audioSource != null)
            {
                if (l.clip != null)
                {
                    audioSource.PlayOneShot(l.clip);
                    duracion = Mathf.Max(duracion, l.clip.length);
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