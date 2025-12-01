using System.Diagnostics;
using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public struct PlayerInputObject
{
    public Vector2 MoveVector { get; internal set; }
    public bool Jump {  get; internal set; }

    internal InputAction IA_MoveVector;
    internal InputAction IA_Jump;

    // Note To Self - I believe this is *not* supposed to be static since Static requires a return type so it's used like a formula
    public void Init()
    {
        IA_MoveVector = InputSystem.actions.FindAction("WASD");
        IA_Jump = InputSystem.actions.FindAction("Jump");
    }

    internal void InputUpdate()
    {
        
        // MoveVector = IA_MoveVector.ReadValue<Vector2>();
        // MoveVector = Vector2.Normalize(MoveVector);

        Jump = IA_Jump.IsPressed();
    }

    internal string PrintTest()
    {
        return "Hi Chris";
    }
}

