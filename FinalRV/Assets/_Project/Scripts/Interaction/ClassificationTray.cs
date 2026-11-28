/*
 * DESCRIPCIÓN: Bandeja de clasificación. Acepta ampollas y verifica si su ChemicalType
 * coincide con el tipo esperado. Correcto = verde; incorrecto = error leve E3 (−10 pts).
 * Cuando todas las bandejas tienen al menos una ampolla correcta, avisa al Cap1Manager.
 */
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ClassificationTray : MonoBehaviour
{
    [SerializeField] ChemicalType tipoEsperado;
    [SerializeField] Renderer piloto;          // esfera indicadora
    [SerializeField] AudioSource audioSource;

    public event Action<ClassificationTray> OnClasificacionCorrecta;
    public bool TieneCorrecta { get; private set; }

    XRSocketInteractor socket;
    Color colorBase;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (piloto != null) colorBase = piloto.material.color;
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(AlInsertar);
        socket.selectExited.AddListener(AlSacar);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(AlInsertar);
        socket.selectExited.RemoveListener(AlSacar);
    }

    void AlInsertar(SelectEnterEventArgs args)
    {
        var cont = args.interactableObject.transform.GetComponent<ChemicalContainer>();
        if (cont == null || cont.sustancia == null)
        {
            Pintar(Color.yellow);
            return;
        }

        if (cont.sustancia.type == tipoEsperado)
        {
            Pintar(Color.green);
            TieneCorrecta = true;
            Debug.Log($"[Clasificación] ✅ {cont.sustancia.displayName} → {tipoEsperado}");
            if (audioSource != null)
                audioSource.PlayOneShot(SimpleTones.Beep(880f, 0.12f));
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
            OnClasificacionCorrecta?.Invoke(this);
        }
        else
        {
            Pintar(Color.red);
            Debug.Log($"[E3] {cont.sustancia.displayName} clasificado incorrectamente en {tipoEsperado}");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E3_Incorrecto,
                    $"{cont.sustancia.displayName} en bandeja {tipoEsperado}");
            if (audioSource != null)
                audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();
            if (NOVA.Instance != null)
                NOVA.Instance.Say($"Clasificación incorrecta. {cont.sustancia.displayName} no es {tipoEsperado}.");
        }
    }

    void AlSacar(SelectExitEventArgs args) => Pintar(colorBase);

    void Pintar(Color c) { if (piloto != null) piloto.material.color = c; }
}
