using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Regenera los tres atlas SDF de Flash conservando las referencias de Unity.</summary>
public static class RegenerarTipografiasFlash
{
    const string Ruta = "Assets/Proyecto/Tipografias/";

    [MenuItem("Tools/Flash/Regenerar tipografías SDF")]
    public static void Ejecutar()
    {
        Regenerar("Roboto-Regular SDF", "roboto/Roboto-Regular.ttf");
        Regenerar("TT Octosquares Trial Compressed DemiBold SDF",
            "tt_octosquares/TT Octosquares Trial Compressed DemiBold.ttf");
        Regenerar("alarm clock SDF", "alarm_clock/alarm clock.ttf");
        AssetDatabase.SaveAssets();
        Debug.Log("FLASH_FONT_REBUILD_OK");
    }

    static void Regenerar(string nombre, string origen)
    {
        var fuente = AssetDatabase.LoadAssetAtPath<Font>(Ruta + origen);
        var destino = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Ruta + nombre + ".asset");
        if (fuente == null || destino == null)
            throw new InvalidOperationException("Falta la fuente o el asset de " + nombre);

        var atlas = destino.atlasTexture;
        var material = destino.material;
        var nuevo = TMP_FontAsset.CreateFontAsset(fuente, 80, 8, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, false);
        string caracteres = string.Concat(Enumerable.Range(32, 95).Select(i => (char)i))
            + "ÁÉÍÓÚÜÑáéíóúüñ¿¡×°·–—";
        nuevo.TryAddCharacters(caracteres, out string faltantes);
        if (!nuevo.HasCharacters("0123456789:"))
            throw new InvalidOperationException("Faltan caracteres del reloj en " + nombre);
        if (!string.IsNullOrEmpty(faltantes))
            Debug.LogWarning(nombre + " no incluye estos glifos en el TTF original: " + faltantes);

        nuevo.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.CopySerialized(nuevo.atlasTexture, atlas);
        EditorUtility.CopySerialized(nuevo.material, material);
        material.mainTexture = atlas;
        EditorUtility.CopySerialized(nuevo, destino);
        var datos = new SerializedObject(destino);
        var texturas = datos.FindProperty("m_AtlasTextures");
        texturas.GetArrayElementAtIndex(0).objectReferenceValue = atlas;
        datos.FindProperty("m_Material").objectReferenceValue = material;
        datos.ApplyModifiedPropertiesWithoutUndo();
        destino.name = nombre;
        atlas.name = nombre + " Atlas";
        material.name = nombre + " Material";
        EditorUtility.SetDirty(atlas);
        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(destino);
    }
}

