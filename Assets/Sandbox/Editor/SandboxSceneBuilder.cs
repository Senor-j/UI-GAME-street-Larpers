using StreetLarpers.HUD.Health;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StreetLarpers.HUD.Sandbox.Editor
{
    /// <summary>
    /// [SOLO SANDBOX] Genera la escena de pruebas del HUD con todas las referencias asignadas.
    /// Menú: Street Larpers ▸ Sandbox ▸ Build HUD Sandbox Scene.
    /// Todo lo que crea se puede editar después a mano; es solo un punto de partida reproducible.
    /// </summary>
    public static class SandboxSceneBuilder
    {
        private const string SceneFolder = "Assets/Sandbox/Scenes";
        private const string ScenePath = SceneFolder + "/HUDSandbox.unity";
        private const string StyleFolder = "Assets/StreetLarpersHUD/Styles";
        private const string StylePath = StyleFolder + "/DefaultHealthBarStyle.asset";

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 HealthBarSize = new Vector2(760f, 44f);
        private static readonly Vector2 HealthBarMargin = new Vector2(60f, 50f);

        [MenuItem("Street Larpers/Sandbox/Build HUD Sandbox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("HUD Sandbox", $"{ScenePath} ya existe. ¿Sobrescribir?", "Sobrescribir", "Cancelar"))
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var style = LoadOrCreateStyle();

            CreateCamera();

            // --- Simulación (solo sandbox) ---
            var sandboxRoot = new GameObject("Sandbox").transform;
            var p1 = CreateFighter("P1_Fighter", "Player 1", sandboxRoot);
            var p2 = CreateFighter("P2_Fighter", "Player 2", sandboxRoot);

            var debugTools = new GameObject("DebugTools").AddComponent<HudDebugKeyboard>();
            debugTools.transform.SetParent(sandboxRoot, false);
            SetReference(debugTools, "player1", p1);
            SetReference(debugTools, "player2", p2);

            // --- HUD (lo que se llevará al juego real) ---
            var canvas = CreateHudCanvas();
            CreateHealthBar("P1_HealthBar", canvas, p1, style, HealthBarView.FillDirection.LeftToRight);
            CreateHealthBar("P2_HealthBar", canvas, p2, style, HealthBarView.FillDirection.RightToLeft);

            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[HUD Sandbox] Escena generada en {ScenePath}. Pulsa Play para probar.");
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            var camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.27f, 0.35f);
            go.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static SimulatedFighter CreateFighter(string objectName, string displayName, Transform parent)
        {
            var fighter = new GameObject(objectName).AddComponent<SimulatedFighter>();
            fighter.transform.SetParent(parent, false);
            var so = new SerializedObject(fighter);
            so.FindProperty("displayName").stringValue = displayName;
            so.ApplyModifiedPropertiesWithoutUndo();
            return fighter;
        }

        private static RectTransform CreateHudCanvas()
        {
            var go = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler)) { layer = LayerMask.NameToLayer("UI") };
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return (RectTransform)go.transform;
        }

        private static void CreateHealthBar(string objectName, RectTransform canvas, SimulatedFighter source,
            HealthBarStyle style, HealthBarView.FillDirection direction)
        {
            bool left = direction == HealthBarView.FillDirection.LeftToRight;

            // Raíz: anclada a la esquina superior izquierda (P1) o derecha (P2).
            var root = CreateUiObject(objectName, canvas);
            var corner = new Vector2(left ? 0f : 1f, 1f);
            root.anchorMin = corner;
            root.anchorMax = corner;
            root.pivot = corner;
            root.sizeDelta = HealthBarSize;
            root.anchoredPosition = new Vector2(left ? HealthBarMargin.x : -HealthBarMargin.x, -HealthBarMargin.y);

            var background = CreateStretchedImage("Background", root);
            var fill = CreateStretchedImage("Fill", root);

            var view = root.gameObject.AddComponent<HealthBarView>();
            SetReference(view, "fill", fill.rectTransform);
            SetReference(view, "fillGraphic", fill);
            SetReference(view, "background", background);
            SetReference(view, "style", style);
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("direction").enumValueIndex = (int)direction;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            var binder = root.gameObject.AddComponent<HealthBarBinder>();
            SetReference(binder, "healthSource", source);
            SetReference(binder, "view", view);
        }

        private static Image CreateStretchedImage(string objectName, RectTransform parent)
        {
            var rect = CreateUiObject(objectName, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false; // El HUD no debe bloquear clics.
            return image;
        }

        private static RectTransform CreateUiObject(string objectName, Transform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform)) { layer = LayerMask.NameToLayer("UI") };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static HealthBarStyle LoadOrCreateStyle()
        {
            var style = AssetDatabase.LoadAssetAtPath<HealthBarStyle>(StylePath);
            if (style != null) return style;

            EnsureFolder(StyleFolder);
            style = ScriptableObject.CreateInstance<HealthBarStyle>();
            AssetDatabase.CreateAsset(style, StylePath);
            AssetDatabase.SaveAssets();
            return style;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
