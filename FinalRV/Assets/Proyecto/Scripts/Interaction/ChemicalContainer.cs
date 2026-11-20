/*
 * DESCRIPCIÓN: Controla el estado interno de cada ampolla o probeta. Guarda qué sustancia 
 * contiene y su cantidad en mililitros, actualizando automáticamente el color del líquido 
 * y el texto flotante para indicar su nombre o avisar si quedó vacía.
 */
using TMPro;
using UnityEngine;

public class ChemicalContainer : MonoBehaviour
{
    public ChemicalSO sustancia;
    public int volumen = 10; // ml. 10 = una ampolla llena
    [SerializeField] Renderer liquidoRenderer;
    [SerializeField] TMP_Text etiqueta;
    [SerializeField] Color colorVacio = Color.gray;

    public bool EstaVacia => sustancia == null || volumen <= 0;

    void Start() => Refrescar();

    public void Refrescar()
    {
        if (liquidoRenderer != null)
            liquidoRenderer.material.color = EstaVacia ? colorVacio : sustancia.color;

        if (etiqueta != null)
            etiqueta.text = EstaVacia ? "VACÍO" : $"{sustancia.displayName}\n{volumen} ml";
    }

    public void Vaciar()
    {
        volumen = 0;
        Refrescar();
    }
}