/*
 * DESCRIPCIÓN: Abre la caja blindada del tritio: anima la tapa y sube los tritios.
 * Se conecta al botón Tritio mediante BotonLab.OnPressed.
 */
using UnityEngine;

public class CajaTritio : MonoBehaviour
{
    [SerializeField] Animator tapa;
    [SerializeField] Animator[] tritios;
    [SerializeField] string estadoTapa = "cubeee";
    [SerializeField] string estadoSube = "Sube tritio";
    [SerializeField] bool abierta;

    public bool Abierta => abierta;

    public void Abrir()
    {
        if (abierta) return;
        abierta = true;

        if (tapa != null) tapa.Play(estadoTapa);
        if (tritios == null) return;
        foreach (var tritio in tritios)
            if (tritio != null) tritio.Play(estadoSube);
    }
}
