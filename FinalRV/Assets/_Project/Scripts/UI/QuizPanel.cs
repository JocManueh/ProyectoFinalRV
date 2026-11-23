/*
 * DESCRIPCIÓN: Mini-quiz de 3 opciones en un PanelUI. Muestra la pregunta y 3 botones.
 * Respuesta correcta = puntos; incorrecta = E3 leve. Al terminar avisa al Cap1Manager.
 */
using System;
using TMPro;
using UnityEngine;

[Serializable]
public class PreguntaQuiz
{
    [TextArea] public string pregunta;
    public string[] opciones = new string[3];
    public int indiceCorrecta; // 0, 1 o 2
    public int puntosCorrecta = 15;
}

public class QuizPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text txtPregunta;
    [SerializeField] BotonLab[] botonesOpciones; // exactamente 3
    [SerializeField] TMP_Text[] txtOpciones;     // texto de cada botón
    [SerializeField] TMP_Text txtFeedback;

    [Header("Preguntas")]
    [SerializeField] PreguntaQuiz[] preguntas;

    public event Action OnQuizCompletado;
    public bool Completado { get; private set; }

    int preguntaActual;

    void Start() => MostrarPregunta(0);

    void OnEnable()
    {
        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            int idx = i;
            if (botonesOpciones[idx] != null)
                botonesOpciones[idx].OnPressed.AddListener(() => Responder(idx));
        }
    }

    void OnDisable()
    {
        foreach (var b in botonesOpciones)
            if (b != null) b.OnPressed.RemoveAllListeners();
    }

    void MostrarPregunta(int idx)
    {
        if (idx >= preguntas.Length) { Finalizar(); return; }
        preguntaActual = idx;
        var p = preguntas[idx];

        if (txtPregunta != null) txtPregunta.text = p.pregunta;
        if (txtFeedback != null) txtFeedback.text = "";

        for (int i = 0; i < botonesOpciones.Length && i < p.opciones.Length; i++)
        {
            if (txtOpciones != null && i < txtOpciones.Length && txtOpciones[i] != null)
                txtOpciones[i].text = p.opciones[i];
            if (botonesOpciones[i] != null)
                botonesOpciones[i].gameObject.SetActive(true);
        }
    }

    void Responder(int opcion)
    {
        if (Completado) return;
        var p = preguntas[preguntaActual];
        bool correcto = opcion == p.indiceCorrecta;

        if (correcto)
        {
            Debug.Log($"[Quiz] ✅ Respuesta correcta: {p.opciones[opcion]}");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.Add(p.puntosCorrecta, "Quiz correcto");
            if (txtFeedback != null) txtFeedback.text = "✅ ¡Correcto!";
            if (NOVA.Instance != null) NOVA.Instance.Say("¡Correcto!");
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
        }
        else
        {
            Debug.Log($"[E3] Quiz incorrecto: {p.opciones[opcion]}");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E3_Incorrecto,
                    $"Quiz: {p.opciones[opcion]}");
            if (txtFeedback != null) txtFeedback.text = $"✗ Incorrecto. Era: {p.opciones[p.indiceCorrecta]}";
            if (NOVA.Instance != null)
                NOVA.Instance.Say($"Incorrecto. La respuesta era: {p.opciones[p.indiceCorrecta]}");
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Error();
        }

        // Avanzar a la siguiente pregunta tras 2 s
        StartCoroutine(SiguientePregunta());
    }

    System.Collections.IEnumerator SiguientePregunta()
    {
        yield return new WaitForSecondsRealtime(2f);
        MostrarPregunta(preguntaActual + 1);
    }

    void Finalizar()
    {
        Completado = true;
        Debug.Log("[Quiz] ✅ Quiz completado.");
        if (txtPregunta != null) txtPregunta.text = "Quiz completado.";
        if (NOVA.Instance != null) NOVA.Instance.Say("Quiz completado. Bien hecho.");
        OnQuizCompletado?.Invoke();
    }
}
