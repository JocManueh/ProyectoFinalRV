/*
 * DESCRIPCIÓN: Actúa como el libro de recetas de cada experimento. Define qué sustancias 
 * mezclar, en qué orden y cantidad, además de los rangos ideales de temperatura, 
 * presión, voltaje y dosis que deben mantenerse estables en el reactor.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipeStep
{
    public ChemicalSO chemical;
    public int ampollas = 1; // cuántas ampollas de esa sustancia
}

[CreateAssetMenu(fileName = "Recipe_", menuName = "Protocolo Origen/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string displayName;
    [Tooltip("El ORDEN de esta lista es el orden en que hay que mezclar.")]
    public List<RecipeStep> steps = new List<RecipeStep>();

    [Header("Rangos del reactor (mínimo, máximo)")]
    public Vector2 temperaturaRango = new Vector2(35f, 40f);
    public Vector2 presionRango = new Vector2(1f, 1.5f);
    public Vector2 voltajeRango = new Vector2(40f, 60f);
    public Vector2 dosisRango = new Vector2(40f, 60f);
    public float segundosEstable = 5f;
}