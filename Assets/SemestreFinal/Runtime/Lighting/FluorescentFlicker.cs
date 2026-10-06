using SemestreFinal.Core;
using UnityEngine;

namespace SemestreFinal.Lighting
{
    /// <summary>
    /// Un fluorescente: controla la intensidad de sus <see cref="Light"/> y, a la vez, la emisión del tubo,
    /// para que la luz SIEMPRE tenga una fuente visible coherente (si la luz baja, el tubo se apaga).
    /// </summary>
    /// <remarks>
    /// Cada instancia tiene su propia semilla, así que nunca parpadean sincronizados.
    /// La única sincronía es deliberada: el "pulso del timbre" de <see cref="InstitutoClock.BellMultiplier"/>.
    /// </remarks>
    [DisallowMultipleComponent]
    public class FluorescentFlicker : MonoBehaviour
    {
        public enum Mode
        {
            Steady,  // A: funciona normal (con un zumbido mínimo de intensidad).
            SlowDip, // B: cada pocos segundos baja lentamente y se recupera.
            Partial, // C: medio apagado, con vibración rápida (tubo agotado).
            Burst,   // D: minutos de calma y de repente una ráfaga de cortes.
            Dead     // E: apagado.
        }

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Fuente")]
        [SerializeField] private Light[] lights = new Light[0];
        [Tooltip("Renderer del tubo. Se crea una instancia de su material para poder variar la emisión.")]
        [SerializeField] private Renderer tubeRenderer;
        [SerializeField, Min(0)] private int tubeMaterialIndex;
        [SerializeField, ColorUsage(false, true)] private Color tubeEmission = new Color(2.2f, 2.35f, 2.2f);

        [Header("Comportamiento")]
        [SerializeField] private Mode mode = Mode.Steady;
        [Tooltip("0 = aleatoria en cada partida.")]
        [SerializeField] private int seed;
        [Tooltip("Variación continua de intensidad (zumbido). 0.02 = ±2 %.")]
        [SerializeField, Range(0f, 0.2f)] private float hum = 0.02f;
        [SerializeField] private bool useUnscaledTime = true;
        [Tooltip("Sigue el pulso común del timbre (bajada del 5 % cada 60 s).")]
        [SerializeField] private bool followBell = true;

        [Header("B · SlowDip")]
        [SerializeField] private Vector2 dipInterval = new Vector2(6f, 14f);
        [SerializeField] private Vector2 dipDuration = new Vector2(0.3f, 0.8f);
        [SerializeField, Range(0f, 1f)] private float dipLevel = 0.3f;

        [Header("C · Partial")]
        [SerializeField, Range(0f, 1f)] private float partialLevel = 0.35f;

        [Header("D · Burst")]
        [SerializeField] private Vector2 burstCalm = new Vector2(20f, 90f);
        [SerializeField] private Vector2Int burstCuts = new Vector2Int(2, 5);

        /// <summary>Nivel aplicado en este frame (0–1.1). Útil para que otros objetos hereden el parpadeo.</summary>
        public float CurrentLevel { get; private set; } = 1f;

        public Mode CurrentMode => mode;

        private System.Random rng;
        private float[] baseIntensities;
        private Material tubeMaterial;
        private float noiseOffset;
        private float smoothed = 1f;
        private float overshoot;
        private float previousLevel = 1f;

        // Estado de los modos con temporizador.
        private float nextEventIn;
        private float dipRemaining;
        private int cutsLeft;
        private bool isCut;
        private float phaseRemaining;
        private float cutLevel;

        private void Awake()
        {
            rng = new System.Random(seed != 0 ? seed : System.Environment.TickCount ^ GetInstanceID());
            noiseOffset = Range(0f, 1000f);

            baseIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
                baseIntensities[i] = lights[i] != null ? lights[i].intensity : 0f;

            if (tubeRenderer != null)
            {
                var materials = tubeRenderer.materials; // Instancia: cada tubo parpadea por su cuenta.
                if (tubeMaterialIndex < materials.Length)
                {
                    tubeMaterial = materials[tubeMaterialIndex];
                    tubeMaterial.EnableKeyword("_EMISSION");
                }
            }

            ScheduleNext();
            // Fase inicial aleatoria para que dos luces con el mismo modo no arranquen igual.
            nextEventIn *= (float)rng.NextDouble();
        }

        private void OnDestroy()
        {
            if (tubeMaterial != null) Destroy(tubeMaterial);
        }

        /// <summary>Cambia el comportamiento en caliente (p. ej. apagar una luz al salir del juego).</summary>
        public void SetMode(Mode newMode)
        {
            mode = newMode;
            if (rng != null) ScheduleNext();
        }

        private void Update()
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float time = useUnscaledTime ? Time.unscaledTime : Time.time;

            float target = EvaluateMode(dt, time);

            // Los cortes eléctricos son instantáneos; las bajadas lentas se suavizan.
            float response = mode == Mode.SlowDip ? 10f : 60f;
            smoothed = Mathf.Lerp(smoothed, target, 1f - Mathf.Exp(-response * dt));

            // Al volver de apagado, un tubo real da un pequeño destello antes de estabilizarse.
            if (previousLevel < 0.2f && smoothed >= 0.8f) overshoot = 0.1f;
            overshoot = Mathf.MoveTowards(overshoot, 0f, dt * 0.4f);
            previousLevel = smoothed;

            float level = smoothed + overshoot;
            if (level > 0f)
            {
                float noise = Mathf.PerlinNoise(noiseOffset, time * 3f) - 0.5f;
                level *= 1f + noise * 2f * hum;
                if (followBell) level *= InstitutoClock.BellMultiplier;
            }

            Apply(Mathf.Max(0f, level));
        }

        private float EvaluateMode(float dt, float time)
        {
            switch (mode)
            {
                case Mode.Dead:
                    return 0f;

                case Mode.Partial:
                    // Vibración rápida e irregular alrededor de un nivel bajo.
                    float buzz = Mathf.PerlinNoise(noiseOffset + 37f, time * 18f) - 0.5f;
                    return Mathf.Clamp01(partialLevel * (1f + buzz * 0.6f));

                case Mode.SlowDip:
                    if (dipRemaining > 0f)
                    {
                        dipRemaining -= dt;
                        return dipLevel;
                    }
                    nextEventIn -= dt;
                    if (nextEventIn <= 0f)
                    {
                        dipRemaining = Range(dipDuration.x, dipDuration.y);
                        ScheduleNext();
                    }
                    return 1f;

                case Mode.Burst:
                    if (cutsLeft > 0)
                    {
                        phaseRemaining -= dt;
                        if (phaseRemaining <= 0f)
                        {
                            isCut = !isCut;
                            if (isCut) cutLevel = Range(0f, 0.15f);
                            else cutsLeft--;
                            phaseRemaining = isCut ? Range(0.03f, 0.12f) : Range(0.04f, 0.25f);
                        }
                        return isCut ? cutLevel : 1f;
                    }
                    nextEventIn -= dt;
                    if (nextEventIn <= 0f)
                    {
                        cutsLeft = rng.Next(burstCuts.x, burstCuts.y + 1);
                        isCut = false;
                        phaseRemaining = 0f;
                        ScheduleNext();
                    }
                    return 1f;

                default:
                    return 1f;
            }
        }

        private void Apply(float level)
        {
            CurrentLevel = level;

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null) continue;
                lights[i].intensity = baseIntensities[i] * level;
                lights[i].enabled = level > 0.001f;
            }

            if (tubeMaterial != null)
                tubeMaterial.SetColor(EmissionColorId, tubeEmission * level);
        }

        private void ScheduleNext()
        {
            nextEventIn = mode == Mode.Burst
                ? Range(burstCalm.x, burstCalm.y)
                : Range(dipInterval.x, dipInterval.y);
        }

        private float Range(float min, float max) => min + (float)rng.NextDouble() * (max - min);
    }
}
