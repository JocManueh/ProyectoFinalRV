using TMPro;
using UnityEngine;

public class RelojMuneca : MonoBehaviour
{
    [SerializeField] TMP_Text texto;

    void Update()
    {
        if (texto == null) return;

        var timer = TimerManager.Instance;
        var score = ScoreSystem.Instance;

        if (timer == null)
        {
            texto.text = "--:--";
            return;
        }

        string t = timer.GlobalCorriendo || timer.GlobalRestante > 0f
            ? TimerManager.Formato(timer.GlobalRestante)
            : "--:--";

        int puntos = score != null ? score.TotalScore : 0;
        texto.text = $"COSECHA {t}\n{puntos} pts";
    }
}
