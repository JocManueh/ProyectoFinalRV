using UnityEngine;

public class HologramaControles : MonoBehaviour
{
    [SerializeField] Renderer stick;
    [SerializeField] Renderer grip;
    [SerializeField] Renderer gatillo;
    [SerializeField] Color colorBase = new Color(0.25f, 0.45f, 0.6f);
    [SerializeField] Color colorResalte = Color.yellow;
    [SerializeField] float velocidad = 4f;

    Renderer activo;

    void Start() => Pintar(null);

    // Tareas del onboarding: 0 mover, 1 agarrar, 2 socket, 3 botón, 4 palanca
    public void Resaltar(int tarea)
    {
        switch (tarea)
        {
            case 0: activo = stick; break;
            case 3: activo = gatillo; break;
            case 1:
            case 2:
            case 4: activo = grip; break;
            default: activo = null; break;
        }
        Pintar(activo);
    }

    public void Apagar()
    {
        activo = null;
        Pintar(null);
    }

    void Update()
    {
        if (activo == null) return;
        float t = (Mathf.Sin(Time.unscaledTime * velocidad) + 1f) * 0.5f;
        activo.material.color = Color.Lerp(colorBase, colorResalte, t);
    }

    void Pintar(Renderer resaltado)
    {
        if (stick != null) stick.material.color = colorBase;
        if (grip != null) grip.material.color = colorBase;
        if (gatillo != null) gatillo.material.color = colorBase;
    }
}