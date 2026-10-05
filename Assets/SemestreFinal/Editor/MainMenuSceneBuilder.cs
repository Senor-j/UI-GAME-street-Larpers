using System.Linq;
using SemestreFinal.Core;
using SemestreFinal.Interaction;
using SemestreFinal.Lighting;
using SemestreFinal.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static SemestreFinal.Editor.InstitutoEditorUtils;
using Pal = SemestreFinal.Editor.InstitutoPalette;

namespace SemestreFinal.Editor
{
    /// <summary>
    /// Genera el blockout jugable del menú principal a partir de la foto de referencia (sala común del semisótano).
    /// Menú: Semestre Final ▸ Build Main Menu Scene.
    /// </summary>
    /// <remarks>
    /// Todo son primitivas para validar composición, luz y menú antes de modelar. Los modelos finales
    /// sustituyen a las cajas manteniendo nombres y posiciones. Ver docs/direccion-artistica-menu-principal.md.
    /// Ejes: +Z = hacia el fondo de la sala (pared de armarios y cristalera), +X = derecha, pared de fieltro en X mínima.
    /// </remarks>
    public static class MainMenuSceneBuilder
    {
        private const string Root = "Assets/SemestreFinal";
        private const string SceneFolder = Root + "/Scenes";
        private const string ScenePath = SceneFolder + "/MainMenu.unity";
        private const string SettingsFolder = Root + "/Settings";
        private const string ProfilePath = SettingsFolder + "/VP_Instituto_Base.asset";
        private const string LightingPath = SettingsFolder + "/LS_Instituto.lighting";

        // --- Sala (metros) ---
        private const float MinX = -2.4f, MaxX = 9f, MinZ = -3f, MaxZ = 11.5f, CeilingY = 2.7f;
        private const float ThresholdZ = 0.3f; // Cambio de baldosa gris (pasillo) a blanca (sala).
        private const float WindowMinX = 1f, WindowMaxX = 5f, WindowMinY = 1f, WindowMaxY = 2.3f;
        private const float ClassroomDepth = 4f;

        // --- Jugador y tablón ---
        // Validado con la previsualización: más cerca del tablón para que los folios se lean con Render Scale 0,6.
        private static readonly Vector3 HomePosition = new Vector3(-1.2f, 1.62f, 0.9f);
        private static readonly Vector3 HomeEuler = new Vector3(3f, -27f, 0f);
        private static readonly Vector3 TablonPosition = new Vector3(MinX + 0.04f, 1.45f, 2.4f);

        private static readonly Vector3 WhiteboardPosition = new Vector3(4.6f, 0f, 5.6f);

        private static readonly Color Ink = Hex("#1A1A1C");
        private static readonly Color InkBlue = Hex("#24345C");
        private static readonly Color Cartulina = Hex("#A3687A");

        [MenuItem("Semestre Final/Build Main Menu Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("Menú principal", $"{ScenePath} ya existe. ¿Sobrescribir?", "Sobrescribir", "Cancelar"))
                return;

            if (TMP_Settings.defaultFontAsset == null)
                Debug.LogWarning("[Instituto] TextMeshPro no tiene fuente por defecto. Importa 'TMP Essential Resources' " +
                                 "(Window ▸ TextMeshPro ▸ Import TMP Essential Resources) y vuelve a generar la escena.");

            Pal.ClearCache();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureEnvironment();

            BuildRoom(Group("Arquitectura"));
            BuildClassroom(Group("Aula_TrasCristalera"));
            BuildProps(Group("Mobiliario"));
            var exitLight = BuildLights(Group("Iluminacion"));

            var rig = BuildPlayerHead(out var camera);
            BuildMenu(rig, camera, exitLight);
            BuildVolume();

            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();

            Debug.Log($"[Instituto] Escena generada en {ScenePath}.\n" +
                      "Siguientes pasos: 1) Semestre Final ▸ Apply Render Settings. " +
                      "2) Añadir 'Screen Space Ambient Occlusion' y 'Decal' al Universal Renderer. " +
                      "3) Window ▸ Rendering ▸ Lighting ▸ Generate Lighting. 4) Play.");
        }

        // =====================================================================
        // Entorno
        // =====================================================================

        private static void ConfigureEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Hex("#0B0D10"); // La oscuridad es real: casi nada de ambiente.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("#0A0B0D");
            RenderSettings.fogStartDistance = 6f;
            RenderSettings.fogEndDistance = 28f;
            RenderSettings.reflectionIntensity = 0.4f;

            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = true;

            var lighting = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingPath);
            if (lighting == null)
            {
                EnsureFolder(SettingsFolder);
                lighting = new LightingSettings { name = "LS_Instituto" };
                AssetDatabase.CreateAsset(lighting, LightingPath);
            }
            lighting.bakedGI = true;
            lighting.realtimeGI = false;
            lighting.mixedBakeMode = MixedLightingMode.IndirectOnly;
            lighting.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            lighting.lightmapResolution = 20f;
            EditorUtility.SetDirty(lighting);
            Lightmapping.lightingSettings = lighting;
        }

        // =====================================================================
        // Arquitectura
        // =====================================================================

        private static void BuildRoom(Transform parent)
        {
            float width = MaxX - MinX, midX = (MinX + MaxX) * 0.5f;

            // Suelo: franja gris del pasillo + baldosa blanca de la sala (frontera narrativa: "aún no has entrado").
            Box("Suelo_Pasillo_BaldosaGris", parent, new Vector3(midX, -0.05f, (MinZ + ThresholdZ) * 0.5f),
                new Vector3(width, 0.1f, ThresholdZ - MinZ), Pal.BaldosaGris);
            Box("Suelo_Sala_BaldosaBlanca", parent, new Vector3(midX, -0.05f, (ThresholdZ + MaxZ) * 0.5f),
                new Vector3(width, 0.1f, MaxZ - ThresholdZ), Pal.BaldosaBlanca);

            Box("Techo", parent, new Vector3(midX, CeilingY + 0.05f, (MinZ + MaxZ) * 0.5f),
                new Vector3(width, 0.1f, MaxZ - MinZ), Pal.Techo);
            foreach (float z in new[] { 1.5f, 4.5f, 7.5f, 10.5f })
                Box($"Viga_z{z:0.0}", parent, new Vector3(midX, CeilingY - 0.12f, z), new Vector3(width, 0.24f, 0.3f), Pal.Viga);

            // Paredes.
            Box("Pared_Izquierda", parent, new Vector3(MinX - 0.05f, CeilingY * 0.5f, (MinZ + MaxZ) * 0.5f),
                new Vector3(0.1f, CeilingY, MaxZ - MinZ), Pal.Pared);
            Box("Pared_Derecha", parent, new Vector3(MaxX + 0.05f, CeilingY * 0.5f, (MinZ + MaxZ) * 0.5f),
                new Vector3(0.1f, CeilingY, MaxZ - MinZ), Pal.Pared);
            Box("Pared_Pasillo", parent, new Vector3(midX, CeilingY * 0.5f, MinZ - 0.05f),
                new Vector3(width, CeilingY, 0.1f), Pal.Pared);

            // Pared del fondo con el hueco de la cristalera del aula.
            float backZ = MaxZ + 0.05f;
            Box("ParedFondo_Izq", parent, new Vector3((MinX + WindowMinX) * 0.5f, CeilingY * 0.5f, backZ),
                new Vector3(WindowMinX - MinX, CeilingY, 0.1f), Pal.Pared);
            Box("ParedFondo_Der", parent, new Vector3((WindowMaxX + MaxX) * 0.5f, CeilingY * 0.5f, backZ),
                new Vector3(MaxX - WindowMaxX, CeilingY, 0.1f), Pal.Pared);
            Box("ParedFondo_BajoCristal", parent, new Vector3((WindowMinX + WindowMaxX) * 0.5f, WindowMinY * 0.5f, backZ),
                new Vector3(WindowMaxX - WindowMinX, WindowMinY, 0.1f), Pal.Pared);
            Box("ParedFondo_SobreCristal", parent, new Vector3((WindowMinX + WindowMaxX) * 0.5f, (WindowMaxY + CeilingY) * 0.5f, backZ),
                new Vector3(WindowMaxX - WindowMinX, CeilingY - WindowMaxY, 0.1f), Pal.Pared);
            Box("Cristalera", parent, new Vector3((WindowMinX + WindowMaxX) * 0.5f, (WindowMinY + WindowMaxY) * 0.5f, backZ),
                new Vector3(WindowMaxX - WindowMinX, WindowMaxY - WindowMinY, 0.01f), Pal.Cristal, collider: false);
            for (float x = WindowMinX; x <= WindowMaxX + 0.01f; x += 1f)
                Box($"Cristalera_Montante_{x:0}", parent, new Vector3(x, (WindowMinY + WindowMaxY) * 0.5f, backZ - 0.02f),
                    new Vector3(0.05f, WindowMaxY - WindowMinY, 0.05f), Pal.MetalGris);

            // Pared de fieltro gris: el tablón donde vive el menú.
            Box("Fieltro_Tablon", parent, new Vector3(MinX + 0.015f, 1.475f, 3.75f), new Vector3(0.03f, 1.95f, 10.5f), Pal.Fieltro);

            // Pilares chapados en piedra.
            Box("Pilar_Piedra_A", parent, new Vector3(3.4f, CeilingY * 0.5f, 5f), new Vector3(0.45f, CeilingY, 0.45f), Pal.Piedra);
            Box("Pilar_Piedra_B", parent, new Vector3(6.2f, CeilingY * 0.5f, 8.5f), new Vector3(0.45f, CeilingY, 0.45f), Pal.Piedra);
        }

        private static void BuildClassroom(Transform parent)
        {
            float minX = WindowMinX - 0.5f, maxX = WindowMaxX + 0.5f;
            float minZ = MaxZ + 0.1f, maxZ = MaxZ + ClassroomDepth;
            float midX = (minX + maxX) * 0.5f, midZ = (minZ + maxZ) * 0.5f, width = maxX - minX;

            Box("Aula_Suelo", parent, new Vector3(midX, -0.05f, midZ), new Vector3(width, 0.1f, ClassroomDepth), Pal.BaldosaBlanca);
            Box("Aula_Techo", parent, new Vector3(midX, CeilingY + 0.05f, midZ), new Vector3(width, 0.1f, ClassroomDepth), Pal.Techo);
            Box("Aula_Pared_Izq", parent, new Vector3(minX - 0.05f, CeilingY * 0.5f, midZ), new Vector3(0.1f, CeilingY, ClassroomDepth), Pal.Pared);
            Box("Aula_Pared_Der", parent, new Vector3(maxX + 0.05f, CeilingY * 0.5f, midZ), new Vector3(0.1f, CeilingY, ClassroomDepth), Pal.Pared);
            Box("Aula_Pared_Fondo", parent, new Vector3(midX, CeilingY * 0.5f, maxZ + 0.05f), new Vector3(width, CeilingY, 0.1f), Pal.Pared);

            // Pantalla del proyector: muestra la hora congelada (fuente de la luz fría del aula).
            Box("Aula_PantallaProyector", parent, new Vector3(midX, 1.6f, maxZ - 0.01f), new Vector3(1.8f, 1.1f, 0.02f),
                Pal.PantallaProyector, collider: false, isStatic: false);
            Text("Aula_PantallaProyector_Hora", parent, new Vector3(midX, 1.6f, maxZ - 0.025f),
                $"TEMA 7\n<size=160%>{InstitutoClock.FrozenTimeText}</size>", new Vector2(1.4f, 0.8f), Hex("#2A3550"));
            PointLight("Aula_LuzProyector", parent, new Vector3(midX, 1.6f, maxZ - 0.6f), Hex("#CFDDF5"), 0.9f, 4.5f,
                LightShadows.None, LightmapBakeType.Mixed);

            // Pupitres vacíos.
            for (int row = 0; row < 2; row++)
                for (int col = 0; col < 3; col++)
                {
                    var p = new Vector3(minX + 1.2f + col * 1.6f, 0f, minZ + 1.0f + row * 1.3f);
                    Table($"Aula_Pupitre_{row}{col}", parent, p, 0f, new Vector2(0.7f, 0.5f));
                    Chair($"Aula_Silla_{row}{col}", parent, p + new Vector3(0f, 0f, -0.45f), 0f, Pal.SillaNaranja);
                }
        }

        // =====================================================================
        // Mobiliario (elementos de la foto + objetos "fuera de lugar")
        // =====================================================================

        private static void BuildProps(Transform parent)
        {
            // Armarios de madera con cinta de embalar (pared del fondo, izquierda).
            for (int i = 0; i < 4; i++)
            {
                float x = MinX + 0.5f + i * 0.76f;
                Box($"Armario_{i}", parent, new Vector3(x, 1.0f, MaxZ - 0.26f), new Vector3(0.74f, 2.0f, 0.5f), Pal.Madera);
                Box($"Armario_{i}_Cinta", parent, new Vector3(x, 1.0f + (i % 2) * 0.35f, MaxZ - 0.505f),
                    new Vector3(0.05f, 0.5f, 0.005f), Pal.Cinta, collider: false);
            }

            // TV de pared sobre los armarios: standby con la hora congelada.
            var tvPos = new Vector3(-0.8f, 2.33f, MaxZ - 0.06f);
            Box("TV_Carcasa", parent, tvPos, new Vector3(0.72f, 0.42f, 0.08f), Pal.PlasticoNegro);
            Box("TV_Pantalla", parent, tvPos + new Vector3(0f, 0f, -0.041f), new Vector3(0.66f, 0.36f, 0.002f),
                Pal.PantallaTv, collider: false, isStatic: false);
            var tvText = Text("TV_Hora", parent, tvPos + new Vector3(0f, 0f, -0.044f), InstitutoClock.FrozenTimeText,
                new Vector2(0.4f, 0.14f), Hex("#5FD6B8"));
            var tvClock = tvText.gameObject.AddComponent<FrozenClock>();
            SetRef(tvClock, "digitalDisplay", tvText);
            Refresh(tvClock);
            PointLight("TV_Luz", parent, tvPos + new Vector3(0f, -0.1f, -0.3f), Hex("#5FD6B8"), 0.25f, 1.6f, LightShadows.None, LightmapBakeType.Realtime);

            // Reloj de pared sobre la cristalera: sin el "11", siempre a las 11:29.
            WallClock(parent, new Vector3((WindowMinX + WindowMaxX) * 0.5f, 2.48f, MaxZ - 0.02f));

            // Puerta de salida + señal verde + luz de emergencia (pared del fondo, derecha).
            Box("Puerta_Salida", parent, new Vector3(6.5f, 1.025f, MaxZ - 0.03f), new Vector3(0.9f, 2.05f, 0.05f), Pal.Madera);
            Box("Senal_Salida", parent, new Vector3(6.5f, 2.4f, MaxZ - 0.04f), new Vector3(0.36f, 0.15f, 0.06f), Pal.SenalSalida, isStatic: false);
            Text("Senal_Salida_Texto", parent, new Vector3(6.5f, 2.4f, MaxZ - 0.072f), "SALIDA", new Vector2(0.3f, 0.1f), Hex("#E8F5EC"));
            PointLight("Senal_Salida_Luz", parent, new Vector3(6.5f, 2.3f, MaxZ - 0.3f), Hex("#3BD16F"), 0.5f, 2.2f, LightShadows.None, LightmapBakeType.Mixed);
            Box("LuzEmergencia", parent, new Vector3(7.3f, 2.4f, MaxZ - 0.04f), new Vector3(0.3f, 0.09f, 0.07f), Pal.LuzEmergencia);
            PointLight("LuzEmergencia_Luz", parent, new Vector3(7.3f, 2.25f, MaxZ - 0.3f), Hex("#EDEDE6"), 0.35f, 2.5f, LightShadows.None, LightmapBakeType.Baked);

            // Extintor y pulsador de incendios (el único rojo: un LED).
            Cylinder("Extintor", parent, new Vector3(MinX + 0.12f, 0.75f, 10.2f), new Vector3(0.16f, 0.27f, 0.16f), Pal.Extintor);
            Box("Pulsador_Incendios", parent, new Vector3(MinX + 0.04f, 1.5f, 10.6f), new Vector3(0.05f, 0.12f, 0.12f), Pal.Extintor);
            var led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            led.name = "Pulsador_LED";
            led.transform.SetParent(parent, false);
            led.transform.localPosition = new Vector3(MinX + 0.07f, 1.54f, 10.6f);
            led.transform.localScale = Vector3.one * 0.012f;
            led.GetComponent<Renderer>().sharedMaterial = Pal.LedRojo;
            Object.DestroyImmediate(led.GetComponent<Collider>());

            // Mesas de patas verdes con sillas naranjas y negras (como en la foto).
            Table("Mesa_A", parent, new Vector3(1.5f, 0f, 4.0f), 5f, new Vector2(1.6f, 0.8f));
            Chair("Mesa_A_Silla_1", parent, new Vector3(1.0f, 0f, 3.35f), 0f, Pal.SillaNaranja);
            Chair("Mesa_A_Silla_2", parent, new Vector3(2.0f, 0f, 3.3f), 10f, Pal.SillaNegra);
            Chair("Mesa_A_Silla_3", parent, new Vector3(1.6f, 0f, 4.7f), 180f, Pal.SillaNaranja);
            Box("Mesa_A_Folio", parent, new Vector3(1.3f, 0.752f, 4.05f), new Vector3(0.21f, 0.002f, 0.297f), Pal.Papel, collider: false);

            Table("Mesa_B", parent, new Vector3(3.9f, 0f, 6.8f), -3f, new Vector2(1.6f, 0.8f));
            Chair("Mesa_B_Silla_1", parent, new Vector3(3.3f, 0f, 6.15f), 0f, Pal.SillaNegra);
            Chair("Mesa_B_Silla_2", parent, new Vector3(4.4f, 0f, 6.2f), -15f, Pal.SillaNaranja);
            Chair("Mesa_B_Silla_3", parent, new Vector3(3.6f, 0f, 7.45f), 180f, Pal.SillaNegra);
            // Sección 14: la mochila azul SIEMPRE junto a la tercera silla de la segunda mesa.
            Box("Mochila_Azul", parent, new Vector3(3.95f, 0.2f, 7.6f), new Vector3(0.3f, 0.4f, 0.18f), Pal.Mochila, isStatic: false);

            // Una silla que mira a la pared. Nadie la ha movido.
            Chair("Silla_MirandoPared", parent, new Vector3(MinX + 0.5f, 0f, 8.8f), -90f, Pal.SillaNaranja);

            // Banco verde y papeleras de reciclaje junto al fieltro.
            Box("Banco_Asiento", parent, new Vector3(MinX + 0.3f, 0.45f, 6.0f), new Vector3(0.35f, 0.04f, 1.8f), Pal.BancoVerde);
            Box("Banco_Respaldo", parent, new Vector3(MinX + 0.1f, 0.7f, 6.0f), new Vector3(0.04f, 0.3f, 1.8f), Pal.BancoVerde);
            foreach (float z in new[] { 5.2f, 6.8f })
                Box($"Banco_Pata_{z:0.0}", parent, new Vector3(MinX + 0.3f, 0.215f, z), new Vector3(0.3f, 0.43f, 0.04f), Pal.MetalVerde);
            Box("Papelera_Azul", parent, new Vector3(MinX + 0.25f, 0.35f, 4.3f), new Vector3(0.36f, 0.7f, 0.36f), Pal.PapeleraAzul);
            Box("Papelera_Amarilla", parent, new Vector3(MinX + 0.25f, 0.3f, 4.75f), new Vector3(0.34f, 0.6f, 0.34f), Pal.PapeleraAmarilla);

            // Pizarra blanca con ruedas (destino de AJUSTES). Fuera de la línea de visión hacia la cristalera.
            Whiteboard(parent, WhiteboardPosition, 39f);

            // Radiador y cajas de cartón (pared derecha; se ven al girar).
            for (int i = 0; i < 14; i++)
                Box($"Radiador_Elemento_{i:00}", parent, new Vector3(MaxX - 0.08f, 0.55f, 2.4f + i * 0.07f),
                    new Vector3(0.08f, 0.6f, 0.05f), Pal.Radiador);
            Box("Caja_1", parent, new Vector3(MaxX - 0.6f, 0.3f, 0.6f), new Vector3(0.9f, 0.6f, 0.6f), Pal.Carton);
            Box("Caja_2", parent, new Vector3(MaxX - 0.62f, 0.9f, 0.58f), new Vector3(0.9f, 0.6f, 0.6f), Pal.Carton);
        }

        private static void Table(string name, Transform parent, Vector3 position, float yaw, Vector2 size)
        {
            var root = Group(name, parent, position, Quaternion.Euler(0f, yaw, 0f));
            Box("Tablero", root, new Vector3(0f, 0.73f, 0f), new Vector3(size.x, 0.025f, size.y), Pal.TableroMesa);
            float lx = size.x * 0.5f - 0.05f, lz = size.y * 0.5f - 0.05f;
            foreach (var p in new[] { new Vector2(lx, lz), new Vector2(-lx, lz), new Vector2(lx, -lz), new Vector2(-lx, -lz) })
                Box("Pata", root, new Vector3(p.x, 0.36f, p.y), new Vector3(0.03f, 0.72f, 0.03f), Pal.MetalVerde);
        }

        /// <summary>Silla de tubo: el asiento mira hacia +Z local.</summary>
        private static void Chair(string name, Transform parent, Vector3 position, float yaw, Material shell)
        {
            var root = Group(name, parent, position, Quaternion.Euler(0f, yaw, 0f));
            Box("Asiento", root, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.03f, 0.42f), shell);
            Box("Respaldo", root, new Vector3(0f, 0.7f, -0.2f), new Vector3(0.42f, 0.36f, 0.03f), shell);
            foreach (var p in new[] { new Vector2(0.18f, 0.18f), new Vector2(-0.18f, 0.18f), new Vector2(0.18f, -0.18f), new Vector2(-0.18f, -0.18f) })
                Box("Pata", root, new Vector3(p.x, 0.22f, p.y), new Vector3(0.022f, 0.44f, 0.022f), Pal.MetalGris);
        }

        private static void Whiteboard(Transform parent, Vector3 position, float yaw)
        {
            var root = Group("Pizarra_Ruedas", parent, position, Quaternion.Euler(0f, yaw, 0f));
            Box("Pizarra_Tablero", root, new Vector3(0f, 1.35f, 0f), new Vector3(1.5f, 1.0f, 0.03f), Pal.Pizarra);
            Box("Pizarra_Marco_Sup", root, new Vector3(0f, 1.86f, 0f), new Vector3(1.54f, 0.03f, 0.04f), Pal.MetalGris);
            Box("Pizarra_Marco_Inf", root, new Vector3(0f, 0.84f, -0.03f), new Vector3(1.54f, 0.03f, 0.08f), Pal.MetalGris);
            foreach (float x in new[] { -0.8f, 0.8f })
            {
                Box("Pizarra_Pie", root, new Vector3(x, 0.95f, 0f), new Vector3(0.04f, 1.9f, 0.04f), Pal.MetalGris);
                Box("Pizarra_Base", root, new Vector3(x, 0.04f, 0f), new Vector3(0.05f, 0.04f, 0.6f), Pal.MetalGris);
            }

            // Marcador de posición de AJUSTES "escrito con rotulador" (el submenú real se construye aparte).
            Text("Pizarra_Ajustes", root, new Vector3(0f, 1.35f, -0.017f),
                "<b>NORMAS DEL CENTRO</b>\n\nVolumen      ———————●———\nBrillo          ———●———————\nParpadeo intenso:   SÍ  /  <u>NO</u>\nSensibilidad  ————●——————",
                new Vector2(1.3f, 0.85f), InkBlue, TextAlignmentOptions.Left);
        }

        /// <summary>Reloj de pared: 11 marcas (falta la de las "11") y agujas puestas por <see cref="FrozenClock"/>.</summary>
        private static void WallClock(Transform parent, Vector3 position)
        {
            const float radius = 0.15f;
            var root = Group("Reloj_Pared", parent, position, Quaternion.identity);

            var face = Cylinder("Esfera", root, Vector3.zero, new Vector3(radius * 2f, 0.01f, radius * 2f), Pal.EsferaReloj);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder("Aro", root, new Vector3(0f, 0f, 0.004f), new Vector3(radius * 2.15f, 0.012f, radius * 2.15f), Pal.PlasticoNegro)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            for (int i = 0; i < 12; i++)
            {
                if (i == 11) continue; // El hueco: el mismo que el del Aro Horario.
                float a = i * 30f * Mathf.Deg2Rad;
                var tick = Box($"Marca_{i:00}", root, new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f) * (radius * 0.82f) + new Vector3(0f, 0f, -0.012f),
                    new Vector3(0.008f, 0.03f, 0.002f), Pal.Tinta, collider: false);
                tick.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 30f);
            }

            var hourPivot = Group("Aguja_Horas", root, new Vector3(0f, 0f, -0.015f), Quaternion.identity);
            Box("Forma", hourPivot, new Vector3(0f, radius * 0.25f, 0f), new Vector3(0.012f, radius * 0.5f, 0.003f), Pal.Tinta, collider: false);
            var minutePivot = Group("Aguja_Minutos", root, new Vector3(0f, 0f, -0.018f), Quaternion.identity);
            Box("Forma", minutePivot, new Vector3(0f, radius * 0.38f, 0f), new Vector3(0.008f, radius * 0.76f, 0.003f), Pal.Tinta, collider: false);

            var clock = root.gameObject.AddComponent<FrozenClock>();
            SetRef(clock, "hourHand", hourPivot);
            SetRef(clock, "minuteHand", minutePivot);
            Refresh(clock);
        }

        // =====================================================================
        // Iluminación (sección G): cada luz tiene un objeto que la produce.
        // =====================================================================

        private static FluorescentFlicker BuildLights(Transform parent)
        {
            Fluorescent("F1_Tablon_Normal", parent, new Vector3(-1.6f, 2.62f, 2.6f), FluorescentFlicker.Mode.Steady, 101, 1.7f, LightShadows.Soft);
            var f2 = Fluorescent("F2_Mesas_ParpadeoLento", parent, new Vector3(1.6f, 2.62f, 4.2f), FluorescentFlicker.Mode.SlowDip, 202, 1.5f, LightShadows.Soft);
            Fluorescent("F3_Pizarra_Parcial", parent, new Vector3(1.4f, 2.62f, 7.4f), FluorescentFlicker.Mode.Partial, 303, 1.4f, LightShadows.None);
            Fluorescent("F4_Banco_Irregular", parent, new Vector3(-1.4f, 2.62f, 6.0f), FluorescentFlicker.Mode.Burst, 404, 1.3f, LightShadows.None);
            Fluorescent("F5_Radiador_Apagado", parent, new Vector3(5.0f, 2.62f, 3.0f), FluorescentFlicker.Mode.Dead, 505, 1.5f, LightShadows.None);
            Fluorescent("F6_Pasillo_Normal", parent, new Vector3(-0.5f, 2.62f, -2.0f), FluorescentFlicker.Mode.Steady, 606, 1.2f, LightShadows.None);
            Fluorescent("F7_Aula_Normal", parent, new Vector3((WindowMinX + WindowMaxX) * 0.5f, 2.62f, MaxZ + ClassroomDepth * 0.5f),
                FluorescentFlicker.Mode.Steady, 707, 1.6f, LightShadows.None, kelvin: 3800f);
            Fluorescent("F8_Pilares_Tenue", parent, new Vector3(5.0f, 2.62f, 8.0f), FluorescentFlicker.Mode.Steady, 808, 0.7f, LightShadows.None);
            return f2;
        }

        private static FluorescentFlicker Fluorescent(string name, Transform parent, Vector3 position, FluorescentFlicker.Mode mode,
            int seed, float intensity, LightShadows shadows, float kelvin = 4300f)
        {
            var root = Group(name, parent, position, Quaternion.identity);
            Box("Regleta", root, Vector3.zero, new Vector3(0.12f, 0.05f, 1.3f), Pal.MetalGris);
            var tube = Box("Tubo", root, new Vector3(0f, -0.04f, 0f), new Vector3(0.04f, 0.035f, 1.2f), Pal.TuboFluorescente,
                collider: false, isStatic: false);

            bool steady = mode == FluorescentFlicker.Mode.Steady;
            var light = PointLight("Luz", root, new Vector3(0f, -0.15f, 0f), Hex("#EEFAEF"), intensity, 6f, shadows,
                steady ? LightmapBakeType.Mixed : LightmapBakeType.Realtime, kelvin);
            if (mode == FluorescentFlicker.Mode.Dead) light.enabled = false;

            var flicker = root.gameObject.AddComponent<FluorescentFlicker>();
            SetRefArray(flicker, "lights", new[] { light });
            SetRef(flicker, "tubeRenderer", tube.GetComponent<Renderer>());
            SetEnum(flicker, "mode", (int)mode);
            SetInt(flicker, "seed", seed);
            return flicker;
        }

        private static Light PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range,
            LightShadows shadows, LightmapBakeType bakeType, float kelvin = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            light.lightmapBakeType = bakeType;
            if (kelvin > 0f)
            {
                light.useColorTemperature = true;
                light.colorTemperature = kelvin;
            }
            return light;
        }

        // =====================================================================
        // Jugador y menú
        // =====================================================================

        private static MenuCameraRig BuildPlayerHead(out Camera camera)
        {
            var head = new GameObject("PlayerHead");
            head.transform.SetPositionAndRotation(HomePosition, Quaternion.Euler(HomeEuler));

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraGo.transform.SetParent(head.transform, false);
            camera = cameraGo.GetComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var rig = head.AddComponent<MenuCameraRig>();
            SetRef(rig, "swayTarget", cameraGo.transform);
            return rig;
        }

        private static void BuildMenu(MenuCameraRig rig, Camera camera, FluorescentFlicker exitLight)
        {
            // El tablón: +Z local hacia la pared, +X hacia el fondo de la sala (derecha del jugador al mirarlo).
            var tablon = new GameObject("Tablon_Menu").transform;
            tablon.SetPositionAndRotation(TablonPosition, Quaternion.Euler(0f, -90f, 0f));
            var sfx = tablon.gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;

            BuildTitle(tablon);

            // Destinos físicos de cada opción.
            var destinations = new GameObject("Destinos_Camara").transform;
            var glassTarget = new Vector3((WindowMinX + WindowMaxX) * 0.5f, 1.5f, MaxZ + ClassroomDepth * 0.6f);
            var toClassroom = Destination("Destino_Aula", destinations, new Vector3(-0.4f, 1.65f, 3.2f), glassTarget);
            var toWhiteboard = Destination("Destino_Pizarra", destinations, new Vector3(3.0f, 1.62f, 3.6f),
                WhiteboardPosition + new Vector3(0f, 1.35f, 0f));
            var toCredits = Destination("Destino_ListaClase", destinations, new Vector3(-1.45f, 1.6f, 1.9f),
                tablon.TransformPoint(new Vector3(0.32f, 0.2f, 0f)));
            var toExit = Destination("Destino_Salida", destinations, HomePosition, new Vector3(6.5f, 2.3f, MaxZ));

            var options = new[]
            {
                Option(tablon, "Opcion_Jugar", MenuOption.Kind.Play, "JUGAR", "INSCRIPCIÓN · ACTIVIDAD DE TARDE", "01",
                    new Vector2(-0.25f, 0.40f), new Vector2(0.36f, 0.19f), 1.5f, Pal.Papel, Pal.ChinchetaRoja, Ink, toClassroom),
                Option(tablon, "Opcion_PartidaPrivada", MenuOption.Kind.PrivateMatch, "PARTIDA PRIVADA", "RESERVA DE AULA", "02",
                    new Vector2(-0.27f, 0.15f), new Vector2(0.36f, 0.19f), -1f, Pal.Papel, Pal.ChinchetaAzul, Ink, toClassroom),
                Option(tablon, "Opcion_Ajustes", MenuOption.Kind.Settings, "AJUSTES", "NORMAS DEL CENTRO", "03",
                    new Vector2(-0.24f, -0.10f), new Vector2(0.36f, 0.19f), 0.8f, Pal.Papel, Pal.ChinchetaAmarilla, Ink, toWhiteboard),
                Option(tablon, "Opcion_Creditos", MenuOption.Kind.Credits, "CRÉDITOS", "LISTA DE CLASE · 2º B", "04",
                    new Vector2(-0.26f, -0.35f), new Vector2(0.36f, 0.19f), -1.8f, Pal.Papel, Pal.ChinchetaRoja, Ink, toCredits),
                Option(tablon, "Opcion_Salir", MenuOption.Kind.Quit, "SALIR", null, "05",
                    new Vector2(-0.30f, -0.56f), new Vector2(0.2f, 0.09f), 3f, Pal.PegatinaSalida, null, Hex("#E8F2EA"), toExit),
            };

            BuildNoticeBoardDecor(tablon);

            // Aro Horario: hermano de las opciones, dibujado en el plano del tablón.
            var aroGo = new GameObject("AroHorario", typeof(MeshFilter), typeof(MeshRenderer));
            aroGo.transform.SetParent(tablon, false);
            aroGo.GetComponent<MeshRenderer>().sharedMaterial = Pal.Glifo;
            var aro = aroGo.AddComponent<AroHorario>();
            SetRef(aro, "audioSource", sfx);

            var controller = new GameObject("MenuController").AddComponent<MenuController>();
            SetRefArray(controller, "options", options);
            SetRef(controller, "aro", aro);
            SetRef(controller, "cameraRig", rig);
            SetRef(controller, "pointerCamera", camera);
            SetRef(controller, "sfxSource", sfx);
            SetRef(controller, "exitLight", exitLight);
        }

        private static Transform Destination(string name, Transform parent, Vector3 position, Vector3 lookAt)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.SetPositionAndRotation(position, Quaternion.LookRotation(lookAt - position, Vector3.up));
            return t;
        }

        /// <summary>Una opción = un folio clavado con sus capas (ver <see cref="MenuOption"/>).</summary>
        private static MenuOption Option(Transform tablon, string name, MenuOption.Kind kind, string label, string header, string index,
            Vector2 position, Vector2 size, float rollDegrees, Material paperMaterial, Material pinMaterial, Color inkColor, Transform destination)
        {
            var root = Group(name, tablon, new Vector3(position.x, position.y, 0f), Quaternion.Euler(0f, 0f, rollDegrees));
            var folio = Group("Folio", root, new Vector3(0f, 0f, -0.002f), Quaternion.identity);

            // Capa 1 (+ hitbox): el papel.
            var paper = Box("Papel", folio, Vector3.zero, new Vector3(size.x, size.y, 0.002f), paperMaterial, isStatic: false);
            paper.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Capa 2: texto principal.
            var text = Text("Texto", folio, new Vector3(0f, -size.y * 0.06f, -0.0015f), label,
                new Vector2(size.x * 0.84f, size.y * 0.42f), inkColor);
            text.fontStyle = FontStyles.Bold;

            // Capa 7: encabezado impreso, índice y subrayado a rotulador.
            if (!string.IsNullOrEmpty(header))
                Text("Encabezado", folio, new Vector3(0f, size.y * 0.33f, -0.0015f), header,
                    new Vector2(size.x * 0.9f, size.y * 0.13f), new Color(inkColor.r, inkColor.g, inkColor.b, 0.6f));
            Text("Indice", folio, new Vector3(size.x * 0.4f, -size.y * 0.37f, -0.0015f), index,
                new Vector2(size.x * 0.12f, size.y * 0.14f), new Color(inkColor.r, inkColor.g, inkColor.b, 0.5f));
            var underline = Box("Subrayado", folio, new Vector3(0f, -size.y * 0.3f, -0.0016f),
                new Vector3(size.x * 0.6f, 0.004f, 0.0005f), Pal.Tinta, collider: false, isStatic: false);

            // Capa 3: chincheta (o nada, si es una pegatina).
            Transform pin = null;
            if (pinMaterial != null)
            {
                pin = Group("Chincheta", folio, new Vector3(0f, size.y * 0.42f, -0.004f), Quaternion.identity);
                Cylinder("Cabeza", pin, Vector3.zero, new Vector3(0.018f, 0.004f, 0.018f), pinMaterial, isStatic: false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            var option = root.gameObject.AddComponent<MenuOption>();
            SetEnum(option, "kind", (int)kind);
            SetRef(option, "cameraDestination", destination);
            SetRef(option, "paper", folio);
            SetRef(option, "paperRenderer", paper.GetComponent<Renderer>());
            SetRef(option, "label", text);
            SetRef(option, "pin", pin);
            SetRef(option, "decoration", underline.transform);
            SetVector2(option, "size", size);
            return option;
        }

        /// <summary>Título en letras de cartulina recortada, grapadas al fieltro. Una se ha caído al suelo.</summary>
        private static void BuildTitle(Transform tablon)
        {
            const string title = "SEMESTRE FINAL";
            const int fallenIndex = 4; // La "S" del medio.
            var root = Group("Titulo_Cartulina", tablon, new Vector3(0f, 0.78f, -0.002f), Quaternion.identity);
            var rng = new System.Random(1129);

            float step = 0.092f, x = -step * (title.Length - 1) * 0.5f;
            for (int i = 0; i < title.Length; i++, x += step)
            {
                char c = title[i];
                if (c == ' ' || i == fallenIndex) continue;

                float roll = (float)(rng.NextDouble() * 2.0 - 1.0) * 6f;
                float dy = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.012f;
                var letter = Text($"Letra_{i:00}_{c}", root, new Vector3(x, dy, 0f), c.ToString(), new Vector2(0.09f, 0.14f), Cartulina);
                letter.fontStyle = FontStyles.Bold;
                letter.transform.localRotation = Quaternion.Euler(0f, 0f, roll);
            }

            // La letra caída, boca arriba bajo el tablón.
            var fallen = Text($"Letra_{fallenIndex:00}_Caida", tablon, Vector3.zero, title[fallenIndex].ToString(), new Vector2(0.09f, 0.14f), Cartulina);
            fallen.fontStyle = FontStyles.Bold;
            fallen.transform.SetParent(null, true);
            fallen.transform.SetPositionAndRotation(new Vector3(MinX + 0.45f, 0.003f, 2.9f), Quaternion.Euler(90f, 35f, 0f));
        }

        /// <summary>El resto del tablón: el horario congelado, una circular y el post-it de versión.</summary>
        private static void BuildNoticeBoardDecor(Transform tablon)
        {
            Notice(tablon, "Cartel_Horario", new Vector2(0.32f, 0.20f), new Vector2(0.34f, 0.44f), -0.8f, Pal.Papel,
                $"<b>HORARIO · 2º B</b>\n<size=70%>{InstitutoClock.FrozenDateText}</size>\n\n" +
                "08:00  Matemáticas\n09:00  Lengua\n10:00  Inglés\n" +
                $"11:00  <s>RECREO</s>  <size=80%>{InstitutoClock.FrozenTimeText}</size>\n11:30  Historia\n12:30  Tutoría",
                TextAlignmentOptions.TopLeft);

            Notice(tablon, "Cartel_Circular", new Vector2(0.34f, -0.20f), new Vector2(0.30f, 0.20f), 1.2f, Pal.Papel,
                $"<b>CIRCULAR {InstitutoClock.FrozenHour}/{InstitutoClock.FrozenMinute}</b>\n<size=75%>Se recuerda al alumnado que el timbre " +
                "sonará a las 11:30.\nPermanezcan en sus sitios.</size>",
                TextAlignmentOptions.TopLeft);

            Notice(tablon, "PostIt_Version", new Vector2(0.47f, -0.52f), new Vector2(0.12f, 0.12f), 6f, Pal.PostIt,
                $"v{Application.version}\n<size=60%>proyecto escolar</size>", TextAlignmentOptions.Center);
        }

        private static void Notice(Transform tablon, string name, Vector2 position, Vector2 size, float roll, Material material,
            string content, TextAlignmentOptions alignment)
        {
            var root = Group(name, tablon, new Vector3(position.x, position.y, -0.002f), Quaternion.Euler(0f, 0f, roll));
            Box("Papel", root, Vector3.zero, new Vector3(size.x, size.y, 0.002f), material, collider: false, isStatic: false);
            Text("Texto", root, new Vector3(0f, 0f, -0.0015f), content, size * 0.86f, Ink, alignment);
        }

        // =====================================================================
        // Post-processing (sección I)
        // =====================================================================

        private static void BuildVolume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                EnsureFolder(SettingsFolder);
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            Configure<Tonemapping>(profile, c => c.mode.Override(TonemappingMode.Neutral));
            Configure<ColorAdjustments>(profile, c =>
            {
                c.postExposure.Override(0f);
                c.contrast.Override(18f);
                c.saturation.Override(-25f);
                c.colorFilter.Override(new Color(0.95f, 0.96f, 0.93f));
            });
            Configure<ShadowsMidtonesHighlights>(profile, c => c.shadows.Override(new Vector4(0.96f, 1.0f, 1.04f, 0f)));
            Configure<Bloom>(profile, c =>
            {
                c.threshold.Override(1.0f);
                c.intensity.Override(0.35f);
                c.scatter.Override(0.55f);
            });
            Configure<FilmGrain>(profile, c =>
            {
                c.type.Override(FilmGrainLookup.Thin2);
                c.intensity.Override(0.12f);
                c.response.Override(0.8f);
            });
            Configure<ChromaticAberration>(profile, c => c.intensity.Override(0.04f));
            Configure<Vignette>(profile, c =>
            {
                c.intensity.Override(0.22f);
                c.smoothness.Override(0.4f);
            });
            EditorUtility.SetDirty(profile);

            var volume = new GameObject("Volume_Global_Instituto").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        private static void Configure<T>(VolumeProfile profile, System.Action<T> setup) where T : VolumeComponent
        {
            if (!profile.TryGet<T>(out var component))
            {
                component = profile.Add<T>();
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            component.active = true;
            setup(component);
            EditorUtility.SetDirty(component);
        }

        // =====================================================================
        // Utilidades
        // =====================================================================

        private static Transform Group(string name) => new GameObject(name).transform;

        private static Transform Group(string name, Transform parent, Vector3 localPosition, Quaternion localRotation)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = localRotation;
            return t;
        }

        private static GameObject Box(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material,
            bool collider = true, bool isStatic = true) =>
            Primitive(PrimitiveType.Cube, name, parent, localPosition, size, material, collider, isStatic);

        private static GameObject Cylinder(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material,
            bool isStatic = true) =>
            Primitive(PrimitiveType.Cylinder, name, parent, localPosition, size, material, false, isStatic);

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 size,
            Material material, bool collider, bool isStatic)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            if (isStatic)
                GameObjectUtility.SetStaticEditorFlags(go,
                    StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
            return go;
        }

        /// <summary>Texto 3D (TextMeshPro) legible mirando hacia +Z local. Se autoajusta a la caja.</summary>
        private static TextMeshPro Text(string name, Transform parent, Vector3 localPosition, string content, Vector2 box, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var tmp = go.AddComponent<TextMeshPro>();
            if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
            tmp.rectTransform.sizeDelta = box;
            tmp.text = content;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.01f;
            tmp.fontSizeMax = 10f;
            tmp.margin = Vector4.zero;
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return tmp;
        }

        /// <summary>Reaplica OnEnable tras asignar referencias, para que la escena se guarde ya con la hora puesta.</summary>
        private static void Refresh(Behaviour behaviour)
        {
            behaviour.enabled = false;
            behaviour.enabled = true;
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
