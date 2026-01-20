using NUnit.Framework;
using System;
using System.Collections;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private c_PlayerInput PlayerInput;

    private Rigidbody this_Rigidbody;
    private GameObject this_CameraObject;
    private GameObject this_GroundRaycastPos;

    private bool PlayerJump;

    private CharacterController this_CharController;

    #region TestContext States
    private bool MouseCursorVisable;
    #endregion TestContext States

    [SerializeField]
    private float MoveSpeed;

    [SerializeField]
    private float HorizontalLookMultiplier;

    [SerializeField]
    private float VerticalLookMultiplier;

    GameObject MultiplayerMenuObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        INIT_PlayerInput();
        INIT_PlayerObjectComponents();

        ToggleMenu(MenuState);
    }

    bool MenuScreenActive = false;
    public void CoherenceLobbyCompleted()
    {
        MenuScreenActive = false;
    }

    IEnumerator CoherenceMenuLobbyScreenWait()
    {
        MenuScreenActive = true;

        while (!MenuState)
            yield return new WaitForEndOfFrame();

        CoherenceLobbyCompleted();

        yield return null;
    }

    bool MenuState;
    void ToggleMenu(bool _menuState)
    {
        print("Menu: " + _menuState);

        MultiplayerMenuObject.SetActive(_menuState);

        MouseCursorVisable = _menuState;
        ToggleCursorState(MouseCursorVisable);
        gameObject.transform.Find("Canvas").gameObject.SetActive(!_menuState);

        if(_menuState)
            StartCoroutine( CoherenceMenuLobbyScreenWait() );

        /*
        if(_menuState) // Enable Menu
        {
            
        }
        else // Disable Menu / Play Game
        {
            GameObject.Find("UI").gameObject.SetActive(false);

            MouseCursorVisable = false;
            ToggleCursorState(MouseCursorVisable);
            gameObject.transform.Find("Canvas").gameObject.SetActive(true);

            CoherenceLobbyCompleted();
        }
        */
    }

    #region Init Functions
    void INIT_PlayerInput()
    {
        PlayerInput = GameObject.Find("GameManager").GetComponent<c_PlayerInput>();

        // gameObject.transform.Find("Canvas").gameObject.SetActive(false);
    }

    void INIT_PlayerObjectComponents()
    {
        this_Rigidbody = gameObject.GetComponent<Rigidbody>();

        this_CharController = gameObject.GetComponent<CharacterController>();
        this_GroundRaycastPos = gameObject.transform.Find("GroundRaycastPos").gameObject;

        this_CameraObject = gameObject.transform.Find("Main Camera").gameObject;
        cameraAngle = this_CameraObject.transform.localEulerAngles.x;

        MultiplayerMenuObject = GameObject.Find("UI").gameObject;
    }

    #endregion Init Functions

    bool QuitPressed;
    float QuitTimer = 0f;
    // Update is called once per frame
    void Update()
    {
        #region Multiplayer Menu Toggle
        if (QuitPressed)
        {
            MenuState = !MenuState;

            ToggleMenu(MenuState);
        }

        if (MenuScreenActive)
            return;
        
        #endregion

        UpdateGroundState();
        UpdateJump();
        UpdateJumpJet();
        UpdateLook();
        UpdateMovement();
        UpdateQuitButton();

        #region Quit Button
        if (QuitPressed)
        {
            print("Pressed");
            if (QuitTimer > 0f)
            {
                Application.Quit();
                print("Quit");
            }
            else
                QuitTimer = 0.2f;
        }

        if(QuitTimer > 0f)
        {
            QuitTimer -= Time.deltaTime;
            if (QuitTimer < 0f)
                QuitTimer = 0f;
        }
        #endregion Quit Button

        #region Mouse Cursor
        if(PlayerInput.TestContext_1())
        {
            MouseCursorVisable = !MouseCursorVisable;
            ToggleCursorState(MouseCursorVisable);
        }

        #endregion Mouse Cursor
    }

    #region Update Functions

    bool b_JustTouchedGround;
    void UpdateGroundState()
    {
        if (b_JumpDelayActive)
            return;

        RaycastHit _hit;
        int layerMask = LayerMask.GetMask("Terrain");
        CapsuleCollider _collider = gameObject.GetComponent<CapsuleCollider>();

        OnGround = Physics.SphereCast(gameObject.transform.position, _collider.radius - 0.001f, Vector3.down, out _hit, _collider.radius + 0.25f, layerMask);

        // If last frame we were off the ground and just touched down,
        b_JustTouchedGround = false;
        if (!OnGround_OLD && OnGround)
            b_JustTouchedGround = true;

        if (OnGround)
        {
            v3_GroundNormal = _hit.normal;

            // float angle = Vector3.Angle(Vector3.up, _hit.normal);
            Vector3 v3_ProjectedVector = Vector3.ProjectOnPlane(gameObject.transform.forward, _hit.normal).normalized;
            Debug.DrawRay(_hit.point, v3_ProjectedVector * 10f, Color.blue);
        }
    }

    Vector3 v3_GroundNormal;
    private float JumpHeight = 1.5f;
    private float Gravity = -9.81f * 3.5f;
    private float yVel;
    private bool OnGround;
    private bool OnGround_OLD;
    void UpdateJump()
    {
        // If there player is on the ground and using Jetpack, reset vertical velocity to allow upward movement.
        if (OnGround && !JetpackActive)
        {
            yVel = 0f;

            if (PlayerInput.GetJumpButton())
            {
                yVel = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                StartCoroutine(JumpDelay());
                OnGround = false;
            }
        }
        else
        {
            if (!JetpackActive)
                yVel += Gravity * Time.deltaTime;
        }

        OnGround_OLD = OnGround;
    }

    bool JetpackActive = false;
    float JetpackMaxVertVelocity = 7f; // 4 Feels like Tribes 'Heavy' Velocity
    float JetpackArmorGravityInfluence = 10f;
    
    void UpdateJumpJet()
    {
        JumpJetButtonState state = PlayerInput.JumpJetState();

        switch (state)
        {
            case JumpJetButtonState.Pressed:
            case JumpJetButtonState.Held:

                JetpackActive = true;

                // Forcing initial velocity if player taps ground while using jetpack
                if (OnGround && yVel < 0f)
                    yVel = 0f;

                yVel += Gravity / JetpackArmorGravityInfluence * -1f * Time.deltaTime;

                if (yVel > JetpackMaxVertVelocity)
                    yVel = JetpackMaxVertVelocity;

                print("Vert Vel: " + (Gravity / JetpackArmorGravityInfluence * -1f * Time.deltaTime));
                break;
            case JumpJetButtonState.Released:
            case JumpJetButtonState.Off:
                JetpackActive = false;
                break;
            default:
                break;
        }
    }


    Vector2 v2_MovementVelocityPercentage;
    float MovementVelocityRate;
    Vector3 v3_LastFrameVelocity;
    float f_MovementVelocityPerc = 1.0f;
    float f_MoveSpeedPenaltyTimer = 0f;
    float f_MoveSpeedPenaltyPerc = 0.3f;
    float f_MoveSpeedPenaltyTimer_MAX = 0.1f;
    float f_JetpackAirborneVelocityModifier = 0.75f;
    Vector3 JetpackVector_Prior;
    float JetpackMagnitude_Prior;
    void UpdateMovement()
    {
        JetpackVector_Prior = this_CharController.velocity.normalized;
        JetpackMagnitude_Prior = this_CharController.velocity.magnitude;

        if (OnGround && b_JustTouchedGround)
        {
            // Only allow velocity to be reduced upon touchdown if moving over a certain speed
            if (GetMoveSpeed() < (f_JetpackAirborneVelocityModifier * MoveSpeed) + 0.05f)
                b_JustTouchedGround = false;
        }

        #region Apply limits to momentum when first touching the ground

        // WARNING: This increases the Maximum Velocity Percentage, but gets overwritten below if the player's move speed penalty is still being applied.
        if (f_MovementVelocityPerc < 1.0f)
        {
            f_MovementVelocityPerc += Time.deltaTime / f_MoveSpeedPenaltyTimer_MAX;

            if (f_MovementVelocityPerc > 1.0f)
                f_MovementVelocityPerc = 1.0f;
        }

        // When less than the Move Speed Penalty Timer is less than MAX, hard force the max move speed penalty
        if (f_MoveSpeedPenaltyTimer < f_MoveSpeedPenaltyTimer_MAX)
        {
            // Forces a move speed penalty
            f_MovementVelocityPerc = f_MoveSpeedPenaltyPerc;

            // Increase/Cap the Penalty Timer
            f_MoveSpeedPenaltyTimer += Time.deltaTime;

            if (f_MoveSpeedPenaltyTimer > f_MoveSpeedPenaltyTimer_MAX)
                f_MoveSpeedPenaltyTimer = f_MoveSpeedPenaltyTimer_MAX;
        }

        // Otherwise, If player is moving max velocity and just touched down this frame, Apply the Move Speed Penalty and reset the timer.
        if(b_JustTouchedGround)
        {
            f_MovementVelocityPerc = f_MoveSpeedPenaltyPerc;

            f_MoveSpeedPenaltyTimer = 0f;
        }

        #endregion Apply limits to momentum when first touching the ground

        Vector2 v2_InputVector = PlayerInput.GetInputVector();

        #region Update Velocity Direction Percentage

        if (!OnGround && JetpackActive)
            MovementVelocityRate = 3f * Time.deltaTime;
        else
            MovementVelocityRate = 10f * Time.deltaTime * f_MovementVelocityPerc;

        v2_InputVector.Normalize();

        v2_MovementVelocityPercentage = MovementVelocityPerc(v2_MovementVelocityPercentage, v2_InputVector);

        #endregion Update Velocity Direction Percentage

        Vector3 v3_PlayerInput = new Vector3(v2_MovementVelocityPercentage.x, 0, v2_MovementVelocityPercentage.y);

        // Default on-ground player input velocity conversion
        v3_PlayerInput = Vector3.ProjectOnPlane(v3_PlayerInput, -v3_GroundNormal);
        Debug.DrawRay(gameObject.transform.position, v3_PlayerInput * 5.0f, Color.red);
        Vector3 playerVel = gameObject.transform.rotation * v3_PlayerInput;

        // Push player slightly downward, 'snapping' them upon the ground.
        if (OnGround)
            this_CharController.Move(-Vector3.up * 0.25f);

        #region Don't touch this order
        // If player is in the air and using jetpack, reduce directional velocity
        if (!OnGround && JetpackActive)
            playerVel *= f_JetpackAirborneVelocityModifier;

        playerVel *= MoveSpeed * f_MovementVelocityPerc;
        playerVel += yVel * Vector3.up;

        Debug.DrawRay(gameObject.transform.position, JetpackVector_Prior * 5.0f, Color.orangeRed);

        // LERP from previous direction into new desired direction IF using Jetpack and looking in new direction
        if(!OnGround && JetpackActive)
        {
            // TODO: Concern - I'm dealing with desired direction in 3 axis (Y) but potentially evaluating against no Y axis vel from last frame. I think?
            playerVel = Vector3.Lerp(JetpackVector_Prior * JetpackMagnitude_Prior, playerVel, 2.50f * Time.deltaTime);
        }

        // OVERRIDE values if player isn't using Jetpack and isn't on the ground. (That's why this goes below above settings)
        if(!OnGround && !JetpackActive)
        {
            playerVel = v3_LastFrameVelocity;
            playerVel.y = yVel;
        }
        #endregion Don't touch this order

        v3_LastFrameVelocity = playerVel;

        this_CharController.Move(playerVel * Time.deltaTime);
    }

    
    float cameraAngle;
    void UpdateLook()
    {
        Vector2 v2_LookVector = PlayerInput.GetLookVector();

        if (v2_LookVector == new Vector2())
            return;

        if(v2_LookVector.x != 0f)
        {
            Vector3 v3_PlayerDirection = this_CharController.transform.localEulerAngles;
            v3_PlayerDirection.y += v2_LookVector.x * HorizontalLookMultiplier;
            this_CharController.transform.localEulerAngles = v3_PlayerDirection;
        }
        
        if(v2_LookVector.y != 0f)
        {
            // print(v2_LookVector);
            cameraAngle -= v2_LookVector.y * VerticalLookMultiplier;
            cameraAngle = Mathf.Clamp(cameraAngle, -44f, 44f);
            this_CameraObject.transform.localEulerAngles = new Vector3(cameraAngle, 0f, 0f);
        }
    }

    void UpdateQuitButton()
    {
        QuitPressed = PlayerInput.QuitButton();
    }

    void ToggleCursorState(bool IsVisible)
    {
        if (!IsVisible)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = IsVisible;
    }

    #endregion Update Functions

    #region Physics Updates

    private void FixedUpdate()
    {
        
    }

    bool b_JumpDelayActive;
    private IEnumerator JumpDelay()
    {
        b_JumpDelayActive = true;
        print("On");

        yield return new WaitForSeconds(0.1f);

        print("Off");

        b_JumpDelayActive = false;

        yield return null;
    }

    #endregion Physics Updates

    #region Math Functions


    Vector2 MovementVelocityPerc(Vector2 VelPercValue, Vector2 PlayerInputValue)
    {
        Vector2 output = new Vector2();

        output.x = MovementVelocityPerc(VelPercValue.x, PlayerInputValue.x);
        output.y = MovementVelocityPerc(VelPercValue.y, PlayerInputValue.y);

        return output;
    }
    float MovementVelocityPerc(float VelPercValue, float PlayerInputValue)
    {
        if (PlayerInputValue != VelPercValue)
        {
            if (PlayerInputValue < VelPercValue)
            {
                VelPercValue -= MovementVelocityRate;

                if (VelPercValue < PlayerInputValue)
                    VelPercValue = PlayerInputValue;
            }
            else
            {
                VelPercValue += MovementVelocityRate;

                if (VelPercValue > PlayerInputValue)
                    VelPercValue = PlayerInputValue;
            }
        }

        return VelPercValue;
    }

    public float GetMoveSpeed()
    {
        return new Vector2(this_CharController.velocity.x, this_CharController.velocity.z).magnitude;
    }
    #endregion Math Functions

    #region Collider Extensions

    public LayerMask obstacleLayers;
    public Color mtvColor = Color.yellow;
    public bool autoResolve = true;
    public bool smoothResolve = true;

    public event Action<Vector3> OnPenetrationStart;
    public event Action<Vector3> OnPenetrationStay;
    public event System.Action OnPenetrationEnd;

    Collider col;
    Vector3 lastCorrection;
    bool resolvingCollision;

    void ObjectClipStart()
    {
        col = GetComponent<Collider>();

        OnPenetrationStart += correction =>
        {
            float penetrationDepth = correction.magnitude;
        };
    }

    void ObjectClipUpdate()
    {
        bool colliding = col.GetPenetrationsInLater(obstacleLayers, out Vector3 correction);
        correction += correction.normalized * 0.001f;
        lastCorrection = colliding ? correction : Vector3.zero;

        if (colliding)
        {
            if (!resolvingCollision) OnPenetrationStart?.Invoke(correction);
            else OnPenetrationStay?.Invoke(correction);

            resolvingCollision = true;

            if (autoResolve)
            {
                Vector3 delta = smoothResolve
                    ? Vector3.Lerp(Vector3.zero, correction, 0.05f)
                    : correction;

                transform.position += delta;
            }

            Debug.Log($"Colliding, MTV = {correction.magnitude:F3}");
        }
        else
        {
            if (resolvingCollision) OnPenetrationEnd?.Invoke();
            resolvingCollision = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;

        if (lastCorrection != Vector3.zero)
        {
            Vector3 start = col.bounds.center;
            Vector3 end = start + lastCorrection;
            Gizmos.color = mtvColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.05f);
        }
    }

    #endregion
}
