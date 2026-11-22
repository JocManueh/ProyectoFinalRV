using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class PanelUI : MonoBehaviour
{
    [SerializeField] TMP_Text titulo;
    [SerializeField] TMP_Text cuerpo;

    void Awake()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    public void SetTexto(string t, string c)
    {
        if (titulo != null) titulo.text = t;
        if (cuerpo != null) cuerpo.text = c;
    }

    public void Probar() => SetTexto("Panel de prueba", "El rayo funciona: botón presionado.");
}
