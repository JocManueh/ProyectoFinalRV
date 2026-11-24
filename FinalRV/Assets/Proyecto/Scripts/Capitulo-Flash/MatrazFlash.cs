// Lee la receta de Flash y vacía cada ampolla cuando su dosis es aceptada.
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class MatrazFlash : MonoBehaviour
{
    [SerializeField] RecipeSO receta;
    [SerializeField] ChemicalSO cargaDeRayo;
    [SerializeField] TMP_Text mensaje;

    XRSocketInteractor socket;
    int paso;
    int dosisDelPaso;
    int dosisAgregadas;
    int dosisTotales;

    public bool Completa => dosisTotales > 0 && dosisAgregadas == dosisTotales;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();

        if (receta == null) return;

        foreach (var ingrediente in receta.steps)
            dosisTotales += ingrediente.ampollas;
    }

    void OnEnable() => socket.selectEntered.AddListener(Verter);
    void OnDisable() => socket.selectEntered.RemoveListener(Verter);

    void Start() => Mostrar($"Mezcla 0/{dosisTotales}");

    void Verter(SelectEnterEventArgs evento)
    {
        if (dosisTotales == 0)
        {
            Mostrar("Asigna la receta del matraz en el Inspector.");
            return;
        }

        var ampolla = evento.interactableObject.transform.GetComponent<ChemicalContainer>();
        if (ampolla == null) return;

        if (ampolla.EstaVacia)
        {
            Mostrar("E1: ampolla vacía.");
            return;
        }

        int indice = receta.steps.FindIndex(item => item.chemical == ampolla.sustancia);
        if (indice < 0)
        {
            Mostrar("E3: ese reactivo no pertenece a la receta.");
            return;
        }

        // En Recipe_Flash, los pasos 0, 1 y 2 son agua, NaCl y KCl.
        if (ampolla.sustancia == cargaDeRayo && paso < 3)
        {
            Mostrar("E3: añade primero las dos sales.");
            return;
        }

        if (indice < paso || ampolla.volumen != 10)
        {
            Mostrar("E4: cantidad incorrecta.");
            return;
        }

        if (indice != paso)
        {
            Mostrar($"E5: ahora corresponde {receta.steps[paso].chemical.displayName}.");
            return;
        }

        ampolla.Vaciar();
        dosisAgregadas++;
        dosisDelPaso++;

        if (dosisDelPaso == receta.steps[paso].ampollas)
        {
            paso++;
            dosisDelPaso = 0;
        }

        Mostrar(Completa ? "Mezcla completa." : $"Mezcla {dosisAgregadas}/{dosisTotales}");
    }

    void Mostrar(string texto)
    {
        if (mensaje != null) mensaje.text = texto;
    }
}