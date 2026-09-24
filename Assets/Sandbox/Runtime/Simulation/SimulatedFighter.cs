using System;
using UnityEngine;

namespace StreetLarpers.HUD.Sandbox
{
    /// <summary>
    /// [SOLO SANDBOX] Luchador ficticio que simula la vida de un jugador.
    /// En el juego real se sustituye por el componente de vida del equipo implementando IHealthSource.
    /// </summary>
    public class SimulatedFighter : MonoBehaviour, IHealthSource
    {
        [SerializeField] private string displayName = "Player";
        [SerializeField, Min(1f)] private float maxHealth = 1000f;

        private float currentHealth;

        public string DisplayName => displayName;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        public event Action<HealthChange> HealthChanged;

        private void Awake() => currentHealth = maxHealth;

        public void ApplyDamage(float amount) => SetHealth(currentHealth - Mathf.Abs(amount));
        public void Heal(float amount) => SetHealth(currentHealth + Mathf.Abs(amount));

        public void SetHealth(float value)
        {
            float previous = currentHealth;
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            if (!Mathf.Approximately(previous, currentHealth))
                HealthChanged?.Invoke(new HealthChange(previous, currentHealth, maxHealth));
        }

        // Accesos rápidos desde el menú contextual del componente (clic derecho / ⋮ en el Inspector).
        [ContextMenu("Test/Damage 100")] private void TestDamage() => ApplyDamage(100f);
        [ContextMenu("Test/Heal 100")] private void TestHeal() => Heal(100f);
        [ContextMenu("Test/KO")] private void TestKo() => SetHealth(0f);
        [ContextMenu("Test/Full Health")] public void ResetHealth() => SetHealth(maxHealth);
    }
}
