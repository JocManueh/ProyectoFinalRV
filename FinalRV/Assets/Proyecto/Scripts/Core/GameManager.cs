/*
 * DESCRIPCIÓN: Es el director general del juego. Controla en qué etapa está el jugador 
 * (tutorial, laboratorio o pantalla final), avisa a los demás sistemas cuando cambia 
 * de fase y se mantiene activo al pasar de una escena a otra.
 */
using System;
using UnityEngine;

public enum GameState
{
    Onboarding,
    Capitulo,
    Resultado
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Onboarding;
    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // sobrevive al cambiar de escena
    }

    public void SetState(GameState nuevo)
    {
        if (nuevo == State) return;
        State = nuevo;
        Debug.Log($"[GameManager] Estado: {State}");
        OnStateChanged?.Invoke(State);
    }
}