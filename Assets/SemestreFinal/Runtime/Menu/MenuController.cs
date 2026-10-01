using System;
using System.Collections;
using System.Collections.Generic;
using SemestreFinal.Audio;
using SemestreFinal.Interaction;
using SemestreFinal.Lighting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SemestreFinal.Menu
{
    /// <summary>
    /// Navegación del menú diegético: teclado, mando y ratón. Mueve el Aro Horario entre opciones,
    /// confirma y gira la cabeza del jugador hacia el destino físico de cada una.
    /// </summary>
    /// <remarks>
    /// No sabe qué hace cada opción en el juego (lobby, ajustes...): lo comunica con <see cref="OptionConfirmed"/>
    /// y con el UnityEvent de cada <see cref="MenuOption"/>. Solo resuelve "Salir" por sí mismo.
    /// </remarks>
    [DisallowMultipleComponent]
    public class MenuController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private List<MenuOption> options = new List<MenuOption>();
        [SerializeField] private AroHorario aro;
        [SerializeField] private MenuCameraRig cameraRig;
        [Tooltip("Cámara para el ratón. Vacío = Camera.main.")]
        [SerializeField] private Camera pointerCamera;

        [Header("Sonido")]
        [SerializeField] private AudioSource sfxSource;
        [Tooltip("Vacío = chincheta provisional generada por código.")]
        [SerializeField] private AudioClip confirmClip;

        [Header("Salir")]
        [Tooltip("Fluorescente que se apaga al salir del juego.")]
        [SerializeField] private FluorescentFlicker exitLight;
        [SerializeField] private float quitDelay = 0.9f;

        [Header("Mando: repetición al mantener el stick")]
        [SerializeField] private float repeatDelay = 0.35f;
        [SerializeField] private float repeatRate = 0.12f;

        /// <summary>Se dispara al confirmar cualquier opción (después de su propio UnityEvent).</summary>
        public event Action<MenuOption> OptionConfirmed;

        public MenuOption Selected => index >= 0 && index < options.Count ? options[index] : null;

        private int index = -1;
        private bool atDestination;
        private bool quitting;
        private int heldDirection;
        private float repeatTimer;
        private Vector2 lastPointer;

        private void Start()
        {
            if (pointerCamera == null) pointerCamera = Camera.main;
            if (confirmClip == null) confirmClip = ProceduralSfx.CreatePinPull();

            for (int i = 0; i < options.Count; i++)
                options[i].SetSelected(false, instant: true);

            if (options.Count > 0) Select(0, instant: true);
            if (aro != null) aro.SetState(AroHorario.State.Focused);
        }

        private void Update()
        {
            if (quitting || options.Count == 0) return;
            if (cameraRig != null && cameraRig.IsBlending) return;

            if (atDestination)
            {
                if (CancelPressed()) ReturnHome();
                return;
            }

            int direction = ReadNavigation();
            if (direction != 0) Select((index + direction + options.Count) % options.Count);

            HandlePointer();

            if (SubmitPressed()) Confirm(options[index]);
        }

        public void Select(int newIndex, bool instant = false)
        {
            if (newIndex == index || newIndex < 0 || newIndex >= options.Count) return;

            if (Selected != null) Selected.SetSelected(false);
            index = newIndex;
            Selected.SetSelected(true, instant);

            if (aro != null) aro.MoveTo(Selected.GetAroAnchor(aro.transform.parent), Selected.Size, instant);
        }

        public void Confirm(MenuOption option)
        {
            option.Confirm();
            if (sfxSource != null && confirmClip != null) sfxSource.PlayOneShot(confirmClip);
            OptionConfirmed?.Invoke(option);

            if (option.OptionKind == MenuOption.Kind.Quit)
            {
                StartCoroutine(QuitRoutine(option));
                return;
            }

            if (option.CameraDestination != null && cameraRig != null)
            {
                atDestination = true;
                if (aro != null) aro.SetState(AroHorario.State.Hidden);
                cameraRig.BlendTo(option.CameraDestination);
            }
        }

        /// <summary>Vuelve a mirar el tablón (Escape / B / clic derecho, o llamado desde un submenú).</summary>
        public void ReturnHome()
        {
            atDestination = false;
            if (cameraRig != null) cameraRig.BlendHome();
            if (aro != null) aro.SetState(AroHorario.State.Focused);
        }

        private IEnumerator QuitRoutine(MenuOption option)
        {
            quitting = true;
            if (aro != null) aro.SetState(AroHorario.State.Hidden);
            if (option.CameraDestination != null && cameraRig != null) cameraRig.BlendTo(option.CameraDestination);

            yield return new WaitForSecondsRealtime(quitDelay * 0.6f);
            if (exitLight != null) exitLight.SetMode(FluorescentFlicker.Mode.Dead);
            yield return new WaitForSecondsRealtime(quitDelay * 0.4f);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------- Entrada ----------------

        private int ReadNavigation()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) return -1;
                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) return 1;
            }

            var gamepad = Gamepad.current;
            if (gamepad == null) return 0;

            if (gamepad.dpad.up.wasPressedThisFrame) return -1;
            if (gamepad.dpad.down.wasPressedThisFrame) return 1;

            // Stick: un paso al inclinar y repetición si se mantiene.
            float y = gamepad.leftStick.ReadValue().y;
            int held = y > 0.5f ? -1 : y < -0.5f ? 1 : 0;
            if (held == 0)
            {
                heldDirection = 0;
                return 0;
            }
            if (held != heldDirection)
            {
                heldDirection = held;
                repeatTimer = repeatDelay;
                return held;
            }
            repeatTimer -= Time.unscaledDeltaTime;
            if (repeatTimer > 0f) return 0;
            repeatTimer = repeatRate;
            return held;
        }

        private static bool SubmitPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                return true;

            var gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        }

        private static bool CancelPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame))
                return true;

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame) return true;

            var gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
        }

        private void HandlePointer()
        {
            var mouse = Mouse.current;
            if (mouse == null || pointerCamera == null) return;

            Vector2 pointer = mouse.position.ReadValue();
            bool moved = (pointer - lastPointer).sqrMagnitude > 1f;
            bool clicked = mouse.leftButton.wasPressedThisFrame;
            lastPointer = pointer;

            // Si el ratón no se mueve, no "roba" la selección al teclado o al mando.
            if (!moved && !clicked) return;

            if (!Physics.Raycast(pointerCamera.ScreenPointToRay(pointer), out var hit, 10f)) return;

            var option = hit.collider.GetComponentInParent<MenuOption>();
            int hitIndex = option != null ? options.IndexOf(option) : -1;
            if (hitIndex < 0) return;

            Select(hitIndex);
            if (clicked) Confirm(option);
        }
    }
}
