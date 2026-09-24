using UnityEngine;

namespace StreetLarpers.HUD.Health
{
    /// <summary>
    /// Conecta una fuente de vida (<see cref="IHealthSource"/>) con una <see cref="HealthBarView"/>.
    /// Es el único punto que conoce ambos lados, así la vista y el juego no dependen entre sí.
    /// </summary>
    /// <remarks>
    /// Dos formas de uso:
    ///  - Inspector: arrastrar el componente que implementa IHealthSource a "Health Source".
    ///  - Código (personajes instanciados en runtime): llamar a <see cref="Bind"/>.
    /// </remarks>
    [DisallowMultipleComponent]
    public class HealthBarBinder : MonoBehaviour
    {
        [Tooltip("Cualquier componente que implemente IHealthSource.")]
        [SerializeField] private MonoBehaviour healthSource;
        [SerializeField] private HealthBarView view;

        private IHealthSource source;
        private bool subscribed;

        private void Awake()
        {
            if (source == null && healthSource is IHealthSource fromInspector)
                source = fromInspector;
        }

        private void OnEnable() => Subscribe();
        private void Start() => Refresh();   // Start: todas las fuentes ya han ejecutado su Awake.
        private void OnDisable() => Unsubscribe();

        /// <summary>Cambia (o asigna por primera vez) la fuente de vida en runtime.</summary>
        public void Bind(IHealthSource newSource)
        {
            Unsubscribe();
            source = newSource;
            healthSource = newSource as MonoBehaviour;
            if (isActiveAndEnabled)
                Subscribe();
            Refresh();
        }

        public void Refresh()
        {
            if (source != null && view != null)
                view.SetHealth(source.CurrentHealth, source.MaxHealth);
        }

        private void Subscribe()
        {
            if (subscribed || source == null) return;
            source.HealthChanged += OnHealthChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            source.HealthChanged -= OnHealthChanged;
            subscribed = false;
        }

        private void OnHealthChanged(HealthChange change)
        {
            if (view != null)
                view.SetHealth(change.Current, change.Max);
        }

        private void OnValidate()
        {
            if (healthSource == null || healthSource is IHealthSource) return;

            // Si se arrastró un GameObject, Unity asigna su primer MonoBehaviour: buscamos el correcto.
            var candidate = healthSource.GetComponent<IHealthSource>() as MonoBehaviour;
            if (candidate == null)
                Debug.LogWarning($"{healthSource.name} no implementa IHealthSource.", this);
            healthSource = candidate;
        }
    }
}
