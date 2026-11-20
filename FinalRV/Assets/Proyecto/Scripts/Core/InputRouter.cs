/*
 * DESCRIPCIÓN: Actúa como un traductor universal que escucha los botones especiales 
 * (pausa, linterna, reiniciar y saltar capítulo) tanto desde los mandos de Meta Quest 
 * como desde el teclado, enviando avisos limpios al resto del juego.
 */
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }

    [Header("Acciones del mapa Lab (LabInputActions)")]
    [SerializeField] InputActionReference pausa;
    [SerializeField] InputActionReference reiniciarCap;
    [SerializeField] InputActionReference saltarCap;
    [SerializeField] InputActionReference linternaToggle;

    public event Action OnPausa;
    public event Action OnReiniciarCap;
    public event Action OnSaltarCap;
    public event Action OnLinternaToggle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        Conectar(pausa, AlPausa);
        Conectar(reiniciarCap, AlReiniciarCap);
        Conectar(saltarCap, AlSaltarCap);
        Conectar(linternaToggle, AlLinterna);
    }

    void OnDisable()
    {
        Desconectar(pausa, AlPausa);
        Desconectar(reiniciarCap, AlReiniciarCap);
        Desconectar(saltarCap, AlSaltarCap);
        Desconectar(linternaToggle, AlLinterna);
    }

    // Enable() aquí garantiza que la acción funcione también en las gafas.
    static void Conectar(InputActionReference r, Action<InputAction.CallbackContext> cb)
    {
        if (r == null) return;
        r.action.Enable();
        r.action.performed += cb;
    }

    static void Desconectar(InputActionReference r, Action<InputAction.CallbackContext> cb)
    {
        if (r == null) return;
        r.action.performed -= cb;
    }

    void AlPausa(InputAction.CallbackContext c) => OnPausa?.Invoke();
    void AlReiniciarCap(InputAction.CallbackContext c) => OnReiniciarCap?.Invoke();
    void AlSaltarCap(InputAction.CallbackContext c) => OnSaltarCap?.Invoke();
    void AlLinterna(InputAction.CallbackContext c) => OnLinternaToggle?.Invoke();
}