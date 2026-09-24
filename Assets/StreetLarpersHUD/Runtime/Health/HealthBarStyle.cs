using UnityEngine;

namespace StreetLarpers.HUD.Health
{
    /// <summary>
    /// Aspecto visual de una barra de vida, separado del prefab/escena.
    /// Cambiar el estilo artístico = crear/editar otro asset, sin tocar código.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthBarStyle", menuName = "Street Larpers/HUD/Health Bar Style")]
    public class HealthBarStyle : ScriptableObject
    {
        [SerializeField] private Color fillColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.15f, 0.1f);
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
        [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.14f);

        public Color BackgroundColor => backgroundColor;
        public float LowHealthThreshold => lowHealthThreshold;

        public Color GetFillColor(float normalizedHealth) =>
            normalizedHealth <= lowHealthThreshold ? lowHealthColor : fillColor;
    }
}
