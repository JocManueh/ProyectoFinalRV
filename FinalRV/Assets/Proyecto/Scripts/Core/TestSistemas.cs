/*
 * DESCRIPCIÓN: Script temporal de verificación que arranca los relojes, escucha las 
 * teclas de prueba y conecta un botón en pantalla para comprobar que el sistema 
 * de puntuación y errores funciona correctamente antes de construir el juego.
 */
using UnityEngine;

public class TestSistemas : MonoBehaviour
{
    float ultimoLog;

    void Start()
    {
        GameManager.Instance.SetState(GameState.Capitulo);
        ScoreSystem.Instance.Add(100, "puntos de prueba");
        TimerManager.Instance.IniciarGlobal();
        TimerManager.Instance.IniciarCapitulo(30f);
        TimerManager.Instance.OnChapterTimeUp += AlAcabarCapitulo;

        if (InputRouter.Instance != null)
        {
            InputRouter.Instance.OnPausa += AlPausa;
            InputRouter.Instance.OnSaltarCap += AlSaltar;
        }
    }

    void OnDestroy()
    {
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnChapterTimeUp -= AlAcabarCapitulo;

        if (InputRouter.Instance != null)
        {
            InputRouter.Instance.OnPausa -= AlPausa;
            InputRouter.Instance.OnSaltarCap -= AlSaltar;
        }
    }

    void Update()
    {
        if (Time.unscaledTime - ultimoLog < 1f) return;
        ultimoLog = Time.unscaledTime;
        var t = TimerManager.Instance;
        Debug.Log($"[Timer] Global {TimerManager.Formato(t.GlobalRestante)} | Capítulo {TimerManager.Formato(t.CapituloRestante)}");
    }

    void AlAcabarCapitulo() => Debug.Log("[Test] ¡Se acabó el tiempo del capítulo!");
    void AlPausa() => Debug.Log("[Input] Pausa (Esc o botón de menú)");
    void AlSaltar() => Debug.Log("[Input] SaltarCap (F1)");

    // Lo llama el botón de la UI
    public void ProbarE1() =>
        ScoreSystem.Instance.RegisterError(ErrorCode.E1_Vacio, "botón de prueba");
}