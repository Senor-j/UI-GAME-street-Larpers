using System;

namespace StreetLarpers.HUD
{
    /// <summary>
    /// Contrato que el HUD necesita para mostrar la vida de algo.
    /// El juego real solo tiene que implementar esta interfaz en su componente de vida
    /// (o en un pequeño adaptador); el HUD nunca conoce la lógica interna del personaje.
    /// </summary>
    public interface IHealthSource
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }

        /// <summary>Se lanza cada vez que la vida cambia (daño, curación, reset...).</summary>
        event Action<HealthChange> HealthChanged;
    }

    /// <summary>
    /// Información de un cambio de vida. Incluye el valor anterior para que el HUD pueda
    /// distinguir daño de curación (feedback, sonidos, barra de vida pendiente...).
    /// </summary>
    public readonly struct HealthChange
    {
        public readonly float Previous;
        public readonly float Current;
        public readonly float Max;

        public HealthChange(float previous, float current, float max)
        {
            Previous = previous;
            Current = current;
            Max = max;
        }

        public float Delta => Current - Previous;
        public bool IsDamage => Delta < 0f;
        public bool IsHeal => Delta > 0f;
        public bool IsDepleted => Current <= 0f;
        public float Normalized => Max > 0f ? Current / Max : 0f;
    }
}
