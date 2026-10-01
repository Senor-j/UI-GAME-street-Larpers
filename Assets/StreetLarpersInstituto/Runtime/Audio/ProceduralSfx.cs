using UnityEngine;

namespace StreetLarpers.Instituto.Audio
{
    /// <summary>
    /// Sonidos provisionales generados por código para poder probar el menú sin assets de audio.
    /// Sustituir por grabaciones reales (reloj de pared, chincheta) asignándolas en el Inspector.
    /// </summary>
    public static class ProceduralSfx
    {
        private const int SampleRate = 44100;

        /// <summary>"Tic" seco de segundero: ruido + tono agudo con caída muy rápida.</summary>
        public static AudioClip CreateTick() => Create("Tic (procedural)", 0.035f, 2400f, 0.5f, 9f, 0.6f);

        /// <summary>Chincheta arrancada del fieltro: golpe grave y algo más largo.</summary>
        public static AudioClip CreatePinPull() => Create("Chincheta (procedural)", 0.12f, 180f, 0.65f, 6f, 0.7f);

        private static AudioClip Create(string name, float seconds, float toneHz, float noiseMix, float decay, float gain)
        {
            int samples = Mathf.CeilToInt(seconds * SampleRate);
            var data = new float[samples];
            var rng = new System.Random(1129);

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float envelope = Mathf.Exp(-t * decay);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float tone = Mathf.Sin(2f * Mathf.PI * toneHz * i / SampleRate);
                data[i] = (noise * noiseMix + tone * (1f - noiseMix)) * envelope * gain;
            }

            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
