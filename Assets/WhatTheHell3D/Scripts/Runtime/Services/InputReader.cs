using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputReader
{
    private InputAction move;
    private InputAction look;
    private InputAction attack;
    private InputAction interact;
    private InputAction jump;
    private InputAction sprint;
    private InputAction guard;
    private InputAction dodge;
    private InputAction lockOn;
    private InputAction pause;

    public void Configure(InputActionAsset asset)
    {
        InputActionMap map = asset == null ? null : asset.FindActionMap("Player", false);
        if (map == null)
        {
            return;
        }

        move = map.FindAction("Move", false);
        look = map.FindAction("Look", false);
        attack = map.FindAction("Attack", false);
        interact = map.FindAction("Interact", false);
        jump = map.FindAction("Jump", false);
        sprint = map.FindAction("Sprint", false);
        guard = map.FindAction("Guard", false);
        dodge = map.FindAction("Dodge", false);
        lockOn = map.FindAction("LockOn", false);
        pause = map.FindAction("Pause", false);
        map.Enable();
    }

    public Vector2 Move
    {
        get
        {
            if (move != null)
            {
                return move.ReadValue<Vector2>();
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            return new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
        }
    }

    public Vector2 Look
    {
        get
        {
            if (look != null)
            {
                return look.ReadValue<Vector2>();
            }

            return Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue();
        }
    }

    public bool AttackHeld => IsPressed(attack, MouseButtonPressed(0));
    public bool AttackPressed => WasPressed(attack, MouseButtonPressed(0));
    public bool InteractPressed => WasPressed(interact, KeyPressed(Key.E));
    public bool JumpPressed => WasPressed(jump, KeyPressed(Key.Space));
    public bool SprintHeld => IsPressed(sprint, KeyPressed(Key.LeftShift));
    public bool GuardHeld => IsPressed(guard, MouseButtonPressed(1));
    public bool DodgePressed => WasPressed(dodge, KeyPressed(Key.Q));
    public bool LockOnPressed => WasPressed(lockOn, MouseButtonPressed(2));
    public bool PausePressed => WasPressed(pause, KeyPressed(Key.Escape));

    private static bool KeyPressed(Key key)
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[key].isPressed;
    }

    private static bool MouseButtonPressed(int button)
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        return button switch
        {
            0 => mouse.leftButton.isPressed,
            1 => mouse.rightButton.isPressed,
            2 => mouse.middleButton.isPressed,
            _ => false
        };
    }

    private static bool IsPressed(InputAction action, bool fallback)
    {
        return action != null ? action.IsPressed() : fallback;
    }

    private static bool WasPressed(InputAction action, bool fallback)
    {
        return action != null ? action.WasPressedThisFrame() : fallback;
    }
}
