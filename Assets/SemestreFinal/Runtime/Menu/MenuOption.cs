using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SemestreFinal.Menu
{
    /// <summary>
    /// Una opción del menú construida por capas físicas independientes (sección 7 del documento):
    /// 0 hitbox · 1 folio · 2 texto · 3 chincheta/marco · 4-5 Aro Horario (externo) · 6 luz · 7 decoración.
    /// Cada capa se anima por separado a partir de un único valor de selección suavizado.
    /// </summary>
    /// <remarks>
    /// Convención: el tablón (padre) tiene +Z local hacia la pared; "hacia el jugador" es -Z local.
    /// El texto, la chincheta y la decoración deben ser hijos del folio para moverse con él.
    /// </remarks>
    [DisallowMultipleComponent]
    public class MenuOption : MonoBehaviour
    {
        public enum Kind { Play, PrivateMatch, Settings, Credits, Quit, Custom }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Datos")]
        [SerializeField] private Kind kind = Kind.Custom;
        [Tooltip("Pose de cámara a la que se gira el jugador al confirmar. Vacío = no se mueve.")]
        [SerializeField] private Transform cameraDestination;

        [Header("Capas")]
        [Tooltip("Capa 1: el folio (lleva el collider para el ratón).")]
        [SerializeField] private Transform paper;
        [SerializeField] private Renderer paperRenderer;
        [Tooltip("Capa 2: texto principal.")]
        [SerializeField] private TMP_Text label;
        [Tooltip("Capa 3: chincheta o trozo de cinta.")]
        [SerializeField] private Transform pin;
        [Tooltip("Capa 7: subrayado a mano que aparece al seleccionar.")]
        [SerializeField] private Transform decoration;

        [Header("Tamaño que rodea el Aro Horario (m)")]
        [SerializeField] private Vector2 size = new Vector2(0.36f, 0.2f);

        [Header("Selección (todo sutil)")]
        [SerializeField] private float liftDistance = 0.012f;
        [SerializeField] private float selectedScale = 1.03f;
        [Tooltip("Giro del folio hacia el jugador al seleccionarlo (grados sobre Y local).")]
        [SerializeField] private float selectedYaw = 8f;
        [SerializeField] private float pinTilt = 5f;
        [SerializeField, Range(0f, 1f)] private float idleBrightness = 0.86f;
        [SerializeField, Range(0f, 1f)] private float idleInkAlpha = 0.85f;
        [SerializeField] private float responsiveness = 14f;

        [Header("Eventos")]
        public UnityEvent onConfirmed = new UnityEvent();

        public Kind OptionKind => kind;
        public Transform CameraDestination => cameraDestination;
        public Vector2 Size => size;
        public bool IsSelected { get; private set; }

        private float selection, punch, noiseSeed;
        private Vector3 paperBasePosition, paperBaseScale, decorationBaseScale;
        private Quaternion paperBaseRotation, pinBaseRotation;
        private Color paperBaseColor = Color.white;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            noiseSeed = Random.Range(0f, 100f);
            propertyBlock = new MaterialPropertyBlock();

            if (paper != null)
            {
                paperBasePosition = paper.localPosition;
                paperBaseRotation = paper.localRotation;
                paperBaseScale = paper.localScale;
            }
            if (pin != null) pinBaseRotation = pin.localRotation;
            if (decoration != null) decorationBaseScale = decoration.localScale;
            if (paperRenderer != null && paperRenderer.sharedMaterial != null && paperRenderer.sharedMaterial.HasProperty(BaseColorId))
                paperBaseColor = paperRenderer.sharedMaterial.GetColor(BaseColorId);

            Apply();
        }

        public void SetSelected(bool selected, bool instant = false)
        {
            IsSelected = selected;
            if (instant)
            {
                selection = selected ? 1f : 0f;
                Apply();
            }
        }

        public void Confirm()
        {
            punch = 1f;
            onConfirmed.Invoke();
        }

        /// <summary>Centro del folio levantado, en el espacio de <paramref name="space"/> (normalmente el padre del Aro).</summary>
        public Vector3 GetAroAnchor(Transform space)
        {
            var world = transform.TransformPoint(new Vector3(0f, 0f, -(liftDistance + 0.006f)));
            return space != null ? space.InverseTransformPoint(world) : world;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            selection = Mathf.Lerp(selection, IsSelected ? 1f : 0f, 1f - Mathf.Exp(-responsiveness * dt));
            punch = Mathf.MoveTowards(punch, 0f, dt * 5f);
            Apply();
        }

        private void Apply()
        {
            if (paper != null)
            {
                // Capa 1: el folio se despega, gira un poco hacia el jugador y crece un 3 %.
                paper.localPosition = paperBasePosition + Vector3.back * (liftDistance * selection);
                paper.localRotation = paperBaseRotation * Quaternion.Euler(0f, selectedYaw * selection, 0f);
                float scale = 1f + (selectedScale - 1f) * selection - punch * 0.02f;
                paper.localScale = paperBaseScale * scale;
            }

            // Capa 6: "recibe más luz". Se aclara el papel en lugar de añadir otra luz a la escena.
            if (paperRenderer != null)
            {
                paperRenderer.GetPropertyBlock(propertyBlock);
                float brightness = Mathf.Lerp(idleBrightness, 1f, selection);
                propertyBlock.SetColor(BaseColorId, paperBaseColor * brightness);
                paperRenderer.SetPropertyBlock(propertyBlock);
            }

            // Capa 2: la tinta gana contraste y vibra mínimamente.
            if (label != null)
            {
                float jitter = (Mathf.PerlinNoise(noiseSeed, Time.unscaledTime * 2f) - 0.5f) * 0.06f * selection;
                label.alpha = Mathf.Clamp01(Mathf.Lerp(idleInkAlpha, 1f, selection) + jitter);
            }

            // Capa 3: la chincheta cede un poco.
            if (pin != null) pin.localRotation = pinBaseRotation * Quaternion.Euler(0f, 0f, pinTilt * selection);

            // Capa 7: subrayado a rotulador que "se dibuja".
            if (decoration != null)
            {
                var s = decorationBaseScale;
                decoration.localScale = new Vector3(s.x * selection, s.y, s.z);
                decoration.gameObject.SetActive(selection > 0.01f);
            }
        }
    }
}
