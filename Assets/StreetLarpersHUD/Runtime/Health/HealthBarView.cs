using UnityEngine;
using UnityEngine.UI;

namespace StreetLarpers.HUD.Health
{
    /// <summary>
    /// Vista pura de una barra de vida: recibe un valor y lo dibuja.
    /// No sabe de quién es la vida ni de dónde viene (eso es trabajo de <see cref="HealthBarBinder"/>).
    /// </summary>
    /// <remarks>
    /// El relleno se hace moviendo los anchors del RectTransform "Fill" en lugar de usar
    /// Image.fillAmount, así funciona con cualquier sprite (o sin sprite) y con sprites 9-slice.
    /// </remarks>
    [DisallowMultipleComponent]
    public class HealthBarView : MonoBehaviour
    {
        public enum FillDirection
        {
            LeftToRight, // Jugador 1: la barra se vacía hacia la izquierda.
            RightToLeft  // Jugador 2 (espejo): la barra se vacía hacia la derecha.
        }

        [Header("References")]
        [SerializeField] private RectTransform fill;
        [SerializeField] private Graphic fillGraphic;
        [SerializeField] private Graphic background;

        [Header("Layout")]
        [SerializeField] private FillDirection direction = FillDirection.LeftToRight;

        [Header("Style (optional)")]
        [Tooltip("Si está vacío se respetan los colores puestos a mano en los Graphics.")]
        [SerializeField] private HealthBarStyle style;

        public float Normalized { get; private set; } = 1f;

        private void Awake()
        {
            ApplyStyle();
            ApplyValue();
        }

        public void SetHealth(float current, float max) => SetNormalized(max > 0f ? current / max : 0f);

        public void SetNormalized(float normalized)
        {
            Normalized = Mathf.Clamp01(normalized);
            ApplyValue();
        }

        public void SetStyle(HealthBarStyle newStyle)
        {
            style = newStyle;
            ApplyStyle();
            ApplyValue();
        }

        private void ApplyStyle()
        {
            if (style != null && background != null)
                background.color = style.BackgroundColor;
        }

        private void ApplyValue()
        {
            if (fill != null)
            {
                bool leftToRight = direction == FillDirection.LeftToRight;
                fill.anchorMin = new Vector2(leftToRight ? 0f : 1f - Normalized, 0f);
                fill.anchorMax = new Vector2(leftToRight ? Normalized : 1f, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
            }

            if (style != null && fillGraphic != null)
                fillGraphic.color = style.GetFillColor(Normalized);
        }
    }
}
