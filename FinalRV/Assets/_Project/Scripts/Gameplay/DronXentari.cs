/*
 * DESCRIPCIÓN: Dron Xentari — esfera con luz que aparece detrás del jugador
 * al completar la fórmula. Muestra aviso rojo en Canvas world-space y NOVA dice
 * "Sentido arácnido activado". Guarda la fórmula en la Bitácora.
 */
using System.Collections;
using UnityEngine;
using TMPro;

public class DronXentari : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Light luzDron;
    [SerializeField] Color colorAlerta = Color.red;
    [SerializeField] float velocidadOrbit = 30f;

    [Header("Canvas de aviso")]
    [SerializeField] Canvas canvasAviso;
    [SerializeField] TMP_Text txtAviso;

    [Header("Spawn")]
    [SerializeField] float distanciaDetras = 1.5f;
    [SerializeField] float alturaSpawn = 1.5f;

    [Header("Duración del aviso")]
    [SerializeField] float segundosAviso = 5f;

    Transform cam;
    bool activo;

    void Awake()
    {
        gameObject.SetActive(false);
        if (canvasAviso != null) canvasAviso.gameObject.SetActive(false);
    }

    public void Activar(string formula)
    {
        if (activo) return;
        activo = true;

        if (Camera.main != null) cam = Camera.main.transform;

        // Posicionar detrás del jugador
        if (cam != null)
        {
            Vector3 detras = cam.position - cam.forward * distanciaDetras + Vector3.up * alturaSpawn;
            transform.position = detras;
        }

        gameObject.SetActive(true);
        if (luzDron != null) luzDron.color = colorAlerta;

        if (canvasAviso != null)
        {
            canvasAviso.gameObject.SetActive(true);
            if (txtAviso != null) txtAviso.text = "⚠ DRON XENTARI DETECTADO ⚠";
        }

        Debug.Log("[DronXentari] Dron activado.");
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Sentido arácnido activado. Un dron Xentari se aproxima por detrás.");
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();

        if (Bitacora.Instance != null)
            Bitacora.Instance.Registrar($"Poder activado: Sentido Arácnido — {formula}");

        StartCoroutine(OcultarAviso());
    }

    void Update()
    {
        if (!activo || cam == null) return;
        // Orbitar alrededor del jugador
        transform.RotateAround(cam.position, Vector3.up, velocidadOrbit * Time.unscaledDeltaTime);
        // Mirar al jugador
        transform.LookAt(cam.position);
    }

    IEnumerator OcultarAviso()
    {
        yield return new WaitForSecondsRealtime(segundosAviso);
        if (canvasAviso != null) canvasAviso.gameObject.SetActive(false);
    }
}
