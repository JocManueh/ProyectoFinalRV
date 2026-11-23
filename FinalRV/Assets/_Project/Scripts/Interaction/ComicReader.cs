/*
 * DESCRIPCIÓN: Socket que acepta el cómic (capa Comic). Al insertarlo llama a
 * ChapterManager.StartChapter() y actualiza la TablaHeroes con los datos del capítulo.
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ComicReader : MonoBehaviour
{
    [SerializeField] ChapterSO capitulo;
    [SerializeField] TablaHeroes tabla;
    [SerializeField] AudioSource audioSource;

    XRSocketInteractor socket;
    bool iniciado;

    void Awake() => socket = GetComponent<XRSocketInteractor>();

    void OnEnable()  => socket.selectEntered.AddListener(AlInsertar);
    void OnDisable() => socket.selectEntered.RemoveListener(AlInsertar);

    void AlInsertar(SelectEnterEventArgs args)
    {
        if (iniciado) return;
        iniciado = true;

        Debug.Log("[ComicReader] Cómic insertado → iniciando capítulo");

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(880f, 0.15f));
        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Suave();

        if (ChapterManager.Instance != null && capitulo != null)
            ChapterManager.Instance.StartChapter(capitulo);

        if (tabla != null && capitulo != null)
            tabla.CargarCapitulo(capitulo);

        if (NOVA.Instance != null)
            NOVA.Instance.Say($"Capítulo cargado: {capitulo?.titulo}. Lee la tabla de héroes para obtener pistas.");
    }
}
