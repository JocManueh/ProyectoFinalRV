/*
 * Script de editor: crea todos los ScriptableObjects del Capítulo 1 (Spider-Man)
 * en Assets/_Project/ScriptableObjects/Cap1/
 * Ejecutar desde Tools > Crear Datos Spider
 */
using UnityEditor;
using UnityEngine;

public static class CrearDatosSpider
{
    const string RUTA = "Assets/_Project/ScriptableObjects/Cap1/";

    [MenuItem("Tools/Crear Datos Spider (Cap1)")]
    public static void Execute()
    {
        System.IO.Directory.CreateDirectory(RUTA);

        // ── CHEMICALS ────────────────────────────────────────────────────────

        var salina = Crear<ChemicalSO>("Chemical_Salina");
        salina.id          = "salina";
        salina.displayName = "Solución Salina";
        salina.symbol      = "NaCl(aq)";
        salina.type        = ChemicalType.Compuesto;
        salina.hazardNote  = "Isotónica. Sin riesgo a concentraciones estándar.";
        salina.color       = new Color(0.85f, 0.95f, 1f);
        salina.requiresProcessing = false;

        var adn = Crear<ChemicalSO>("Chemical_ADN_Arana");
        adn.id          = "adn_arana";
        adn.displayName = "ADN de Araña";
        adn.symbol      = "DNA-Ar";
        adn.type        = ChemicalType.BiologicoORadiactivo;
        adn.hazardNote  = "Material biológico. Requiere extracción previa. Error E2 si se usa sin procesar.";
        adn.color       = new Color(0.2f, 0.8f, 0.3f);
        adn.requiresProcessing = true;

        var tritio = Crear<ChemicalSO>("Chemical_Tritio");
        tritio.id          = "tritio";
        tritio.displayName = "Tritio";
        tritio.symbol      = "³H";
        tritio.type        = ChemicalType.BiologicoORadiactivo;
        tritio.hazardNote  = "Isótopo radiactivo del hidrógeno. Manipular solo con pinzas blindadas. Error E7 si contacto directo.";
        tritio.color       = new Color(0.4f, 1f, 0.6f);
        tritio.requiresProcessing = false;

        var mercurio = Crear<ChemicalSO>("Chemical_Mercurio");
        mercurio.id          = "mercurio";
        mercurio.displayName = "Mercurio";
        mercurio.symbol      = "Hg";
        mercurio.type        = ChemicalType.Elemento;
        mercurio.hazardNote  = "Metal líquido tóxico. No mezclar con ácidos. Clasificar como Elemento.";
        mercurio.color       = new Color(0.75f, 0.75f, 0.8f);
        mercurio.requiresProcessing = false;

        var cloro = Crear<ChemicalSO>("Chemical_Cloro");
        cloro.id          = "cloro";
        cloro.displayName = "Cloro";
        cloro.symbol      = "Cl₂";
        cloro.type        = ChemicalType.Elemento;
        cloro.hazardNote  = "Gas halógeno oxidante. Clasificar como Elemento.";
        cloro.color       = new Color(0.9f, 1f, 0.5f);
        cloro.requiresProcessing = false;

        var cafeina = Crear<ChemicalSO>("Chemical_Cafeina");
        cafeina.id          = "cafeina";
        cafeina.displayName = "Cafeína";
        cafeina.symbol      = "C₈H₁₀N₄O₂";
        cafeina.type        = ChemicalType.Compuesto;
        cafeina.hazardNote  = "Alcaloide estimulante. Clasificar como Compuesto.";
        cafeina.color       = new Color(0.9f, 0.8f, 0.5f);
        cafeina.requiresProcessing = false;

        // ── RECIPE ───────────────────────────────────────────────────────────

        var receta = Crear<RecipeSO>("Recipe_Spider");
        receta.displayName = "Suero Arácnido";
        receta.steps = new System.Collections.Generic.List<RecipeStep>
        {
            new RecipeStep { chemical = salina,  ampollas = 2 },
            new RecipeStep { chemical = adn,     ampollas = 1 },
            new RecipeStep { chemical = tritio,  ampollas = 1 },
        };
        receta.temperaturaRango = new Vector2(35f, 40f);
        receta.presionRango     = new Vector2(1f,  1.5f);
        receta.voltajeRango     = new Vector2(40f, 60f);
        receta.dosisRango       = new Vector2(40f, 60f);
        receta.segundosEstable  = 5f;

        // ── CHAPTER ──────────────────────────────────────────────────────────

        var cap = Crear<ChapterSO>("Chapter_Spider");
        cap.titulo        = "Capítulo 1 · Sentido Arácnido";
        cap.ficha         = "Amazing Fantasy #15 · 1962 · Marvel Comics";
        cap.origen        = "Peter Parker, estudiante de secundaria, es picado por una araña radiactiva durante una visita escolar. La radiación transfiere parte del ADN arácnido a su genoma, otorgándole fuerza proporcional, adherencia y un sexto sentido de peligro.";
        cap.cienciaReal   = "La transferencia horizontal de genes (THG) es real en bacterias y algunos invertebrados. En mamíferos, retrovirus endógenos han insertado fragmentos de ADN viral en el genoma humano a lo largo de millones de años. El tritio (³H) es un isótopo radiactivo del hidrógeno usado como trazador en biología molecular.";
        cap.pistas        = new string[]
        {
            "ADN de araña",
            "Tritio como vector radiactivo",
            "Temperatura de síntesis: 35–40 °C"
        };
        cap.tiempoSegundos = 210f; // 3:30
        cap.receta         = receta;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CrearDatosSpider] ✅ Todos los assets creados en {RUTA}");
    }

    static T Crear<T>(string nombre) where T : ScriptableObject
    {
        string ruta = RUTA + nombre + ".asset";

        // Si ya existe, lo carga y lo sobreescribe
        var existente = AssetDatabase.LoadAssetAtPath<T>(ruta);
        if (existente != null)
        {
            Debug.Log($"[CrearDatosSpider] Actualizando existente: {nombre}");
            return existente;
        }

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, ruta);
        return asset;
    }
}
