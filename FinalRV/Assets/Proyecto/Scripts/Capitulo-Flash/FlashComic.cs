using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>Presenta la historia al seleccionar el cómic y revela la receta de Flash.</summary>
public class FlashComic : MonoBehaviour
{
    const string Introduccion = "Querido doctor, el ejército está siendo derrotado por los veloces raptores de Lane Tat. Necesitamos que los soldados puedan correr más rápido que ellos. ¿Puede crear el suero de la velocidad?";
    const string Historia =
        "Una noche de tormenta, el forense Barry Allen trabajaba en su laboratorio. " +
        "En la mesa quedaron agua destilada, cloruro de sodio y cloruro de potasio. " +
        "Cerca de las sales había un frasco de glucosa. Una descarga de rayo alcanzó el estante " +
        "y bañó a Barry con la mezcla. Al recuperarse, descubrió que podía moverse a una velocidad extraordinaria.";

    [SerializeField] FlashNivel nivel;
    [SerializeField] PanelUI panel;
    [SerializeField] GameObject comic;

    XRSimpleInteractable interactivo;

    void Start()
    {
        if (panel != null) panel.SetTexto("Nivel 2", Introduccion);
        if (comic == null) return;

        interactivo = comic.GetComponent<XRSimpleInteractable>();
        if (interactivo == null) interactivo = comic.AddComponent<XRSimpleInteractable>();
        interactivo.selectEntered.AddListener(AlSeleccionarComic);
    }

    void AlSeleccionarComic(SelectEnterEventArgs evento) => AbrirHistoria();

    public void AbrirHistoria()
    {
        if (panel != null) panel.SetTexto("La historia de Flash", Historia);
        if (nivel != null) nivel.RevelarReceta();
    }

    void OnDestroy()
    {
        if (interactivo != null) interactivo.selectEntered.RemoveListener(AlSeleccionarComic);
    }
}
