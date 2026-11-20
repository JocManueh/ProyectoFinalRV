/*
 * DESCRIPCIÓN: Funciona como una ficha técnica o etiqueta digital para cada sustancia 
 * del laboratorio. Guarda su nombre, símbolo químico, color y si requiere prepararse 
 * antes de usarse, permitiendo crear nuevos químicos sin modificar código.
 */
using UnityEngine;

public enum ChemicalType
{
    Elemento,
    Compuesto,
    BiologicoORadiactivo
}

[CreateAssetMenu(fileName = "Chemical_", menuName = "Protocolo Origen/Chemical")]
public class ChemicalSO : ScriptableObject
{
    public string id;
    public string displayName;
    public string symbol;
    public ChemicalType type;
    [TextArea] public string hazardNote;
    public Color color = Color.white;
    [Tooltip("Si es true, la sustancia no se puede usar hasta procesarla (por ejemplo, extraer el ADN). Error E2.")]
    public bool requiresProcessing;
}