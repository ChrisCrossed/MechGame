using UnityEngine;

using UnityEngine.InputSystem;

public struct PlayerInputObject
{
    public Vector2 MoveVector { get; internal set; }
    public bool Jump { get; internal set; }

    internal InputAction IA_MoveVector;
    internal InputAction IA_Jump;

    // Note To Self - I believe this is *not* supposed to be static since Static requires a return type so it's used like a formula
    public void Init()
    {
        IA_MoveVector = InputSystem.actions.FindAction("Move");
        IA_Jump = InputSystem.actions.FindAction("Jump");
    }

    internal void InputUpdate()
    {

        MoveVector = IA_MoveVector.ReadValue<Vector2>();
        // MoveVector = Vector2.Normalize(MoveVector);

        Jump = IA_Jump.IsPressed();
    }

    internal string PrintTest()
    {
        return "Hi Chris";
    }
}

public class c_PlayerInput : MonoBehaviour
{
    PlayerInputObject inputObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputObject = new PlayerInputObject();

        inputObject.Init();
    }

    // Update is called once per frame
    void Update()
    {
        inputObject.InputUpdate();

        print(inputObject.MoveVector);
    }

    public Vector2 GetInputVector()
    {
        Vector2 returnVector = new Vector2();



        return returnVector;
    }
}
