/*
 * DESCRIPCIÓN: PanelUI con 4 tarjetas (Ficha, Origen, Ciencia real, Pistas) y
 * 3 palabras clave tocables. Al revelar las 3 se completa "Leer la tabla".
 */
using System;
using TMPro;
using UnityEngine;

public class TablaHeroes : MonoBehaviour
{
    [Header("Tarjetas de texto")]
    [SerializeField] TMP_Text txtFicha;
    [SerializeField] TMP_Text txtOrigen;
    [SerializeField] TMP_Text txtCiencia;
    [SerializeField] TMP_Text txtPistas;   // muestra las pistas ya reveladas

    [Header("Pistas revelables (3)")]
    [SerializeField] PistaRevelable[] pistasRevelables; // exactamente 3

    [Header("Audio")]
    [SerializeField] AudioSource audioSource; // 2D: se oye aunque la tabla esté lejos

    public event Action OnTablaCompletada;
    public bool Completada { get; private set; }

    int reveladas;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // interfaz: volumen constante, sin atenuación por distancia
    }

    void OnEnable()
    {
        // El socket activa la tabla con SetActive(true): aquí suena su aparición.
        if (Application.isPlaying && audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(880f, 0.15f));

        foreach (var p in pistasRevelables)
            if (p != null) p.OnRevelada += AlRevelar;
    }

    void OnDisable()
    {
        foreach (var p in pistasRevelables)
            if (p != null) p.OnRevelada -= AlRevelar;
    }

    public void CargarCapitulo(ChapterSO cap)
    {
        if (cap == null) return;

        if (txtFicha   != null) txtFicha.text   = cap.ficha;
        if (txtOrigen  != null) txtOrigen.text  = cap.origen;
        if (txtCiencia != null) txtCiencia.text = cap.cienciaReal;

        // Configura las pistas revelables con los textos del SO
        for (int i = 0; i < pistasRevelables.Length && i < cap.pistas.Length; i++)
            if (pistasRevelables[i] != null)
                pistasRevelables[i].Configurar("???", cap.pistas[i]);

        ActualizarTextoPistas();
        Debug.Log("[TablaHeroes] Capítulo cargado en la tabla.");
    }

    void AlRevelar(PistaRevelable p)
    {
        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(660f, 0.12f));

        reveladas++;
        ActualizarTextoPistas();

        if (reveladas >= pistasRevelables.Length && !Completada)
        {
            Completada = true;
            Debug.Log("[TablaHeroes] ✅ Todas las pistas reveladas → tabla completada");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("Has leído todas las pistas. Ahora clasifica los reactivos.");
            OnTablaCompletada?.Invoke();
        }
    }

    void ActualizarTextoPistas()
    {
        // txtPistas es opcional (resumen acumulador); cada pista tiene su propio TMP.
        if (txtPistas == null) return;
        var sb = new System.Text.StringBuilder("PISTAS:\n");
        foreach (var p in pistasRevelables)
        {
            if (p == null) continue;
            sb.AppendLine(p.Revelada ? $"✓ {p.TextoReal}" : "???");
        }
        txtPistas.text = sb.ToString();
    }
}
