using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace StreetLarpers.HUD.Sandbox
{
    /// <summary>
    /// [SOLO SANDBOX] Atajos de teclado para provocar situaciones y ver cómo responde el HUD.
    /// Provisional: más adelante se complementará con un panel de debug en pantalla.
    /// </summary>
    public class HudDebugKeyboard : MonoBehaviour
    {
        [SerializeField] private SimulatedFighter player1;
        [SerializeField] private SimulatedFighter player2;

        [Header("Amounts")]
        [SerializeField, Min(0f)] private float lightDamage = 50f;
        [SerializeField, Min(0f)] private float heavyDamage = 250f;
        [SerializeField, Min(0f)] private float healAmount = 100f;

        private const string Help =
            "[HUD Sandbox] Controles:\n" +
            "  P1 → 1: daño | 2: curar | 3: KO\n" +
            "  P2 → 8: daño | 9: curar | 0: KO\n" +
            "  Mantener Shift: daño fuerte\n" +
            "  R: reiniciar vida de ambos";

        private void Start() => Debug.Log(Help, this);

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            float damage = keyboard.shiftKey.isPressed ? heavyDamage : lightDamage;

            HandleFighter(player1, keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key, damage);
            HandleFighter(player2, keyboard.digit8Key, keyboard.digit9Key, keyboard.digit0Key, damage);

            if (keyboard.rKey.wasPressedThisFrame)
            {
                if (player1 != null) player1.ResetHealth();
                if (player2 != null) player2.ResetHealth();
            }
        }

        private void HandleFighter(SimulatedFighter fighter, KeyControl damageKey, KeyControl healKey,
            KeyControl koKey, float damage)
        {
            if (fighter == null) return;
            if (damageKey.wasPressedThisFrame) fighter.ApplyDamage(damage);
            if (healKey.wasPressedThisFrame) fighter.Heal(healAmount);
            if (koKey.wasPressedThisFrame) fighter.SetHealth(0f);
        }
    }
}
