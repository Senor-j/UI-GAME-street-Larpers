using System;
using System.Collections.Generic;
using StreetLarpers.Instituto.Audio;
using StreetLarpers.Instituto.Core;
using UnityEngine;

namespace StreetLarpers.Instituto.Interaction
{
    /// <summary>
    /// El glifo de interacción del juego: 12 marcas de esfera de reloj con el hueco del "11"
    /// y una aguja que descansa en las 11:29. Rodea un rectángulo (folio, puerta, taquilla...)
    /// adaptando su forma a un "estadio" (rectángulo redondeado) sin deformar las marcas.
    /// </summary>
    /// <remarks>
    /// Convención de espacio: el aro se dibuja en el plano XY local; el espectador mira hacia +Z local,
    /// +X es su derecha y +Y arriba. Se coloca moviendo su propio localPosition (ver <see cref="MoveTo"/>),
    /// así que su padre define el plano (p. ej. el tablón del menú).
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AroHorario : MonoBehaviour
    {
        public enum State
        {
            Hidden,  // No se ve.
            Latent,  // Solo 4 marcas tenues: hay algo interactuable cerca.
            Focused, // Aro completo: lo estás mirando y puedes interactuar.
            Hold,    // La aguja barre la esfera según HoldProgress (mantener pulsado).
            Locked,  // La aguja vuelve a las 11:29 y el aro tiembla: no puedes.
            Danger   // La aguja gira al revés. Recurso narrativo, usar muy poco.
        }

        private const int TickCount = 12;
        private const int GapIndex = 11; // La marca de las "11" no existe. Nunca.

        [Header("Forma (metros)")]
        [SerializeField, Min(0f)] private float padding = 0.025f;
        [SerializeField, Min(0.001f)] private float tickLength = 0.022f;
        [SerializeField, Min(0.001f)] private float tickWidth = 0.006f;
        [SerializeField, Min(0.001f)] private float needleLength = 0.05f;
        [SerializeField, Min(0.001f)] private float needleWidth = 0.007f;
        [Tooltip("Variación fija de longitud de cada marca: aspecto de plantilla + rotulador.")]
        [SerializeField, Range(0f, 0.5f)] private float handDrawnJitter = 0.15f;
        [SerializeField] private int jitterSeed = 1129;

        [Header("Movimiento")]
        [Tooltip("Rotación lenta permanente (grados/segundo).")]
        [SerializeField] private float rotationSpeed = 2.5f;
        [SerializeField] private float springStiffness = 320f;
        [SerializeField] private float springDamping = 24f;
        [SerializeField] private float revealDuration = 0.18f;
        [SerializeField] private float lockedTremble = 0.002f;

        [Header("Aspecto")]
        [SerializeField] private Color color = new Color(0.91f, 0.894f, 0.847f, 1f);
        [SerializeField, Range(0f, 1f)] private float latentAlpha = 0.2f;
        [SerializeField, Range(0f, 1f)] private float holdPendingAlpha = 0.35f;

        [Header("Sonido")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Vacío = se genera un tic provisional por código.")]
        [SerializeField] private AudioClip tickClip;

        /// <summary>Se dispara con cada "tic" (al llegar a un objetivo, al cerrarse el aro...).</summary>
        public event Action Ticked;

        public State CurrentState { get; private set; } = State.Hidden;
        public float HoldProgress { get; private set; }

        private readonly List<Vector3> vertices = new List<Vector3>(64);
        private readonly List<Color> colors = new List<Color>(64);
        private readonly List<int> triangles = new List<int>(96);
        private readonly float[] tickJitter = new float[TickCount];

        private Mesh mesh;
        private MeshRenderer meshRenderer;

        private Vector3 position, positionVelocity, targetPosition;
        private Vector2 size = new Vector2(0.2f, 0.2f), sizeVelocity, targetSize = new Vector2(0.2f, 0.2f);
        private bool travelling;
        private float rotation01;
        private float needle01 = InstitutoClock.FrozenMinute01;
        private float reveal = 1f;
        private float alpha;
        private float tickKick;

        private void Awake()
        {
            mesh = new Mesh { name = "AroHorario" };
            mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = mesh;

            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            if (meshRenderer.sharedMaterial == null)
            {
                var shader = Shader.Find("StreetLarpers/Glifo");
                if (shader != null) meshRenderer.sharedMaterial = new Material(shader);
            }

            var rng = new System.Random(jitterSeed);
            for (int i = 0; i < TickCount; i++)
                tickJitter[i] = ((float)rng.NextDouble() * 2f - 1f) * handDrawnJitter;

            if (tickClip == null) tickClip = ProceduralSfx.CreateTick();

            position = targetPosition = transform.localPosition;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        /// <summary>Lleva el aro a rodear un rectángulo centrado en <paramref name="localPosition"/> (espacio del padre).</summary>
        public void MoveTo(Vector3 localPosition, Vector2 rectSize, bool instant = false)
        {
            targetPosition = localPosition;
            targetSize = rectSize;

            if (instant)
            {
                position = targetPosition;
                size = targetSize;
                positionVelocity = Vector3.zero;
                sizeVelocity = Vector2.zero;
                travelling = false;
                transform.localPosition = position;
                return;
            }

            travelling = true;
        }

        public void SetState(State state)
        {
            if (state == CurrentState) return;

            bool wasClosed = CurrentState == State.Hidden || CurrentState == State.Latent;
            CurrentState = state;

            if (wasClosed && state != State.Hidden && state != State.Latent)
            {
                reveal = 0f; // El aro "se cierra" marca a marca y termina con un tic.
            }
            if (state == State.Hold) HoldProgress = 0f;
        }

        public void SetHoldProgress(float progress) => HoldProgress = Mathf.Clamp01(progress);

        /// <summary>Tic de segundero: la aguja salta un minuto y vuelve, y suena el reloj.</summary>
        public void Tick()
        {
            tickKick = 1f;
            if (audioSource != null && tickClip != null) audioSource.PlayOneShot(tickClip);
            Ticked?.Invoke();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            UpdateSpring(dt);
            UpdateNeedle(dt);

            rotation01 = Mathf.Repeat(rotation01 + rotationSpeed / 360f * dt, 1f);
            tickKick = Mathf.MoveTowards(tickKick, 0f, dt * 8f);

            float targetAlpha = CurrentState == State.Hidden ? 0f : CurrentState == State.Latent ? latentAlpha : 1f;
            alpha = Mathf.MoveTowards(alpha, targetAlpha, dt * 6f);

            if (CurrentState != State.Hidden && CurrentState != State.Latent && reveal < 1f)
            {
                reveal = Mathf.MoveTowards(reveal, 1f, dt / Mathf.Max(0.01f, revealDuration));
                if (reveal >= 1f) Tick();
            }

            bool visible = alpha > 0.001f;
            meshRenderer.enabled = visible;
            if (visible) Rebuild();
        }

        private void UpdateSpring(float dt)
        {
            // Muelle ligeramente subamortiguado: un pequeño overshoot al llegar, nada exagerado.
            positionVelocity += (targetPosition - position) * (springStiffness * dt);
            positionVelocity *= Mathf.Exp(-springDamping * dt);
            position += positionVelocity * dt;

            sizeVelocity += (targetSize - size) * (springStiffness * dt);
            sizeVelocity *= Mathf.Exp(-springDamping * dt);
            size += sizeVelocity * dt;

            Vector3 tremble = Vector3.zero;
            if (CurrentState == State.Locked)
                tremble = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f) * lockedTremble;

            transform.localPosition = position + tremble;

            if (travelling && (targetPosition - position).sqrMagnitude < 1e-6f && positionVelocity.sqrMagnitude < 1e-4f)
            {
                travelling = false;
                Tick();
            }
        }

        private void UpdateNeedle(float dt)
        {
            switch (CurrentState)
            {
                case State.Hold:
                    needle01 = HoldProgress;
                    break;
                case State.Danger:
                    needle01 = Mathf.Repeat(needle01 - dt * 0.25f, 1f);
                    break;
                default:
                    // Vuelve a las 11:29 por el camino más corto de la esfera.
                    float delta = Mathf.Repeat(InstitutoClock.FrozenMinute01 - needle01 + 0.5f, 1f) - 0.5f;
                    needle01 = Mathf.Repeat(needle01 + delta * (1f - Mathf.Exp(-10f * dt)), 1f);
                    break;
            }
        }

        private void Rebuild()
        {
            vertices.Clear();
            colors.Clear();
            triangles.Clear();

            float halfWidth = size.x * 0.5f + padding;
            float halfHeight = size.y * 0.5f + padding;

            for (int i = 0; i < TickCount; i++)
            {
                if (i == GapIndex) continue;

                float tickAlpha = alpha;
                if (CurrentState == State.Latent && i % 3 != 0) continue;
                if (i / (float)TickCount > reveal) continue;
                if (CurrentState == State.Hold && i / (float)TickCount > HoldProgress) tickAlpha *= holdPendingAlpha;

                Perimeter(rotation01 + i / (float)TickCount, halfWidth, halfHeight, out var point, out var normal);
                AddQuad(point, normal, tickLength * (1f + tickJitter[i]), tickWidth, tickAlpha);
            }

            if (CurrentState != State.Latent)
            {
                float needleT = rotation01 + needle01 + tickKick / 60f;
                Perimeter(needleT, halfWidth, halfHeight, out var point, out var normal);
                // La aguja sobresale poco hacia fuera y entra hacia el objeto: "señala" lo seleccionado.
                AddQuad(point - normal * (needleLength * 0.2f), normal, needleLength, needleWidth, alpha);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void AddQuad(Vector2 center, Vector2 direction, float length, float width, float quadAlpha)
        {
            Vector2 along = direction * (length * 0.5f);
            Vector2 across = new Vector2(-direction.y, direction.x) * (width * 0.5f);
            int start = vertices.Count;

            vertices.Add(center - along - across);
            vertices.Add(center - along + across);
            vertices.Add(center + along + across);
            vertices.Add(center + along - across);

            var c = new Color(color.r, color.g, color.b, color.a * quadAlpha);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);

            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        /// <summary>
        /// Punto y normal exterior de un rectángulo redondeado (estadio) recorrido en sentido horario
        /// desde "las 12". t en [0, 1). Con ancho == alto es un círculo: una esfera de reloj.
        /// </summary>
        public static void Perimeter(float t, float halfWidth, float halfHeight, out Vector2 point, out Vector2 normal)
        {
            float radius = Mathf.Max(1e-4f, Mathf.Min(halfWidth, halfHeight));
            float sx = Mathf.Max(0f, halfWidth - radius);
            float sy = Mathf.Max(0f, halfHeight - radius);
            float arc = 0.5f * Mathf.PI * radius;
            float total = 4f * sx + 4f * sy + 4f * arc;
            float d = Mathf.Repeat(t, 1f) * total;

            // 1. Mitad derecha del lado superior.
            if (d < sx) { point = new Vector2(d, halfHeight); normal = Vector2.up; return; }
            d -= sx;
            // 2. Esquina superior derecha.
            if (d < arc) { Corner(sx, sy, radius, 0.5f * Mathf.PI - d / radius, out point, out normal); return; }
            d -= arc;
            // 3. Lado derecho (bajando).
            if (d < 2f * sy) { point = new Vector2(halfWidth, sy - d); normal = Vector2.right; return; }
            d -= 2f * sy;
            // 4. Esquina inferior derecha.
            if (d < arc) { Corner(sx, -sy, radius, -d / radius, out point, out normal); return; }
            d -= arc;
            // 5. Lado inferior (hacia la izquierda).
            if (d < 2f * sx) { point = new Vector2(sx - d, -halfHeight); normal = Vector2.down; return; }
            d -= 2f * sx;
            // 6. Esquina inferior izquierda.
            if (d < arc) { Corner(-sx, -sy, radius, -0.5f * Mathf.PI - d / radius, out point, out normal); return; }
            d -= arc;
            // 7. Lado izquierdo (subiendo).
            if (d < 2f * sy) { point = new Vector2(-halfWidth, -sy + d); normal = Vector2.left; return; }
            d -= 2f * sy;
            // 8. Esquina superior izquierda.
            if (d < arc) { Corner(-sx, sy, radius, -Mathf.PI - d / radius, out point, out normal); return; }
            d -= arc;
            // 9. Mitad izquierda del lado superior.
            point = new Vector2(-sx + d, halfHeight);
            normal = Vector2.up;
        }

        private static void Corner(float cx, float cy, float radius, float angle, out Vector2 point, out Vector2 normal)
        {
            normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            point = new Vector2(cx, cy) + normal * radius;
        }
    }
}
