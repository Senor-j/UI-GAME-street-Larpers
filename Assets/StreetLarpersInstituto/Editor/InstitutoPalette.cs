using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StreetLarpers.Instituto.Editor
{
    /// <summary>
    /// Materiales del instituto (sección H): todos URP/Lit, colores desaturados y smoothness por reglas.
    /// Regenerarlos es idempotente: si el asset existe, se actualizan sus valores.
    /// </summary>
    internal static class InstitutoPalette
    {
        public const string Folder = "Assets/StreetLarpersInstituto/Art/Materials";

        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        // --- Arquitectura ---
        public static Material Pared => Lit("M_Pared", "#C4C3CC", 0.12f);
        public static Material Techo => Lit("M_Techo", "#ABABB0", 0.08f);
        public static Material Viga => Lit("M_Viga", "#B6B6BA", 0.08f);
        public static Material BaldosaBlanca => Lit("M_BaldosaBlanca", "#C9C6BD", 0.55f);
        public static Material BaldosaGris => Lit("M_BaldosaGris", "#6F6C67", 0.45f);
        public static Material Fieltro => Lit("M_Fieltro", "#5B5B5E", 0.05f);
        public static Material Piedra => Lit("M_Piedra", "#6C6660", 0.15f);
        public static Material Cristal => Glass("M_Cristal", "#9FB0B5", 0.18f);

        // --- Mobiliario (los tres acentos: naranja, verde, azul) ---
        public static Material Madera => Lit("M_MaderaArmario", "#5E4835", 0.2f);
        public static Material TableroMesa => Lit("M_TableroMesa", "#8A7458", 0.3f);
        public static Material MetalVerde => Lit("M_MetalVerde", "#3F5546", 0.4f, 0.6f);
        public static Material SillaNaranja => Lit("M_SillaNaranja", "#AE5530", 0.35f);
        public static Material SillaNegra => Lit("M_SillaNegra", "#1F1F22", 0.3f);
        public static Material BancoVerde => Lit("M_BancoVerde", "#55613F", 0.2f);
        public static Material PapeleraAzul => Lit("M_PapeleraAzul", "#2F4C86", 0.35f);
        public static Material PapeleraAmarilla => Lit("M_PapeleraAmarilla", "#A8922E", 0.35f);
        public static Material Carton => Lit("M_Carton", "#86694A", 0.1f);
        public static Material Cinta => Lit("M_Cinta", "#A99A7C", 0.4f);
        public static Material Radiador => Lit("M_Radiador", "#BDBCB6", 0.4f, 0.6f);
        public static Material MetalGris => Lit("M_MetalGris", "#77787A", 0.4f, 0.6f);
        public static Material Pizarra => Lit("M_Pizarra", "#D5D8D4", 0.6f);
        public static Material Extintor => Lit("M_Extintor", "#7A2A22", 0.45f);
        public static Material Mochila => Lit("M_Mochila", "#2D4A78", 0.2f);
        public static Material PlasticoNegro => Lit("M_PlasticoNegro", "#141416", 0.4f);

        // --- Papelería del menú ---
        public static Material Papel => Lit("M_Papel", "#D9D4C6", 0.1f);
        public static Material PostIt => Lit("M_PostIt", "#C9B860", 0.1f);
        public static Material PegatinaSalida => Lit("M_PegatinaSalida", "#2F7A4A", 0.2f);
        public static Material Tinta => Lit("M_Tinta", "#1A1A1C", 0.1f);
        public static Material ChinchetaRoja => Lit("M_ChinchetaRoja", "#8C2E2A", 0.5f);
        public static Material ChinchetaAzul => Lit("M_ChinchetaAzul", "#2E4A7A", 0.5f);
        public static Material ChinchetaAmarilla => Lit("M_ChinchetaAmarilla", "#A8902E", 0.5f);
        public static Material EsferaReloj => Lit("M_EsferaReloj", "#D8D6CE", 0.3f);

        // --- Fuentes de luz (emisivas) ---
        public static Material TuboFluorescente => Lit("M_TuboFluorescente", "#E6F0E8", 0.5f, 0f, new Color(2.2f, 2.35f, 2.2f));
        public static Material SenalSalida => Lit("M_SenalSalida", "#2E9A58", 0.3f, 0f, new Color(0.35f, 1.6f, 0.65f));
        public static Material PantallaTv => Lit("M_PantallaTv", "#0E1A1A", 0.7f, 0f, new Color(0.05f, 0.22f, 0.2f));
        public static Material PantallaProyector => Lit("M_PantallaProyector", "#C9D3E6", 0.1f, 0f, new Color(0.7f, 0.8f, 1.0f));
        public static Material LedRojo => Lit("M_LedRojo", "#5A0E0E", 0.5f, 0f, new Color(2.5f, 0.15f, 0.1f));
        public static Material LuzEmergencia => Lit("M_LuzEmergencia", "#E8E8E2", 0.4f, 0f, new Color(0.6f, 0.6f, 0.58f));

        public static Material Glifo
        {
            get
            {
                const string path = Folder + "/M_Glifo.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null) return material;

                var shader = Shader.Find("StreetLarpers/Glifo");
                if (shader == null)
                {
                    Debug.LogError("[Instituto] No se encuentra el shader 'StreetLarpers/Glifo'.");
                    return null;
                }
                material = new Material(shader);
                Save(material, path);
                return material;
            }
        }

        public static Material Lit(string name, string hex, float smoothness, float metallic = 0f, Color? emission = null)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            string path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = material == null;
            if (isNew) material = new Material(LitShader);

            ColorUtility.TryParseHtmlString(hex, out var color);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);

            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                // La emisión cambia en runtime (parpadeo), así que no se hornea.
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }

            if (isNew) Save(material, path);
            else EditorUtility.SetDirty(material);

            Cache[name] = material;
            return material;
        }

        private static Material Glass(string name, string hex, float alpha)
        {
            var material = Lit(name, hex, 0.9f);
            var color = material.GetColor("_BaseColor");
            color.a = alpha;
            material.SetColor("_BaseColor", color);

            material.SetFloat("_Surface", 1f); // Transparent
            material.SetFloat("_Blend", 0f);   // Alpha
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetShaderPassEnabled("DepthOnly", false);
            material.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void ClearCache() => Cache.Clear();

        private static Shader LitShader => Shader.Find("Universal Render Pipeline/Lit");

        private static void Save(Object asset, string path)
        {
            InstitutoEditorUtils.EnsureFolder(Folder);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
