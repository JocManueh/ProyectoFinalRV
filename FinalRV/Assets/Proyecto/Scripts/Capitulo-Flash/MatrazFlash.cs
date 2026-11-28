using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Valida las dosis y actualiza el nivel, el color y la reacción visual del matraz.</summary>
[RequireComponent(typeof(XRSocketInteractor))]
public class MatrazFlash : MonoBehaviour
{
    [SerializeField] RecipeSO receta;
    [SerializeField] ChemicalSO cargaDeRayo;
    [SerializeField] TMP_Text mensaje;

    [Header("Llenado visual")]
    [SerializeField] Transform liquido;
    [SerializeField] GameObject efectoFinal;
    [SerializeField] Color colorFinal = new Color(0.08f, 0.88f, 0.82f);
    [SerializeField, Min(0.01f)] float segundosPorDosis = 0.35f;

    static readonly int ColorBase = Shader.PropertyToID("_BaseColor");
    static readonly int ColorClasico = Shader.PropertyToID("_Color");

    XRSocketInteractor socket;
    Renderer[] superficies;
    MaterialPropertyBlock propiedades;
    Vector3 posicionLlena;
    Vector3 escalaLlena;
    float fondoLocal;
    float altura;
    int paso;
    int dosisDelPaso;
    int dosisAgregadas;
    int dosisTotales;

    public event System.Action<FlashAudioController.Aviso> ErrorDosis;
    public event System.Action CargaVertida;

    public bool Completa => dosisTotales > 0 && dosisAgregadas == dosisTotales;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        PrepararLiquido();

        if (receta == null) return;

        foreach (var ingrediente in receta.steps)
            dosisTotales += ingrediente.ampollas;
    }

    void OnEnable() => socket.selectEntered.AddListener(Verter);
    void OnDisable() => socket.selectEntered.RemoveListener(Verter);

    void Start() => Mostrar($"Mezcla 0/{dosisTotales}");

    void Verter(SelectEnterEventArgs evento)
    {
        if (dosisTotales == 0)
        {
            Mostrar("Asigna la receta del matraz en el Inspector.");
            return;
        }

        var ampolla = evento.interactableObject.transform.GetComponent<ChemicalContainer>();
        if (ampolla == null) return;

        if (ampolla.EstaVacia)
        {
            Mostrar("E1: ampolla vacía.");
            ErrorDosis?.Invoke(FlashAudioController.Aviso.Vacia);
            return;
        }

        int indice = receta.steps.FindIndex(item => item.chemical == ampolla.sustancia);
        if (indice < 0)
        {
            Mostrar("E3: ese reactivo no pertenece a la receta.");
            ErrorDosis?.Invoke(FlashAudioController.Aviso.Reactivo);
            return;
        }

        // En Recipe_Flash, los pasos 0, 1 y 2 son agua, NaCl y KCl.
        if (ampolla.sustancia == cargaDeRayo && paso < 3)
        {
            Mostrar("E3: añade primero las dos sales.");
            ErrorDosis?.Invoke(FlashAudioController.Aviso.CargaAnticipada);
            return;
        }

        if (indice < paso || ampolla.volumen != 10)
        {
            Mostrar("E4: cantidad incorrecta.");
            ErrorDosis?.Invoke(FlashAudioController.Aviso.Cantidad);
            return;
        }

        if (indice != paso)
        {
            Mostrar($"E5: ahora corresponde {receta.steps[paso].chemical.displayName}.");
            ErrorDosis?.Invoke(FlashAudioController.Aviso.Orden);
            return;
        }

        ampolla.Vaciar();
        if (ampolla.sustancia == cargaDeRayo) CargaVertida?.Invoke();
        dosisAgregadas++;
        dosisDelPaso++;
        bool esCarga = ampolla.sustancia == cargaDeRayo;
        Pintar(esCarga ? colorFinal : ampolla.sustancia.color);
        if (esCarga && efectoFinal != null) efectoFinal.SetActive(true);

        if (dosisDelPaso == receta.steps[paso].ampollas)
        {
            paso++;
            dosisDelPaso = 0;
        }

        Mostrar(Completa ? "Mezcla completa." : $"Mezcla {dosisAgregadas}/{dosisTotales}");
    }

    void PrepararLiquido()
    {
        if (efectoFinal != null) efectoFinal.SetActive(false);
        if (liquido == null) return;

        posicionLlena = liquido.localPosition;
        escalaLlena = liquido.localScale;
        escalaLlena.y = 1f;
        superficies = liquido.GetComponentsInChildren<Renderer>(true);
        propiedades = new MaterialPropertyBlock();

        float minimo = float.PositiveInfinity;
        foreach (Renderer superficie in superficies)
        {
            Bounds limites = superficie.localBounds;
            Matrix4x4 aLiquido = liquido.worldToLocalMatrix * superficie.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                var esquina = new Vector3(
                    (i & 1) == 0 ? limites.min.x : limites.max.x,
                    (i & 2) == 0 ? limites.min.y : limites.max.y,
                    (i & 4) == 0 ? limites.min.z : limites.max.z);
                minimo = Mathf.Min(minimo, aLiquido.MultiplyPoint3x4(esquina).y);
            }
        }

        fondoLocal = float.IsPositiveInfinity(minimo) ? 0f : minimo;
        AplicarAltura(0f);
    }

    void Update()
    {
        if (liquido == null || dosisTotales == 0) return;

        float objetivo = (float)dosisAgregadas / dosisTotales;
        if (altura == objetivo) return;

        float velocidad = 1f / (dosisTotales * segundosPorDosis);
        AplicarAltura(Mathf.MoveTowards(altura, objetivo, velocidad * Time.deltaTime));
    }

    void AplicarAltura(float nuevaAltura)
    {
        altura = nuevaAltura;
        liquido.localScale = new Vector3(escalaLlena.x, altura, escalaLlena.z);
        // Compensa el pivote para que el fondo del líquido se mantenga en su sitio.
        liquido.localPosition = posicionLlena +
            liquido.localRotation * (Vector3.up * (fondoLocal * (1f - altura)));
        foreach (Renderer superficie in superficies)
            superficie.enabled = altura > 0f;
    }

    void Pintar(Color color)
    {
        if (superficies == null) return;

        foreach (Renderer superficie in superficies)
        {
            Material material = superficie.sharedMaterial;
            if (material == null) continue;

            int campo = material.HasProperty(ColorBase) ? ColorBase : ColorClasico;
            if (!material.HasProperty(campo)) continue;

            Color tinte = color;
            tinte.a = material.GetColor(campo).a;
            superficie.GetPropertyBlock(propiedades);
            propiedades.SetColor(campo, tinte);
            superficie.SetPropertyBlock(propiedades);
        }
    }

    void Mostrar(string texto)
    {
        if (mensaje != null) mensaje.text = texto;
    }
}