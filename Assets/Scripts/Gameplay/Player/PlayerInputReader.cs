using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceExplorer
{
    /// <summary>
    /// Translates raw device input into gameplay intent (move direction, fire).
    /// Other player components read from here and never touch the Input System directly.
    /// Controls: Arrow keys / WASD to move, Space to shoot (gamepad stick + South button too).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour
    {
        InputAction move;
        InputAction fire;

        /// <summary>Desired movement direction, length clamped to 1.</summary>
        public Vector2 Move => Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);

        /// <summary>True while the fire button is held.</summary>
        public bool IsFiring => fire.IsPressed();

        void Awake()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            fire = new InputAction("Fire", InputActionType.Button, "<Keyboard>/space");
            fire.AddBinding("<Gamepad>/buttonSouth");
        }

        void OnEnable()
        {
            move.Enable();
            fire.Enable();
        }

        void OnDisable()
        {
            move.Disable();
            fire.Disable();
        }

        void OnDestroy()
        {
            move.Dispose();
            fire.Dispose();
        }
    }
}
