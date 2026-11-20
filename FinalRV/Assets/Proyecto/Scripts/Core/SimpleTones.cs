/*
 * DESCRIPCIÓN: Sintetizador interno que genera pitidos y tonos por código usando 
 * ondas matemáticas. Permite tener retroalimentación sonora inmediata en botones, 
 * errores y alertas sin depender todavía de archivos de audio externos.
 */
using System.Collections.Generic;
using UnityEngine;

public static class SimpleTones
{
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static AudioClip Beep(float frecuencia, float segundos, float volumen = 0.35f)
    {
        string clave = $"{frecuencia}_{segundos}_{volumen}";
        if (cache.TryGetValue(clave, out var existente) && existente != null)
            return existente;

        const int rate = 44100;
        int n = Mathf.CeilToInt(rate * segundos);
        var datos = new float[n];

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float envolvente = 1f - i / (float)n; // baja a 0 para evitar "clics"
            datos[i] = Mathf.Sin(2f * Mathf.PI * frecuencia * t) * volumen * envolvente;
        }

        var clip = AudioClip.Create("tono_" + clave, n, 1, rate, false);
        clip.SetData(datos, 0);
        cache[clave] = clip;
        return clip;
    }
}