using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StreetLarpers.Instituto.Editor
{
    /// <summary>
    /// Ajustes globales de URP que forman parte de la "receta visual" (sección I del documento).
    /// Menú: Street Larpers ▸ Instituto ▸ Apply Render Settings.
    /// </summary>
    /// <remarks>
    /// Se aplica a TODOS los URP Assets del proyecto (uno por nivel de calidad en la plantilla de Unity 6).
    /// SSAO y Decals no se añaden por código a propósito: la API de renderer features no es pública y
    /// tocarla a mano puede corromper el asset. Aquí solo se comprueba si faltan y se avisa.
    /// </remarks>
    public static class InstitutoRenderSetup
    {
        public const float RenderScale = 0.6f;

        [MenuItem("Street Larpers/Instituto/Apply Render Settings")]
        public static void Apply()
        {
            if (!EditorUtility.DisplayDialog("Ajustes de render del instituto",
                    $"Se cambiará en todos los URP Assets:\n\n• Render Scale = {RenderScale}\n• Upscaling Filter = Nearest-Neighbor (Point)\n\n" +
                    "Es el aspecto retro del juego. ¿Continuar?", "Aplicar", "Cancelar"))
                return;

            Visited.Clear();
            int changed = 0;
            for (int i = 0; i < QualitySettings.count; i++)
                changed += ApplyTo(QualitySettings.GetRenderPipelineAssetAt(i) as UniversalRenderPipelineAsset);
            changed += ApplyTo(GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset);

            if (changed == 0)
            {
                Debug.LogError("[Instituto] No se ha encontrado ningún Universal Render Pipeline Asset. ¿El proyecto usa URP?");
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Instituto] Ajustes de render aplicados a {changed} URP Asset(s).");
        }

        private static readonly System.Collections.Generic.HashSet<Object> Visited = new System.Collections.Generic.HashSet<Object>();

        private static int ApplyTo(UniversalRenderPipelineAsset asset)
        {
            if (asset == null || !Visited.Add(asset)) return 0;

            Undo.RecordObject(asset, "Instituto render settings");
            asset.renderScale = RenderScale;
            asset.upscalingFilter = UpscalingFilterSelection.Point;
            EditorUtility.SetDirty(asset);

            CheckRendererFeatures(asset);
            return 1;
        }

        private static void CheckRendererFeatures(UniversalRenderPipelineAsset asset)
        {
            var list = new SerializedObject(asset).FindProperty("m_RendererDataList");
            if (list == null) return;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (!(list.GetArrayElementAtIndex(i).objectReferenceValue is ScriptableRendererData data)) continue;

                bool hasSsao = false, hasDecals = false;
                foreach (var feature in data.rendererFeatures)
                {
                    if (feature == null) continue;
                    string type = feature.GetType().Name;
                    hasSsao |= type == "ScreenSpaceAmbientOcclusion";
                    hasDecals |= type == "DecalRendererFeature";
                }

                if (!hasSsao)
                    Debug.LogWarning($"[Instituto] '{data.name}' no tiene SSAO. Add Renderer Feature ▸ Screen Space Ambient Occlusion " +
                                     "(Intensity 1.2, Radius 0.35, Direct Lighting Strength 0.35, Samples Medium, Downsample).", data);
                if (!hasDecals)
                    Debug.LogWarning($"[Instituto] '{data.name}' no tiene Decals. Add Renderer Feature ▸ Decal (para el desgaste compartido).", data);
            }
        }
    }
}
