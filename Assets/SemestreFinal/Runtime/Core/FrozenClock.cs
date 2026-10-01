using TMPro;
using UnityEngine;

namespace SemestreFinal.Core
{
    /// <summary>
    /// Reloj (analógico y/o digital) que siempre marca <see cref="InstitutoClock.FrozenTimeText"/>.
    /// </summary>
    /// <remarks>
    /// Convención de la esfera: el espectador mira hacia +Z local, +X es su derecha y +Y arriba.
    /// Las agujas apuntan a +Y en reposo y giran sobre Z (sentido horario visto de frente = ángulo negativo).
    /// </remarks>
    [ExecuteAlways]
    public class FrozenClock : MonoBehaviour
    {
        [SerializeField] private Transform hourHand;
        [SerializeField] private Transform minuteHand;
        [Tooltip("Opcional: pantallas, TVs, proyectores, despertadores digitales...")]
        [SerializeField] private TMP_Text digitalDisplay;

        // OnEnable (y no OnValidate) porque modificar un TMP_Text dentro de OnValidate genera avisos.
        private void OnEnable() => Apply();

        private void Apply()
        {
            if (hourHand != null) hourHand.localRotation = Quaternion.Euler(0f, 0f, -InstitutoClock.HourHandAngle);
            if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0f, 0f, -InstitutoClock.MinuteHandAngle);
            if (digitalDisplay != null && digitalDisplay.text != InstitutoClock.FrozenTimeText)
                digitalDisplay.text = InstitutoClock.FrozenTimeText;
        }
    }
}
