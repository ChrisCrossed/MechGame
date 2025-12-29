using UnityEngine;

using UnityEngine.InputSystem;

public enum JumpJetButtonState
{
    Pressed,
    Held,
    Released,
    Off
}

public struct PlayerInputObject
{
    


    public Vector2 MoveVector { get; internal set; }
    public bool Jump { get; internal set; }
    public bool JumpJetState { get; internal set; }
    public Vector2 LookVector { get; internal set; }

    public bool QuitButton { get; internal set; }
    
    public bool TestContext_1 { get; internal set; }

    internal InputAction IA_MoveVector;
    internal InputAction IA_Jump;
    internal InputAction IA_Look;
    internal InputAction IA_Quit;
    internal InputAction IA_JumpJet;
    internal InputAction IA_TestContext_1;

    public void Init()
    {
        IA_MoveVector = InputSystem.actions.FindAction("Move");
        IA_Jump = InputSystem.actions.FindAction("Jump");
        IA_JumpJet = InputSystem.actions.FindAction("JumpJet");
        IA_Look = InputSystem.actions.FindAction("Look");
        IA_Quit = InputSystem.actions.FindAction("Quit");
        IA_TestContext_1 = InputSystem.actions.FindAction("TestContext_1");
    }

    internal void InputUpdate()
    {
        MoveVector = IA_MoveVector.ReadValue<Vector2>();

        Jump = IA_Jump.IsPressed();
        JumpJetState = IA_JumpJet.IsPressed();

        // TODO: Run If-check when system is not using a mouse for the ReadValue
        LookVector = IA_Look.ReadValue<Vector2>();
        // LookVector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        QuitButton = IA_Quit.IsPressed();

        TestContext_1 = IA_TestContext_1.IsPressed();
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
    }

    public Vector2 GetInputVector()
    {
        return inputObject.MoveVector;
    }

    bool WasPressed_Jump;
    bool IsPressed_Jump;
    public bool GetJumpButton()
    {
        IsPressed_Jump = false;

        if(inputObject.Jump && !WasPressed_Jump)
            IsPressed_Jump = true;

        WasPressed_Jump = inputObject.Jump;

        return IsPressed_Jump;
    }

    bool WasPressed_JumpJet;
    bool IsPressed_JumpJet;
    public JumpJetButtonState JumpJetState()
    {
        JumpJetButtonState state = new JumpJetButtonState();

        IsPressed_Jump = inputObject.JumpJetState;

        // Logic
        if(IsPressed_Jump)
        {
            if (WasPressed_Jump)
                state = JumpJetButtonState.Held;
            else
                state = JumpJetButtonState.Pressed;
        }
        else
        {
            if (WasPressed_Jump)
                state = JumpJetButtonState.Released;
            else
                state = JumpJetButtonState.Off;
        }

        WasPressed_Jump = IsPressed_Jump;

        return state;
    }

    public Vector2 GetLookVector()
    {
        return inputObject.LookVector;
    }

    bool WasPressed_Quit;
    bool IsPressed_Quit;
    public bool QuitButton()
    {
        IsPressed_Quit = false;

        if (inputObject.QuitButton && !WasPressed_Quit)
            IsPressed_Quit = true;

        WasPressed_Quit = inputObject.QuitButton;

        return IsPressed_Quit;
    }

    bool WasPressed_TestContext_1;
    bool IsPressed_TestContext_1;
    public bool TestContext_1()
    {
        IsPressed_TestContext_1 = false;

        if(inputObject.TestContext_1 && !WasPressed_TestContext_1)
            IsPressed_TestContext_1 = true;

        WasPressed_TestContext_1 = inputObject.TestContext_1;

        return IsPressed_TestContext_1;
    }
}
