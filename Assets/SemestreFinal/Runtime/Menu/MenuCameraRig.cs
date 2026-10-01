using UnityEngine;

namespace SemestreFinal.Menu
{
    /// <summary>
    /// "Cabeza" del jugador en el menú: cámara a la altura de los ojos que gira o da unos pasos
    /// hacia el destino físico de cada opción, con una respiración mínima para sentir presencia.
    /// </summary>
    /// <remarks>
    /// Jerarquía esperada: este componente en un objeto raíz ("PlayerHead") y la cámara como hijo
    /// (<see cref="swayTarget"/>), para que el balanceo no interfiera con las transiciones.
    /// Si más adelante se usa Cinemachine, este componente se puede sustituir sin tocar el menú.
    /// </remarks>
    [DisallowMultipleComponent]
    public class MenuCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform swayTarget;
        [SerializeField, Min(0.05f)] private float blendDuration = 1.0f;

        [Header("Respiración (casi imperceptible)")]
        [SerializeField] private float swayPositionAmplitude = 0.005f;
        [SerializeField] private float swayRotationAmplitude = 0.2f;
        [SerializeField] private float swayFrequency = 0.25f;

        public bool IsBlending { get; private set; }

        private Pose home, from, to;
        private float blendT = 1f;

        private void Awake() => home = new Pose(transform.position, transform.rotation);

        public void BlendTo(Transform target)
        {
            if (target != null) BlendTo(new Pose(target.position, target.rotation));
        }

        public void BlendTo(Pose target)
        {
            from = new Pose(transform.position, transform.rotation);
            to = target;
            blendT = 0f;
            IsBlending = true;
        }

        public void BlendHome() => BlendTo(home);

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (IsBlending)
            {
                blendT = Mathf.Clamp01(blendT + dt / blendDuration);
                float e = blendT * blendT * (3f - 2f * blendT); // ease in-out
                transform.SetPositionAndRotation(
                    Vector3.Lerp(from.position, to.position, e),
                    Quaternion.Slerp(from.rotation, to.rotation, e));
                if (blendT >= 1f) IsBlending = false;
            }

            if (swayTarget != null)
            {
                float t = Time.unscaledTime * swayFrequency;
                float nx = Mathf.PerlinNoise(t, 0.31f) - 0.5f;
                float ny = Mathf.PerlinNoise(0.73f, t) - 0.5f;
                float nr = Mathf.PerlinNoise(t * 0.7f, 5.1f) - 0.5f;
                swayTarget.localPosition = new Vector3(nx, ny, 0f) * (2f * swayPositionAmplitude);
                swayTarget.localRotation = Quaternion.Euler(ny * 2f * swayRotationAmplitude, nr * 2f * swayRotationAmplitude, 0f);
            }
        }
    }
}
