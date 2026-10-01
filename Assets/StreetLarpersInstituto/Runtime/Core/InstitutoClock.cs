using UnityEngine;

namespace StreetLarpers.Instituto.Core
{
    /// <summary>
    /// "El instituto está atrapado en el mismo momento": única fuente de verdad de la hora congelada
    /// y del "pulso del timbre" que comparten todas las luces del juego.
    /// </summary>
    /// <remarks>
    /// Si se cambia la hora aquí, cambian a la vez relojes, pantallas, el Aro Horario y los carteles
    /// generados por código. Nunca escribir "11:29" a mano en un script.
    /// </remarks>
    public static class InstitutoClock
    {
        public const int FrozenHour = 11;
        public const int FrozenMinute = 29;
        public const string FrozenTimeText = "11:29";
        public const string FrozenDateText = "Jueves, 14 de marzo";

        /// <summary>Cada cuántos segundos "casi suena" el timbre.</summary>
        public const float BellPeriod = 60f;

        /// <summary>Cuánto bajan todas las luces durante el frame del timbre (5 %).</summary>
        public const float BellDip = 0.05f;

        /// <summary>Posición de la aguja en la esfera (0 = las 12, 0.5 = las 6).</summary>
        public static float FrozenMinute01 => FrozenMinute / 60f;

        /// <summary>Ángulo (grados, sentido horario desde las 12) de la aguja de las horas.</summary>
        public static float HourHandAngle => (FrozenHour % 12 + FrozenMinute / 60f) * 30f;

        /// <summary>Ángulo (grados, sentido horario desde las 12) del minutero.</summary>
        public static float MinuteHandAngle => FrozenMinute * 6f;

        private static int s_cachedFrame = -1;
        private static int s_lastPeriod = -1;
        private static float s_cachedMultiplier = 1f;

        /// <summary>
        /// 1 casi siempre; (1 - BellDip) durante exactamente un frame cada <see cref="BellPeriod"/> segundos.
        /// Todas las luces que lo consultan en el mismo frame reciben el mismo valor, así que bajan a la vez.
        /// </summary>
        public static float BellMultiplier
        {
            get
            {
                int frame = Time.frameCount;
                if (frame == s_cachedFrame) return s_cachedMultiplier;

                s_cachedFrame = frame;
                int period = Mathf.FloorToInt(Time.unscaledTime / BellPeriod);
                s_cachedMultiplier = s_lastPeriod >= 0 && period != s_lastPeriod ? 1f - BellDip : 1f;
                s_lastPeriod = period;
                return s_cachedMultiplier;
            }
        }

        // Necesario si "Enter Play Mode Options" desactiva el domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_cachedFrame = -1;
            s_lastPeriod = -1;
            s_cachedMultiplier = 1f;
        }
    }
}
