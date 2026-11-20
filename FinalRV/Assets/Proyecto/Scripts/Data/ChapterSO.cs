/*
 * DESCRIPCIÓN: Reúne toda la información de un capítulo del juego (título, historia 
 * del cómic, ciencia real, pistas, tiempo límite y receta asociada) en un solo paquete 
 * reutilizable para cambiar de nivel fácilmente.
 */
using UnityEngine;

[CreateAssetMenu(fileName = "Chapter_", menuName = "Protocolo Origen/Chapter")]
public class ChapterSO : ScriptableObject
{
    public string titulo;
    public string ficha; // cómic, año, editorial
    [TextArea(2, 6)] public string origen;
    [TextArea(2, 6)] public string cienciaReal;
    public string[] pistas;
    public float tiempoSegundos = 210f; // 3:30
    public RecipeSO receta;
}